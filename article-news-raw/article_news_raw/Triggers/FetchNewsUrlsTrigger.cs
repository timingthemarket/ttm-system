using article_news_raw.DataAccess.InternalEvents;
using article_news_raw.Domain.Handlers;
using article_news_raw.Domain.Handlers.FetchNews;
using MassTransit;

namespace article_news_raw.Triggers;

public class FetchNewsUrlsTrigger(FetchNewsUrlsHandler fetchNewsUrlsHandler)
    : IConsumer<FetchNewesUrlsTriggerEvent>
{
    public async Task Consume(ConsumeContext<FetchNewesUrlsTriggerEvent> context)
    {
        await fetchNewsUrlsHandler.FetchNewsUrls();
    }
}
