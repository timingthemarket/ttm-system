using System.Diagnostics;
using securities_masterdata.DataAccess.Cache;
using securities_masterdata.DataAccess.Interfaces;

namespace securities_masterdata.BackgroundWorkers;

public class PriceCacheWorker(ILogger<PriceCacheWorker> logger, IServiceProvider serviceProvider, SecuritiesPricesCache pricesCache) : BackgroundService
{
    private static readonly DayOfWeek RunDay = DayOfWeek.Saturday;
    private static readonly TimeSpan RunTimeUtc = new(23, 0, 0);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (pricesCache.IsEmpty)
            {
                logger.LogInformation("Price cache is empty, running update immediately");
                await UpdatePricesAsync();
            }

            var delay = GetDelayUntilNextRun(DateTime.UtcNow);
            logger.LogInformation("PriceCacheWorker next run at {NextRun:u}", DateTime.UtcNow + delay);
            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            await UpdatePricesAsync();
        }
    }

    internal static TimeSpan GetDelayUntilNextRun(DateTime nowUtc)
    {
        var nextRun = nowUtc.Date + RunTimeUtc;
        var daysToAdd = ((int)RunDay - (int)nowUtc.DayOfWeek + 7) % 7;
        nextRun = nextRun.AddDays(daysToAdd);

        if (nextRun <= nowUtc) nextRun = nextRun.AddDays(7);

        return nextRun - nowUtc;
    }

    private async Task UpdatePricesAsync()
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        var securityRepository = scope.ServiceProvider.GetRequiredService<ISecurityRepository>();

        logger.LogInformation("Updating price cache");
        
        var sw = new Stopwatch();
        sw.Start();
        
        var securities = await securityRepository.GetAll();

        var processed = 0;
        foreach (var ids in securities.Select(s => s.SecurityId).Chunk(100))
        {
            var maxDatePast = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));
            var prices = await securityRepository.GetSecuritiesPricesHistoryNoCache(ids.ToHashSet(), maxDatePast);

            processed += pricesCache.UpdateCache(prices);
            Console.WriteLine($"Nr securities processed {processed}/{securities.Count}");
        }

        sw.Stop();
        logger.LogInformation("Price cache update done: {Elapsed}", sw.Elapsed);
        logger.LogInformation("Nr securities processed {Total}", securities.Count);
    }
}