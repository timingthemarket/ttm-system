using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using MongoDB.Driver;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class MarketRepository : IMarketRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Market> _defaultCollection;

    public MarketRepository(IMongoClient context)
    {
        _database = context.GetDatabase(MongoDatabaseSettings.BoersdataDatabaseName);
        _defaultCollection = _database.GetCollection<Market>("Markets");
    }

    public async Task<bool> Save(Market market, CancellationToken token = default)
    {
        var replaced = await _defaultCollection
            .ReplaceOneAsync(s => s.Name == market.Name, market, new ReplaceOptions { IsUpsert = true }, token);
        return replaced.IsAcknowledged;
    }

    public async Task<long> SaveBatch(List<Market> market, CancellationToken token = default)
    {
        var tasks = market.Select(s => Save(s, token)).ToArray();
        var completedUpserts = await Task.WhenAll(tasks);
        return completedUpserts.Sum(u => u ? 1 : 0);
    }

    public async Task Delete(string name, CancellationToken token = default)
    {
        await _defaultCollection.DeleteOneAsync(s => s.Name == name, token);
    }

    public async Task<Market?> GetById(string ticker, CancellationToken token = default)
    {
        return await _defaultCollection.Find(s => s.Name == ticker).FirstOrDefaultAsync(token);
    }

    public async Task<IList<Market>> GetAll(CancellationToken token = default)
    {
        return await _defaultCollection.Find(_ => true).ToListAsync(token);
    }
}