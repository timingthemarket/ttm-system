using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Marten;
using Microsoft.Extensions.Caching.Memory;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class StockPricesRepository : IStockPricesRepository
{
    private readonly IDocumentStore _store;
    private readonly IMemoryCache _cache;

    public StockPricesRepository(IDocumentStore store, IMemoryCache cache)
    {
        _store = store;
        _cache = cache;
    }

    public async Task<List<StockPrice>> GetHistoricalPrices(string ticker, CancellationToken token = default)
    {
        if (_cache.TryGetValue(MakeCacheKey(ticker), out List<StockPrice>? prices))
            return prices!;

        await using var session = _store.QuerySession();
        var result = await session.Query<StockPrice>()
            .Where(s => s.Ticker == ticker)
            .OrderBy(s => s.Date)
            .ToListAsync(token);
        return result.ToList();
    }

    public async Task<List<StockPrice>> GetHistoricalPrices(string ticker, DateTime fromDate,
        CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var from = AsUtc(fromDate);
        var result = await session.Query<StockPrice>()
            .Where(s => s.Ticker == ticker && s.Date >= from)
            .OrderBy(s => s.Date)
            .ToListAsync(token);
        return result.ToList();
    }

    public async Task<bool> SavePrice(StockPrice price, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        price.Date = AsUtc(price.Date);

        var existing = await session.Query<StockPrice>()
            .FirstOrDefaultAsync(s => s.InsId == price.InsId && s.Date == price.Date, token);
        if (existing is not null)
            price.Id = existing.Id;

        session.Store(price);
        await session.SaveChangesAsync(token);
        return true;
    }

    public async Task<int> SaveBatch(List<StockPrice> prices, CancellationToken token = default)
    {
        if (prices.Count == 0)
            return 0;

        await using var session = _store.LightweightSession();
        var duplicates = await StoreNewPrices(session, prices, token);
        await session.SaveChangesAsync(token);
        return duplicates;
    }

    public async Task<int> SaveHistoricalPrices(List<StockPrice> prices, string? ticker, bool useCache = true,
        CancellationToken token = default)
    {
        if (useCache && ticker is not null)
            _cache.Set(MakeCacheKey(ticker), prices, TimeSpan.FromMinutes(60));

        return await SaveBatch(prices, token);
    }

    /// <summary>
    ///     Delete all prices from a ticker
    /// </summary>
    /// <param name="ticker"></param>
    public async Task DeleteHistoricalPrices(string ticker)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<StockPrice>(s => s.Ticker == ticker);
        await session.SaveChangesAsync();
    }

    /// <summary>
    ///     Delete all items fro the ticker and insert prices
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="prices"></param>
    public async Task<int> OverwriteHistoricalPrices(string ticker, List<StockPrice> prices)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<StockPrice>(s => s.Ticker == ticker);
        await session.SaveChangesAsync();

        if (prices.Count == 0)
            return 0;

        var duplicates = DeduplicateAndStore(session, prices, new HashSet<(string, DateTime)>());
        await session.SaveChangesAsync();
        return duplicates;
    }

    /// <summary>
    ///     Skips prices already present (same ticker + date), matching the old Mongo
    ///     unordered-insert behaviour. Returns the duplicate count.
    /// </summary>
    private static async Task<int> StoreNewPrices(IDocumentSession session, List<StockPrice> prices,
        CancellationToken token)
    {
        var tickers = prices.Select(p => p.Ticker).Distinct().ToList();
        var dates = prices.Select(p => AsUtc(p.Date)).Distinct().ToList();

        // Daily sync is many tickers on few dates; per-ticker backfill is the reverse.
        // Query existing keys on whichever axis is narrower.
        var existingQuery = dates.Count <= tickers.Count
            ? session.Query<StockPrice>().Where(p => p.Date.IsOneOf(dates))
            : session.Query<StockPrice>().Where(p => p.Ticker.IsOneOf(tickers));

        var existing = await existingQuery
            .Select(p => new { p.Ticker, p.Date })
            .ToListAsync(token);

        var seenKeys = existing
            .Select(e => (e.Ticker, AsUtc(e.Date)))
            .ToHashSet();

        return DeduplicateAndStore(session, prices, seenKeys);
    }

    private static int DeduplicateAndStore(IDocumentSession session, List<StockPrice> prices,
        HashSet<(string, DateTime)> seenKeys)
    {
        var duplicates = 0;
        foreach (var price in prices)
        {
            price.Date = AsUtc(price.Date);
            if (!seenKeys.Add((price.Ticker, price.Date)))
            {
                duplicates++;
                continue;
            }

            session.Store(price);
        }

        return duplicates;
    }

    private static DateTime AsUtc(DateTime date) =>
        date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc);

    private string MakeCacheKey(string ticker) => $"HISTORICAL-PRICE_{ticker}";
}
