using System.Text.Json;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.ClientFactory;
using ProtoBuf.Grpc.Server;
using securities_masterdata;
using securities_masterdata.BackgroundWorkers;
using securities_masterdata.DataAccess;
using securities_masterdata.Filters;
using securities_masterdata.gRPC.Services;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Extensions.Serilog;
using TTM.Shared.gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

//Logger
string? hostname = Environment.GetEnvironmentVariable("HOSTNAME");
SharedSettings.AppName = $"{nameof(securities_masterdata)}_{hostname}";

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var connString = Environment.GetEnvironmentVariable("POSTGRESSQL_CONN") ??
                 throw new Exception("The environment variable 'POSTGRESSQL_CONN' was not found");
builder.Services.AddDbContext<MasterdataDbContext>(options =>
{
    options.EnableSensitiveDataLogging();
    options.UseNpgsql(connString, o => { 
        o.EnableRetryOnFailure( );
        o.CommandTimeout(60);
    });
}, ServiceLifetime.Transient);

if (builder.Environment.IsProduction())
{
    builder.Services.AddHostedService<PriceCacheWorker>();
    builder.Services.AddHostedService<IndicatorsCacheWorker>();
}

builder.Services.AddHostedService<BackfillReportsWorker>();

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connString)
        .ScanIn(typeof(DiContainer).Assembly).For.Migrations());

builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

int port = builder.Environment.IsDevelopment() ? 5011 : 5001;
int portHttp2 = 5101;
Console.WriteLine($"Hosted on http://localhost:{port}");
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(port, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });
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


builder.Services.AddCustomServices();

// GRPC Clients
var boersDataUrl = Environment.GetEnvironmentVariable("BOERSDATA_URL") ?? "http://localhost:5104";

GrpcClientFactory.AllowUnencryptedHttp2 = true;
builder.Services.AddCodeFirstGrpcClient<IBackfillService>(o =>
{
    o.Address = new Uri(boersDataUrl);
});

// gRPC Services
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<InterceptorHandler>();
    options.EnableDetailedErrors = true;
});

builder.Services.AddCodeFirstGrpc();

// Builder //
var app = builder.Build();

app.UseRouting();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<ExceptionLoggerMiddleware>();

app.MapGrpcService<MasterdataService>();

app.MapControllers();

var scope = app.Services.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

app.Run();