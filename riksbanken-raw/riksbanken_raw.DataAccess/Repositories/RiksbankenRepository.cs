using MongoDB.Driver;
using riksbanken_raw.DataAccess.Interfaces;
using riksbanken_raw.DataAccess.Models;

namespace riksbanken_raw.DataAccess.Repositories;

public class RiksbankenRepository : IRiksbankenRepository
{
    private readonly IMongoDatabase database;
    private readonly IMongoCollection<ExchangeRateSeries> _ratesCollection;
    private readonly IMongoCollection<CurrencyRate> _currencyCollection;
    
    public RiksbankenRepository(IMongoClient context)
    {
        database = context.GetDatabase(MongoDatabaseSettings.CurrenciesDatabaseName);
        _ratesCollection = database.GetCollection<ExchangeRateSeries>("ExchangeRateSeries");
        _currencyCollection = database.GetCollection<CurrencyRate>("Currency");

        if (!IndexExist(_currencyCollection.Indexes, "Date#FromCode#ToCode"))
        {
            var index = Builders<CurrencyRate>.IndexKeys
                .Ascending(s => s.Date)
                .Ascending(s => s.ToCode)
                .Ascending(s => s.FromCode);

            _currencyCollection.Indexes.CreateOne(
                new CreateIndexModel<CurrencyRate>(index,
                    new CreateIndexOptions { Unique = true, Name = "Date#FromCode#ToCode" }));
        }
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

    public async Task<List<ExchangeRateSeries>> GetExchangeRateSeries()
    {
        return await _ratesCollection.Find(_ => true).ToListAsync();
    }
    
    public async Task<bool> UpdateLatestFetchedDate(string seriesId, DateTime latestDate)
    {
        var filter = Builders<ExchangeRateSeries>.Filter
            .Eq(serie => serie.SeriesId, seriesId);
        var update = Builders<ExchangeRateSeries>.Update
            .Set(serie => serie.LastFetched, latestDate);
        
        var result = await _ratesCollection.UpdateOneAsync(filter, update);
        return result.IsAcknowledged;
    }

    public async Task<List<CurrencyRate>> GetCurrenciesFromCode(string code)
    {
        return await _currencyCollection.Find(c => c.FromCode == code).ToListAsync();
    }

    public async Task<bool> SaveCurrency(CurrencyRate cur)
    {
        var replaced = await _currencyCollection
            .ReplaceOneAsync(s => s.FromCode == cur.FromCode && s.Date == cur.Date, cur, new ReplaceOptions { IsUpsert = true });
        return replaced.IsAcknowledged;
    }

    public async Task<int> SaveHistoricalCurrencies(List<CurrencyRate> curriencies)
    {
        try
        {
            await _currencyCollection.InsertManyAsync(curriencies, new InsertManyOptions { IsOrdered = false });
        }
        catch (MongoBulkWriteException e)
        {
            return e.WriteErrors.Count;
        }

        return 0;
    }
}