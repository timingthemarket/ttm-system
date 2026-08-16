namespace article_news_raw.Domain.Interfaces;

/// <summary>
/// One implementation per market data source. Register them in
/// <c>DiContainer.AddCustomServices</c> and <c>FetchMarketDataHandler</c> picks them all up.
/// </summary>
public interface IFetchMarketDataHandler
{
    public string FetcherName { get; }

    /// <returns>The number of data points stored.</returns>
    Task<int> HandleFetchMarketData(CancellationToken token = default);
}
