using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using MongoDB.Driver;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    private readonly IMongoDatabase database;
    private readonly IMongoCollection<Country> defaultCollection;
    
    public CountryRepository(IMongoClient context)
    {
        database = context.GetDatabase(MongoDatabaseSettings.BoersdataDatabaseName);
        defaultCollection = database.GetCollection<Country>("Countries");
    }

    public async Task<bool> Save(Country market, CancellationToken token = default)
    {
        var replaced = await defaultCollection
            .ReplaceOneAsync(s => s.Name == market.Name, market, new ReplaceOptions { IsUpsert = true }, token);
        return replaced.IsAcknowledged;
    }

    public async Task<long> SaveBatch(List<Country> market, CancellationToken token = default)
    {
        var tasks = market.Select(s => Save(s, token)).ToArray();
        var completedUpserts = await Task.WhenAll(tasks);
        return completedUpserts.Sum(u => u ? 1 : 0);
    }

    public async Task Delete(string name, CancellationToken token = default)
    {
        await defaultCollection.DeleteOneAsync(s => s.Name == name, token);
    }

    public async Task<Country?> GetById(string name, CancellationToken token = default)
    {
        return await defaultCollection.Find<Country>(s => s.Name == name).FirstOrDefaultAsync(token);
    }

    public async Task<IList<Country>> GetAll(CancellationToken token = default)
    {
        return await defaultCollection.Find(_ => true).ToListAsync(token);
    }
}