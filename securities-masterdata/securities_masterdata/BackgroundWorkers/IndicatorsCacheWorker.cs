using System.Diagnostics;
using securities_masterdata.DataAccess.Cache;
using securities_masterdata.DataAccess.Interfaces;

namespace securities_masterdata.BackgroundWorkers;

public class IndicatorsCacheWorker(ILogger<IndicatorsCacheWorker> logger, IServiceProvider serviceProvider, IndicatorsCache indicatorsCache) : BackgroundService
{
    private bool _hasRunFirstTime;
    private DateTime? LastIndicatorsSync { get; set; }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_hasRunFirstTime)
            {
                logger.LogInformation("Starting up IndicatorsCacheWorker...");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                _hasRunFirstTime = true;
            }
            
            if (LastIndicatorsSync == null || LastIndicatorsSync.Value < DateTime.UtcNow.AddHours(-24))
            {
                try
                {
                    await UpdateIndicatorsAsync();
                }
                finally
                {
                    LastIndicatorsSync = DateTime.UtcNow;
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task UpdateIndicatorsAsync()
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        var securityRepository = scope.ServiceProvider.GetRequiredService<ISecurityRepository>();
        var indicatorsRepository = scope.ServiceProvider.GetRequiredService<IIndicatorsRepository>();

        logger.LogInformation("Updating indicators cache");
        
        var sw = new Stopwatch();
        sw.Start();
        
        var securities = await securityRepository.GetAll();

        var processed = 0;
        foreach (var ids in securities.Select(s => s.SecurityId).Chunk(50))
        {
            var maxDatePast = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));
            var indicators = await GetIndicatorsHistoryNoCache(indicatorsRepository, ids.ToHashSet(), maxDatePast);

            processed += indicatorsCache.UpdateCache(indicators);
            Console.WriteLine($"Nr securities indicators processed {processed}/{securities.Count}");
        }

        sw.Stop();
        logger.LogInformation("Indicators cache update done: {Elapsed}", sw.Elapsed);
        logger.LogInformation("Nr securities indicators processed {Total}", securities.Count);
    }

    private async Task<List<DataAccess.Entities.Indicator>> GetIndicatorsHistoryNoCache(IIndicatorsRepository repository, HashSet<long> securityIds, DateOnly fromDate)
    {
        // Since IndicatorsRepository doesn't have a direct method to get all indicators by security IDs and date range,
        // we'll need to use the existing GetIndicatorsByDate method with all indicator types
        var allIndicatorIds = Enum.GetValues<TTM.Shared.Constants.Indicators>()
            .Select(i => (long)i)
            .ToHashSet();

        return await repository.GetIndicatorsByDate(fromDate, allIndicatorIds, securityIds);
    }
}