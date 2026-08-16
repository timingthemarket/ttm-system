using article_news_raw.DataAccess.InternalEvents;
using article_news_raw.Domain.Handlers;
using MassTransit;

namespace article_news_raw.Triggers;

public class FetchMarketDataTrigger(FetchMarketDataHandler fetchMarketDataHandler)
    : IConsumer<FetchMarketDataTriggerEvent>
{
    public async Task Consume(ConsumeContext<FetchMarketDataTriggerEvent> context)
    {
        await fetchMarketDataHandler.FetchMarketData();
    }
}
