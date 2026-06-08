namespace article_news_raw.Domain.Interfaces;

public interface IFetchNewsUrlsHandler
{
    public string FetcherName { get; }
    Task HandleFetchNewsUrls(DateTime? toDate = null);
}   