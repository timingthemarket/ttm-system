using System.Text.Json;
using boersdata_raw;
using boersdata_raw.BackgroundServices;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.Report;
using boersdata_raw.Filters;
using boersdata_raw.gRPC.Services;
using boersdata_raw.Scheduler;
using Hangfire;
using JasperFx;
using Marten;
using Marten.Schema;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Server;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Extensions.Serilog;

//var builder = Host.CreateDefaultBuilder(args);
var builder = WebApplication.CreateBuilder(args);

SharedSettings.AppName = nameof(boersdata_raw);

var connString = Environment.GetEnvironmentVariable("POSTGRESSQL_CONN") ??
                 throw new Exception("The environment variable 'POSTGRESSQL_CONN' was not found");

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(opts =>
{
    opts.Connection(connString);
    //opts.DatabaseSchemaName = "boersdata";
    opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

    // Tickers are only unique within an origin (nordic vs global collections in Mongo)
    opts.Schema.For<Security>()
        .UniqueIndex(UniqueIndexType.Computed, "uidx_security_origin_ticker",
            x => x.Origin, x => x.Ticker)
        .Index(x => x.InsId);

    // Dedupe guard for daily batch inserts; ticker index serves the historical reads
    opts.Schema.For<StockPrice>()
        .UniqueIndex(UniqueIndexType.Computed, "uidx_stock_price_ticker_date",
            x => x.Ticker, x => x.Date)
        .Index(x => x.InsId);

    // Serves GetReports(ticker, type) and delete-by-ticker on re-sync
    opts.Schema.For<Report>()
        .Index(x => x.Ticker);

    opts.Schema.For<Country>()
        .UniqueIndex(UniqueIndexType.Computed, "uidx_country_name", x => x.Name);
    opts.Schema.For<Market>()
        .UniqueIndex(UniqueIndexType.Computed, "uidx_market_name", x => x.Name);
    opts.Schema.For<Sector>()
        .UniqueIndex(UniqueIndexType.Computed, "uidx_sector_name", x => x.Name);
});

builder.Services.AddHangfire(h =>
{
    h.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
    h.UseRecommendedSerializerSettings();
    h.UseInMemoryStorage();
});

// GRPC
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<InterceptorHandler>();
    options.EnableDetailedErrors = true;
});
builder.Services.AddGrpcReflection();
//

builder.Services.AddHangfireServer();
builder.Services.AddSingleton<SetupHangfireJobs>();

const int portHttp1 = 5004;
const int portHttp2 = 5104;
Console.WriteLine($"Port: {portHttp1} Http1, {portHttp2} http2");
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(portHttp1, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });
    options.ListenAnyIP(portHttp2, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
});

builder.Services.AddHttpClient(Options.DefaultName);

builder.Services.AddHealthChecks();

builder.Services.AddCustomServices();

var oltpEndpoint = Environment.GetEnvironmentVariable("OLT_ENDPOINT") ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddTtmMetrics(oltpEndpoint, SharedSettings.AppName))
    .WithTracing(b => { 
        b.AddTtmTracing(oltpEndpoint, SharedSettings.AppName)
         .AddSource("boersdata_raw.BackgroundServices.DailyPricesService")
         .AddSource("boersdata_raw.BackgroundServices.ReportsService")
         .AddSource("boersdata_raw.BackgroundServices.WeeklyRefreshPricesService");
    });


string infraServiceEndpoint = Environment.GetEnvironmentVariable("INFRA_SERVICE_URL") ?? "http://localhost:4317";
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.WithSpan()
    .WriteTo.Grpc(new GrpcSinkConfiguration
    {
        ServerUrl = infraServiceEndpoint,
        ServiceName = SharedSettings.AppName
    }, LogEventLevel.Information)
    .WriteTo.Console();

var logger = loggerConfiguration.CreateLogger();
Log.Logger = logger;

builder.Logging
    .ClearProviders()
    .AddSerilog(logger);

//builder.Services.AddHostedService<DailyPricesService>();
builder.Services.AddHostedService<ReportsService>();
builder.Services.AddHostedService<WeeklyRefreshPricesService>();


builder.Services.AddCodeFirstGrpc();
// Builder //

var app = builder.Build();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionLoggerMiddleware>();

app.MapGrpcService<BackfillService>();

app.MapControllers();

var scope = app.Services.CreateScope();
var hfJobs = scope.ServiceProvider.GetRequiredService<SetupHangfireJobs>();
hfJobs.SetupJobs();

app.Run();