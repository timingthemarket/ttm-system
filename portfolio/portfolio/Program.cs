using System.Text.Json;
using FluentMigrator.Runner;
using Hangfire;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using portfolio.BackgroundServices;
using portfolio.DataAccess;
using portfolio.DataAccess.Constants;
using portfolio.Domain;
using portfolio.Domain.Handlers;
using portfolio.Domain.Interfaces;
using portfolio.Filters;
using portfolio.Scheduler;
using Serilog.Events;
using Serilog.Enrichers.Span;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Filters;
using TTM.Shared.Models.SecuritiesMasterdata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var connString = Environment.GetEnvironmentVariable("POSTGRESSQL_CONN") ??
                 throw new Exception("The environment variable 'POSTGRESSQL_CONN' was not found");

Configuration.DbConString = connString;
SharedSettings.AppName = nameof(portfolio);

builder.Services.AddDbContext<PortfolioDbContext>();

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(Configuration.DbConString)
        .ScanIn(typeof(Program).Assembly).For.Migrations());

builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const int port = 5006;
builder.WebHost.UseKestrel((context, options) =>
{
    options.ListenAnyIP(port, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });
});

var isProd = builder.Environment.IsProduction();
if (isProd)
{
    //builder.Services.AddHostedService<SimulationService>();
    //builder.Services.AddHostedService<PortfolioOutcomeViewRefreshBackgroundService>();
    //builder.Services.AddHostedService<HistoricalExplorerBackgroundService>();
    //builder.Services.AddHostedService<PortfolioTrendsBackgroundService>();
}

builder.Services.AddHealthChecks();

var oltpEndpoint = Environment.GetEnvironmentVariable("OLT_ENDPOINT") ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddTtmMetrics(oltpEndpoint, SharedSettings.AppName))
    .WithTracing(b => { b.AddTtmTracing(oltpEndpoint, SharedSettings.AppName); });

builder.Logging
    .ClearProviders()
    .AddTtmOtelLogger(oltpEndpoint, SharedSettings.AppName,
        c => { 
            c.MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);
            c.Enrich.WithSpan();
        });

builder.Services.AddHangfireServer();
builder.Services.AddHangfire(h =>
{
    h.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
    h.UseRecommendedSerializerSettings();
    h.UseInMemoryStorage();
});

// Custom DI injections
builder.Services.AddCustomServices();

builder.Services.AddScoped<SetupHangfireJobs>();

//-----------//

var localhostAddress = Environment.GetEnvironmentVariable("DOCKER_RABBITMQ_ACCESS") ?? "localhost";
builder.Services.AddMassTransit(x =>
{
    x.AddRequestClient<SecuritiesIndicatorsQry>(RequestTimeout.After(s: 60));
    
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseConsumeFilter(typeof(ExceptionFilter<>), context);
        
        cfg.UseMessageRetry(r => r.Immediate(2));
        cfg.Host(localhostAddress, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// Builder //
var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseRouting();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionLoggerMiddleware>();

app.MapControllers();

var scope = app.Services.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

var hfJobs = scope.ServiceProvider.GetRequiredService<SetupHangfireJobs>();
hfJobs.SetupJobs();


app.Run();