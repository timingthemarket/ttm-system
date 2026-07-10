using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class StockPricesRepository : IStockPricesRepository
{
    private readonly IMemoryCache _cache;

    public StockPricesRepository(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<List<StockPrice>> GetHistoricalPrices(string ticker, CancellationToken token = default)
    {
        if (_cache.TryGetValue(MakeCacheKey(ticker), out List<StockPrice>? prices))
            return prices!;

        await using var context = new BoersDataDbContext();
        return await context.StockPrices.AsNoTracking()
            .Where(s => s.Ticker == ticker)
            .OrderBy(s => s.Date)
            .ToListAsync(token);
    }

    public async Task<List<StockPrice>> GetHistoricalPrices(string ticker, DateTime fromDate,
        CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        var from = AsUtc(fromDate);
        return await context.StockPrices.AsNoTracking()
            .Where(s => s.Ticker == ticker && s.Date >= from)
            .OrderBy(s => s.Date)
            .ToListAsync(token);
    }

    public async Task<bool> SavePrice(StockPrice price, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        var date = AsUtc(price.Date);

        var affected = await context.Database.ExecuteSqlAsync($"""
            INSERT INTO stock_price (ins_id, ticker, open, close, high, low, volume, date)
            VALUES ({price.InsId}, {price.Ticker}, {price.Open}, {price.Close}, {price.High}, {price.Low}, {price.Volume}, {date})
            ON CONFLICT (ins_id, date) DO UPDATE SET
                ticker = EXCLUDED.ticker,
                open = EXCLUDED.open,
                close = EXCLUDED.close,
                high = EXCLUDED.high,
                low = EXCLUDED.low,
                volume = EXCLUDED.volume
            """, token);

        return affected > 0;
    }

    public async Task<int> SaveBatch(List<StockPrice> prices, CancellationToken token = default)
    {
        if (prices.Count == 0)
            return 0;

        await using var context = new BoersDataDbContext();
        return await SaveBatchCore(context, prices, token);
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
        await using var context = new BoersDataDbContext();
        await context.StockPrices.Where(s => s.Ticker == ticker).ExecuteDeleteAsync();
    }

    /// <summary>
    ///     Delete all items fro the ticker and insert prices
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="prices"></param>
    public async Task<int> OverwriteHistoricalPrices(string ticker, List<StockPrice> prices)
    {
        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        await context.StockPrices.Where(s => s.Ticker == ticker).ExecuteDeleteAsync();
        var duplicates = prices.Count == 0 ? 0 : await SaveBatchCore(context, prices);

        await transaction.CommitAsync();
        return duplicates;
    }

    /// <summary>
    ///     Single multi-row insert; ON CONFLICT DO NOTHING drops duplicates on either
    ///     unique index, matching Mongo's unordered InsertMany. Returns the duplicate count.
    /// </summary>
    private static async Task<int> SaveBatchCore(BoersDataDbContext context, List<StockPrice> prices,
        CancellationToken token = default)
    {
        const string sql = """
            INSERT INTO stock_price (ins_id, ticker, open, close, high, low, volume, date)
            SELECT * FROM unnest(
                @ins_ids::bigint[], @tickers::text[], @opens::float8[], @closes::float8[],
                @highs::float8[], @lows::float8[], @volumes::bigint[], @dates::timestamptz[])
            ON CONFLICT DO NOTHING
            """;

        var parameters = new object[]
        {
            new NpgsqlParameter("ins_ids", prices.Select(p => p.InsId).ToArray()),
            new NpgsqlParameter("tickers", prices.Select(p => p.Ticker).ToArray()),
            new NpgsqlParameter("opens", prices.Select(p => p.Open).ToArray()),
            new NpgsqlParameter("closes", prices.Select(p => p.Close).ToArray()),
            new NpgsqlParameter("highs", prices.Select(p => p.High).ToArray()),
            new NpgsqlParameter("lows", prices.Select(p => p.Low).ToArray()),
            new NpgsqlParameter("volumes", prices.Select(p => p.Volume).ToArray()),
            new NpgsqlParameter("dates", prices.Select(p => AsUtc(p.Date)).ToArray())
        };

        var inserted = await context.Database.ExecuteSqlRawAsync(sql, parameters, token);
        return prices.Count - inserted;
    }

    private static DateTime AsUtc(DateTime date) =>
        date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc);

    private string MakeCacheKey(string ticker) => $"HISTORICAL-PRICE_{ticker}";
}
