using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using MongoDB.Driver;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class SectorRepository : ISectorRepository
{
    private readonly IMongoCollection<Sector> _defaultCollection;

    public SectorRepository(IMongoClient context)
    {
        var database = context.GetDatabase(MongoDatabaseSettings.BoersdataDatabaseName);
        _defaultCollection = database.GetCollection<Sector>("Sectors");
    }

    public async Task Delete(string name, CancellationToken token = default)
    {
        await _defaultCollection.DeleteOneAsync(s => s.Name == name, token);
    }

    public async Task<bool> Save(Sector market, CancellationToken token = default)
    {
        var replaced = await _defaultCollection
            .ReplaceOneAsync(s => s.Name == market.Name, market, new ReplaceOptions { IsUpsert = true }, token);
        return replaced.IsAcknowledged;
    }

    public async Task<long> SaveBatch(List<Sector> market, CancellationToken token = default)
    {
        var tasks = market.Select(s => Save(s, token)).ToArray();
        var completedUpserts = await Task.WhenAll(tasks);
        return completedUpserts.Sum(u => u ? 1 : 0);
    }

    public async Task<Sector?> GetById(string name, CancellationToken token = default)
    {
        return await _defaultCollection.Find(s => s.Name == name).FirstOrDefaultAsync(token);
    }

    public async Task<IList<Sector>> GetAll(CancellationToken token = default)
    {
        return await _defaultCollection.Find(_ => true).ToListAsync(token);
    }
}