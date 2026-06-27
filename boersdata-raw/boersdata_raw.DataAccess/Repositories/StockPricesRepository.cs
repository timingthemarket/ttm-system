using System.Runtime.CompilerServices;
using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class StockPricesRepository : IStockPricesRepository
{
    private readonly IMemoryCache _cache;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<StockPrice> _defaultCollection;

    public StockPricesRepository(IMongoClient context, IMemoryCache cache)
    {
        _cache = cache;
        _database = context.GetDatabase(MongoDatabaseSettings.BoersdataDatabaseName);
        _defaultCollection = _database.GetCollection<StockPrice>("SecurityPrices");

        if (!IndexExist(_defaultCollection.Indexes, "Ticker#Date"))
        {
            var index = Builders<StockPrice>.IndexKeys
                .Ascending(s => s.Ticker)
                .Ascending(s => s.Date);

            _defaultCollection.Indexes.CreateOne(
                new CreateIndexModel<StockPrice>(index,
                    new CreateIndexOptions { Unique = true, Name = "Ticker#Date" }));
        }
    }

    public async Task<List<StockPrice>> GetHistoricalPrices(string ticker, CancellationToken token = default)
    {
        if (_cache.TryGetValue(MakeCacheKey(ticker), out List<StockPrice> prices))
            return prices;

        return await _defaultCollection.Find(s => s.Ticker == ticker).ToListAsync(token);
    }

    public async Task<List<StockPrice>> GetHistoricalPrices(string ticker, DateTime fromDate,
        CancellationToken token = default)
    {
        return await _defaultCollection.Find(s => s.Ticker == ticker && s.Date >= fromDate)
            .ToListAsync(token);
    }

    public async Task<bool> SavePrice(StockPrice price, CancellationToken token = default)
    {
        var replaced = await _defaultCollection
            .ReplaceOneAsync(s => s.InsId == price.InsId && s.Date == price.Date, price,
                new ReplaceOptions { IsUpsert = true }, token);
        return replaced.IsAcknowledged;
    }
    
    public async Task<int> SaveBatch(List<StockPrice> prices, CancellationToken token = default)
    {
        try
        {
            await _defaultCollection.InsertManyAsync(prices, new InsertManyOptions { IsOrdered = false },
                token);
        }
        catch (MongoBulkWriteException e)
        {
            return e.WriteErrors.Count;
        }

        return 0;
    }
    
    public async Task<int> SaveHistoricalPrices(List<StockPrice> prices, string? ticker, bool useCache = true, CancellationToken token = default)
    {
        if (useCache && ticker is not null)
            _cache.Set(MakeCacheKey(ticker), prices, TimeSpan.FromMinutes(60));

        return await SaveBatch(prices, token);
    }

    private bool IndexExist<T>(IMongoIndexManager<T> indexManager, string indexName)
    {
        var allIndexes = indexManager.List().ToList();
        var indexNames = allIndexes
            .SelectMany(index => index.Elements)
            .Where(element => element.Name == "name")
            .Select(name => name.Value.ToString());

        return indexNames.Contains(indexName);
    }

    /// <summary>
    ///     Delete all prices from a ticker
    /// </summary>
    /// <param name="ticker"></param>
    public async Task DeleteHistoricalPrices(string ticker)
    {
        await _defaultCollection.DeleteManyAsync(s => s.Ticker == ticker);
    }

    /// <summary>
    ///     Delete all items fro the ticker and insert prices
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="prices"></param>
    public async Task<int> OverwriteHistoricalPrices(string ticker, List<StockPrice> prices)
    {
        await DeleteHistoricalPrices(ticker);
        return await SaveHistoricalPrices(prices, ticker, false);
    }

    private string MakeCacheKey(string ticker) => $"HISTORICAL-PRICE_{ticker}";
}