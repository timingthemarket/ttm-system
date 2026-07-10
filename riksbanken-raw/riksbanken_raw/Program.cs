using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using riksbanken_raw;
using riksbanken_raw.Filters;
using riksbanken_raw.Scheduler;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

SharedSettings.AppName = nameof(riksbanken_raw);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var mongoClientString = Environment.GetEnvironmentVariable("MONGODB_CONN_STRING") ?? "";
var settings = MongoClientSettings.FromConnectionString(mongoClientString);
settings.ConnectTimeout = TimeSpan.FromSeconds(10);
var mongoClient = new MongoClient(settings);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddHangfire(h =>
{
    h.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    h.UseRecommendedSerializerSettings();
    h.UseInMemoryStorage();
});

builder.Services.AddHangfireServer();
builder.Services.AddSingleton<SetupHangfireJobs>();

const int port = 5005;
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(port, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });
});

builder.Services.AddHttpClient(Options.DefaultName);

builder.Services.AddHealthChecks();

builder.Services.AddCustomServices();


var oltpEndpoint = Environment.GetEnvironmentVariable("OLT_ENDPOINT") ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddTtmMetrics(oltpEndpoint, SharedSettings.AppName))
    .WithTracing(b => { b.AddTtmTracing(oltpEndpoint, SharedSettings.AppName); });

builder.Logging
    .ClearProviders()
    .AddTtmOtelLogger(oltpEndpoint, SharedSettings.AppName)
    .AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);


// Builder //
var app = builder.Build();
app.UseRouting();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionLoggerMiddleware>();

app.MapControllers();

var scope = app.Services.CreateScope();
var hfJobs = scope.ServiceProvider.GetRequiredService<SetupHangfireJobs>();
hfJobs.SetupJobs();

app.Run();