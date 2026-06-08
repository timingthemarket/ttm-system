using MassTransit;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Events.BoersDataRaw;

namespace securities_masterdata.Consumers.BoersDataRaw;

public class HistoricalPricesSyncCompleteEventConsumer(
    ILogger<HistoricalPricesSyncCompleteEventConsumer> logger,
    IBackfillSecuritiesPricesHandler backfillSecuritiesPricesHandler) : IConsumer<HistoricalPricesSyncCompleteEvent>
{
    public async Task Consume(ConsumeContext<HistoricalPricesSyncCompleteEvent> context)
    {
        var tickers = context.Message.Tickers;
        if (tickers == null || !tickers.Any())
        {
            logger.LogInformation("Starting to backfill ALL securitites prices");
            await backfillSecuritiesPricesHandler.HandleBackfillSecuritiesPrices();
        }
        else
        {
            logger.LogInformation("Starting to backfill {Count} securitites prices", tickers.Count);
            await backfillSecuritiesPricesHandler.HandleBackfillSecurityPrices(tickers);
        }
    }
}