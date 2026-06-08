using System.Diagnostics;
using securities_masterdata.DataAccess.Cache;
using securities_masterdata.DataAccess.Interfaces;

namespace securities_masterdata.BackgroundWorkers;

public class PriceCacheWorker(ILogger<PriceCacheWorker> logger, IServiceProvider serviceProvider, SecuritiesPricesCache pricesCache) : BackgroundService
{
    private bool _hasRunFirstTime;
    private DateTime? LastPriceSync { get; set; }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_hasRunFirstTime)
            {
                logger.LogInformation("Starting up PriceCacheWorker...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                _hasRunFirstTime = true;
            }
            
            if (LastPriceSync == null || LastPriceSync.Value < DateTime.UtcNow.AddHours(-24))
            {
                try
                {
                    await UpdatePricesAsync();
                }
                finally
                {
                    LastPriceSync = DateTime.UtcNow;
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
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