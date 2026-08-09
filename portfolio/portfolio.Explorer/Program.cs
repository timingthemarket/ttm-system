using System.Diagnostics;
using MassTransit;
using OpenTelemetry.Trace;
using portfolio.DataAccess;
using portfolio.DataAccess.Constants;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using portfolio.Domain;
using portfolio.Domain.Handlers;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using portfolio.Domain.Services;
using portfolio.Domain.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Enrichers.Span;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Extensions.Serilog;
using TTM.Shared.Filters;
using ILogger = Microsoft.Extensions.Logging.ILogger;

string connString = Environment.GetEnvironmentVariable("POSTGRESSQL_CONN") ??
                    throw new Exception("The environment variable 'POSTGRESSQL_CONN' was not found");

Configuration.DbConString = connString;
string? hostname = Environment.GetEnvironmentVariable("HOSTNAME");
SharedSettings.AppName = $"portfolio.Explorer_{hostname}";

// Setup services
var services = new ServiceCollection();

services.AddScoped<SessionDateHandler>();
services.AddDbContext<PortfolioDbContext>();
services.AddCustomServices();

string localhostAddress = Environment.GetEnvironmentVariable("DOCKER_RABBITMQ_ACCESS") ?? "localhost";
services.AddMassTransit(x =>
{
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
    x.AddOptions<MassTransitHostOptions>()
        .Configure(options => { options.WaitUntilStarted = true; });
});

string oltpEndpoint = Environment.GetEnvironmentVariable("OLT_ENDPOINT") ?? "http://localhost:4317";

services.AddOpenTelemetry()
    .WithTracing(b =>
    {
        b.AddTtmTracing(oltpEndpoint, SharedSettings.AppName);
        b.AddSource("portfolio.Explorer");
    });

// Setup logging
string infraServiceEndpoint = Environment.GetEnvironmentVariable("INFRA_SERVICE_URL") ?? "http://localhost:4317";
LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .Enrich.WithSpan()
    .WriteTo.Grpc(new GrpcSinkConfiguration
    {
        ServerUrl = infraServiceEndpoint,
        ServiceName = SharedSettings.AppName
    }, LogEventLevel.Information)
    .WriteTo.Console();

var logger = loggerConfiguration.CreateLogger();
Log.Logger = logger;

services.AddLogging(builder => builder.AddSerilog(logger));

// Build service provider
ServiceProvider serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
ILogger explorerLogger = loggerFactory.CreateLogger("PortfolioExplorer");

// Force OpenTelemetry tracing to start by getting the TracerProvider
var tracerProvider = serviceProvider.GetRequiredService<TracerProvider>();
explorerLogger.LogInformation("TracerProvider initialized: {TracerProvider}", tracerProvider.GetType().Name);

// Alternative run mode: score each supported indicator on its own instead of searching indicator
// combinations. One shot - it runs the backtest and exits, which is what the monthly schedule needs.
// This has to come before INDICATOR_SEARCH_STRATEGY is parsed, since that throws on unknown values.
var explorerMode = Environment.GetEnvironmentVariable("EXPLORER_MODE") ?? "Search";
if (explorerMode == "IndicatorStrength")
{
    int backfillYears = int.Parse(Environment.GetEnvironmentVariable("STRENGTH_BACKFILL_YEARS") ?? "12");
    explorerLogger.LogInformation("Running in IndicatorStrength mode with a {BackfillYears} year backfill",
        backfillYears);

    using IServiceScope strengthScope = serviceProvider.CreateScope();
    var indicatorStrengthHandler = strengthScope.ServiceProvider.GetRequiredService<IIndicatorStrengthHandler>();
    await indicatorStrengthHandler.ProcessIndicatorStrength(DateOnly.FromDateTime(DateTime.UtcNow), backfillYears);

    explorerLogger.LogInformation("Indicator strength processing complete. Exiting.");
    await Log.CloseAndFlushAsync();
    return;
}

// Production-ready configuration
int maxIterations = int.Parse(Environment.GetEnvironmentVariable("MAX_ITERATIONS") ?? "1000");
int waitSecondsOnNoSession = int.Parse(Environment.GetEnvironmentVariable("WAIT_SECONDS_NO_SESSION") ?? "30");
int errorRetryDelay = int.Parse(Environment.GetEnvironmentVariable("ERROR_RETRY_DELAY") ?? "60");
var indicatorSearchStrategy = Environment.GetEnvironmentVariable("INDICATOR_SEARCH_STRATEGY") ?? "Random";

IndicatorSearchSpace searchSpace;
List<List<PortfolioInputIndicatorVariable>>? portfolioInputIndicators = null;
if (indicatorSearchStrategy == "Random")
{
    searchSpace = IndicatorSearchSpace.Random;
}
else if (indicatorSearchStrategy == "Start")
{
    searchSpace = IndicatorSearchSpace.Start;
    portfolioInputIndicators = IndicatorCombinationGenerator.GenerateAllIndicatorCombinations();
    maxIterations = portfolioInputIndicators.Count;
}
else if (indicatorSearchStrategy == "End")
{
    searchSpace = IndicatorSearchSpace.End;
    portfolioInputIndicators = IndicatorCombinationGenerator.GenerateAllIndicatorCombinations();
    maxIterations = portfolioInputIndicators.Count; 
}
else
{
    throw new ArgumentException($"Invalid indicator search strategy: {indicatorSearchStrategy}");
}

explorerLogger.LogInformation("Using indicator search strategy: {SearchStrategy}", searchSpace);

var hasRunFirstTime = false;
var iterationCount = 0;

var sessionHashes = new HashSet<string>();
DateOnly? sessionHashesDate = null;
try
{
    explorerLogger.LogInformation("Portfolio Explorer starting with max {MaxIterations} iterations...", maxIterations);
    explorerLogger.LogInformation("Configuration: WaitOnNoSession={WaitSeconds}s, ErrorRetryDelay={ErrorRetryDelay}s",
        waitSecondsOnNoSession, errorRetryDelay);

    while (iterationCount < maxIterations)
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            var simulationRepository = scope.ServiceProvider.GetRequiredService<ISimulationRepository>();
            Session? session = await simulationRepository.GetLatestSession();

            if (session != null)
            {
                using var activitySource = new ActivitySource("portfolio.Explorer");
                using var activity = activitySource.StartActivity("portfolio.Explorer.ExecuteAsync");
                
                if (activity != null)
                {
                    activity.SetTag("session.id", session.Id.ToString());
                    activity.SetTag("session.date", session.SessionDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    explorerLogger.LogWarning("Failed to create activity for session processing");
                }
                
                // Get the portfolio input indicators
                List<PortfolioInputIndicatorVariable> indicators;
                if (searchSpace == IndicatorSearchSpace.Random)
                {
                    indicators = IndicatorCombinationGenerator.GenerateIndicators();
                }
                else if (searchSpace == IndicatorSearchSpace.Start)
                {
                    indicators = portfolioInputIndicators[iterationCount];
                }
                else if (searchSpace == IndicatorSearchSpace.End)
                {
                    var indexBackwards = iterationCount + 1;
                    indicators = portfolioInputIndicators[^indexBackwards];
                }
                else
                {
                    throw new ArgumentException($"Invalid search space: {searchSpace}");
                }

                if (sessionHashesDate == null || sessionHashesDate != session.SessionDate)
                {
                    // If we have a new session date, fetch the portfolio hashes
                    var sessionHashesList = await simulationRepository.GetPortfolioHashesFromSessionDate(session.SessionDate);
                    sessionHashes = new HashSet<string>(sessionHashesList);
                    sessionHashesDate = session.SessionDate;
                }

                var initMoney = 50_000;
                
                // Run the discoverer with the indicator set
                var portfolioExplorerHandler = scope.ServiceProvider.GetRequiredService<IPortfolioExplorerHandler>();
                var hasProcessed = await portfolioExplorerHandler.HandlePortfolioDiscover(session.Id, session.SessionDate, indicators, sessionHashes, initMoney);
                iterationCount++;
                if (!hasProcessed) continue;
                
                if (iterationCount % 1000 == 0 || iterationCount <= 10)
                {
                    explorerLogger.LogInformation(
                        "Progress: {CurrentIteration}/{MaxIterations} iterations completed for session date {SessionDate}",
                        iterationCount, maxIterations, session?.SessionDate);
                }
                else
                {
                    explorerLogger.LogDebug("Completed iteration {CurrentIteration}/{MaxIterations}", iterationCount,
                        maxIterations);
                }
            }
            else
            {
                explorerLogger.LogInformation("No session found. Waiting {Wait} seconds...", waitSecondsOnNoSession);
                await Task.Delay(TimeSpan.FromSeconds(waitSecondsOnNoSession));
            }
        }
        catch (Exception ex)
        {
            explorerLogger.LogError(ex, "Error in portfolio exploration cycle: {Message}", ex.Message);

            explorerLogger.LogWarning("Waiting {ErrorRetryDelay} seconds before retrying due to error...",
                errorRetryDelay);
            await Task.Delay(TimeSpan.FromSeconds(errorRetryDelay));

            // Exit the application on error so Docker can restart it
            Environment.Exit(1);
        }

    explorerLogger.LogInformation("Completed all {MaxIterations} iterations. Portfolio Explorer exiting...",
        maxIterations);
}
catch (Exception ex)
{
    explorerLogger.LogCritical(ex, "Critical error in Portfolio Explorer: {Message}", ex.Message);
    Environment.Exit(1);
}