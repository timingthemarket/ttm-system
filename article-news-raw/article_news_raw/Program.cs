using System.Text.Json;
using article_news_raw;
using article_news_raw.Filters;
using article_news_raw.Scheduler;
using Hangfire;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
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

const int port = 5007;
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(port, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });
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


// Builder //

var app = builder.Build();
app.UseRouting();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionLoggerMiddleware>();

app.MapControllers();

var scope = app.Services.CreateScope();
var hfJobs = scope.ServiceProvider.GetRequiredService<SetupHangfireJobs>();
hfJobs.SetupJobs();

app.Run();