using System.Text.Json;
using Hangfire;
using JasperFx;
using Marten;
using Marten.Schema;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using riksbanken_raw;
using riksbanken_raw.DataAccess.Models;
using riksbanken_raw.DataAccess.Seed;
using riksbanken_raw.Filters;
using riksbanken_raw.Scheduler;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

SharedSettings.AppName = nameof(riksbanken_raw);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var connString = Environment.GetEnvironmentVariable("POSTGRESSQL_CONN") ??
                 throw new Exception("The environment variable 'POSTGRESSQL_CONN' was not found");

builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(opts =>
    {
        opts.Connection(connString);
        opts.DatabaseSchemaName = "riksbanken";
        opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

        opts.Schema.For<CurrencyRate>()
            .UniqueIndex(UniqueIndexType.Computed, "uidx_currency_date_tocode_fromcode",
                x => x.Date, x => x.ToCode, x => x.FromCode);
    })
    .InitializeWith(new SeedExchangeRateSeries());

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