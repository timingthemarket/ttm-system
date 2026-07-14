using System.Diagnostics;
using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.Domain.Interfaces;
using boersdata_raw.Domain.Models;
using boersdata_raw.Domain.Queue;
using MassTransit;
using MassTransit.Contracts.JobService;
using TTM.Shared.Constants;
using TTM.Shared.Events.BoersDataRaw;
using TTM.Shared.Extensions;
using TTM.Shared.Functions;

namespace boersdata_raw.BackgroundServices;

public class WeeklyRefreshPricesService(ILogger<WeeklyRefreshPricesService> logger, 
    IServiceProvider serviceProvider, IQueueCache<WeeklyRefreshPricesQueue> queue)  : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("boersdata_raw.BackgroundServices.WeeklyRefreshPricesService");
    
    private bool _hasRunFirstTime;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = ActivitySource.StartActivity("WeeklyRefreshPricesService.ExecutionLoop");
            
            if (!_hasRunFirstTime)
            {
                logger.LogInformation($"Starting up {nameof(WeeklyRefreshPricesService)}...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                _hasRunFirstTime = true;
                activity?.SetTag("service.startup", "true");
            }
            
            using var scope = serviceProvider.CreateScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            
            try
            {
                var queuedItem = queue.DequeueAndGetItem();
                activity?.SetTag("queue.has_item", queuedItem != null);
                
                if (queuedItem != null)
                {
                    using var processActivity = ActivitySource.StartActivity("WeeklyRefreshPricesService.ProcessQueueItem");
                    
                    logger.LogInformation("Dequeued item from 'WeeklyRefreshPricesQueue'");

                    // resolving dependencies
                    var pricesRepository =
                        scope.ServiceProvider.GetRequiredService<IStockPricesRepository>();
                    
                    var securitiesRepository =
                        scope.ServiceProvider.GetRequiredService<ISecuritiesRepository>();
                    
                    var securitiesReportsHandler =
                        scope.ServiceProvider.GetRequiredService<ISyncSecuritiesHistoricalPricesHandler>();

                    // ---------------------
                    
                    using var securitiesActivity = ActivitySource.StartActivity("WeeklyRefreshPricesService.GetAllSecurities");
                    var securities = await securitiesRepository.GetAllSecurities(null, stoppingToken);
                    securitiesActivity?.SetTag("securities.count", securities.Count);
                    
                    var securityChunks = securities.Chunk(5).ToArray();
                    processActivity?.SetTag("security_chunks.count", securityChunks.Length);
                    
                    foreach (var (securityChunk, chunkIndex) in securityChunks.Select((chunk, index) => (chunk, index)))
                    {
                        using var chunkActivity = ActivitySource.StartActivity("WeeklyRefreshPricesService.ProcessSecurityChunk");
                        chunkActivity?.SetTag("chunk.index", chunkIndex);
                        chunkActivity?.SetTag("chunk.size", securityChunk.Length);
                        
                        var tickers = await GetSecuritiesToPerformHistoricalRefresh(securityChunk, pricesRepository);
                        chunkActivity?.SetTag("tickers_to_refresh.count", tickers.Count);

                        if (tickers.Count > 0)
                        {
                            logger.LogInformation("Refreshing historical prices for {Tickers}", string.Join("|", tickers));
                            chunkActivity?.SetTag("tickers_to_refresh.list", string.Join(",", tickers));

                            await securitiesReportsHandler.HandleSelectedSyncHistoricalPrices(tickers);
                            
                            await publishEndpoint.Publish(new HistoricalPricesSyncCompleteEvent
                            {
                                Tickers = tickers
                            }, stoppingToken);
                            chunkActivity?.SetTag("event.type", "HistoricalPricesSyncCompleteEvent");
                            chunkActivity?.SetTag("tickers.count", tickers.Count);
                            
                            logger.LogInformation("Historical prices synced!");
                        }
                    }

                    logger.LogInformation("Refresh prices done!");
                }
                else
                {
                    var waitTs = TimeSpan.FromSeconds(5);
                    await Task.Delay(waitTs, stoppingToken);
                    activity?.SetTag("queue.wait_seconds", 5);
                }
            }
            catch (TaskCanceledException e)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Task cancelled");
                logger.LogError(e, "Task cancelled");
            }
            catch (Exception e)
            {
                activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                await publishEndpoint.SendSystemError(e, SharedSettings.AppName);
                logger.LogError(e, "Unhandled exception occured");
            }
        }
    }

    private async Task<List<string>> GetSecuritiesToPerformHistoricalRefresh(Security[] securities,
        IStockPricesRepository pricesRepository)
    {
        const double threshold = 0.3;
        
        var date = DateTime.UtcNow.AddDays(-7);
        var pricesFetchTask = securities.Select(s => pricesRepository.GetHistoricalPrices(s.Ticker, date)).ToList();

        var allPrices = await Task.WhenAll(pricesFetchTask);
        
        var groupedSecurities = allPrices.SelectMany(p => p)
            .GroupBy(p => p.Ticker);

        var tickersToRefetch = new List<string>();
        foreach (var securitiyWithPrices in groupedSecurities)
        {
            var ticker = securitiyWithPrices.Key;
            
            var prices = securitiyWithPrices.OrderByDescending(s => s.Date).ToList();
            if (prices.Count < 2)
            {
                tickersToRefetch.Add(ticker);
                continue;
            }
            
            var latestPrice = prices.First();
            var secondLatestPrice = prices.Skip(1).First();

            if (latestPrice.Close == null || secondLatestPrice.Close == null)
            {
                tickersToRefetch.Add(ticker);
                continue;
            }
            
            var fraction = SharedFunctions.CalculateFraction(latestPrice.Close.Value, secondLatestPrice.Close.Value);
            // if the price has changed more than the threshold then we need to refetch
            if (Math.Abs(fraction) > threshold) 
            {
                tickersToRefetch.Add(ticker);
            }
        }
        
        return tickersToRefetch.Distinct().ToList();
    }
}