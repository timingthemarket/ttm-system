using System.Text.Json;
using article_news_raw;
using article_news_raw.Filters;
using article_news_raw.gRPC.Services;
using article_news_raw.Scheduler;
using FluentMigrator.Runner;
using Hangfire;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using ProtoBuf.Grpc.Server;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);

SharedSettings.AppName = nameof(article_news_raw);


builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(h =>
{
    h.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    h.UseRecommendedSerializerSettings();
    h.UseInMemoryStorage();
});

builder.Services.AddHangfireServer();
builder.Services.AddScoped<SetupHangfireJobs>();

const int portHttp1 = 5007;
const int portHttp2 = 5107;
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(portHttp1, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });
    options.ListenAnyIP(portHttp2, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
});

builder.Services.AddHttpClient(Options.DefaultName);

builder.Services.AddHealthChecks();

var oltpEndpoint = Environment.GetEnvironmentVariable("OLT_ENDPOINT") ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .WithTracing(b => { b.AddTtmTracing(oltpEndpoint, SharedSettings.AppName); });

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

builder.Services.AddCustomServices(builder.Environment);

builder.Services.AddGrpc(options => { options.EnableDetailedErrors = true; });
builder.Services.AddCodeFirstGrpc();

// Builder //

var app = builder.Build();
app.UseRouting();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionLoggerMiddleware>();

app.MapGrpcService<ArticleNewsService>();
app.MapGrpcService<MarketDataService>();

app.MapControllers();

var scope = app.Services.CreateScope();

var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

var hfJobs = scope.ServiceProvider.GetRequiredService<SetupHangfireJobs>();
hfJobs.SetupJobs();

app.Run();