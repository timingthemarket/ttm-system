using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class SecuritiesRepository : ISecuritiesRepository
{
    private readonly IMongoDatabase database;
    private readonly IMongoCollection<Security> nordicollection;
    private readonly IMongoCollection<Security> globalCollection;

    private const string SecuritiesCacheKey = "securitiescachekey";

    public SecuritiesRepository(IMongoClient context)
    {
        database = context.GetDatabase(MongoDatabaseSettings.BoersdataDatabaseName);
        nordicollection = database.GetCollection<Security>("NordicSecurities");
        globalCollection = database.GetCollection<Security>("GlobalSecurities");

        const string indexName = "Ticker_1";
        if (!IndexExist(nordicollection.Indexes, indexName))
        {
            var indexKeysDef = Builders<Security>.IndexKeys.Ascending(s => s.Ticker);
            nordicollection.Indexes.CreateOne(new CreateIndexModel<Security>(indexKeysDef, 
                new CreateIndexOptions { Unique = true, Name = indexName }));
        }
        
        if (!IndexExist(globalCollection.Indexes, indexName))
        {
            var indexKeysDef = Builders<Security>.IndexKeys.Ascending(s => s.Ticker);
            globalCollection.Indexes.CreateOne(new CreateIndexModel<Security>(indexKeysDef,
                new CreateIndexOptions { Unique = true, Name = indexName }));
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
    
    public async Task<bool> Save(Security security, CancellationToken token = default)
    {
        var replaced = await nordicollection
            .ReplaceOneAsync(s => s.Ticker == security.Ticker, security, new ReplaceOptions { IsUpsert = true }, token);
        return replaced.IsAcknowledged;
    }

    public async Task<long> SaveBatch(List<Security> security, CancellationToken token = default)
    {
        var tasks = security.Select(s => Save(s, token)).ToArray();
        var completedUpserts = await Task.WhenAll(tasks);
        return completedUpserts.Sum(u => u ? 1 : 0);
    }

    public async Task<long> SaveGlobalBatch(List<Security> security, CancellationToken token = default)
    {
        await globalCollection.InsertManyAsync(security, cancellationToken: token);
        return security.Count;
    }

    public async Task DeleteBatch(List<long> insIds, CancellationToken token = default)
    {
        await nordicollection.DeleteManyAsync(s => insIds.Contains(s.InsId), token);
    }

    public async Task DeleteAllNordic(CancellationToken token = default)
    {
        await nordicollection.DeleteManyAsync(new BsonDocument(), token);
    }

    public async Task DeleteAllGlobal(CancellationToken token = default)
    {
        await globalCollection.DeleteManyAsync(new BsonDocument(), token);
    }

    public async Task Delete(string ticker, CancellationToken token = default)
    {
        await nordicollection.DeleteOneAsync(s => s.Ticker == ticker, token);
    }

    public async Task<Security?> GetById(string ticker, CancellationToken token = default)
    {
        return await nordicollection.Find<Security>(s => s.Ticker == ticker).FirstOrDefaultAsync(token);
    }

    public async Task<List<Security>> GetNordicSecurities(int? limit = null, CancellationToken token = default)
    {
        return await nordicollection.Find(_ => true).Limit(limit).ToListAsync(token);
    }

    public async Task<List<Security>> GetGlobalSecurities(int? limit = null, CancellationToken token = default)
    {
        return await globalCollection.Find(_ => true).Limit(limit).ToListAsync(token);
    }

    public async Task<List<Security>> GetAllSecurities(int? limit = null, CancellationToken token = default)
    {
        var nordicSecuritiesTask = GetNordicSecurities(token: token);
        var globalSecuritiesTask = GetGlobalSecurities(token: token);
        
        var securitiesResult = await Task.WhenAll(nordicSecuritiesTask, globalSecuritiesTask);

        var securities = securitiesResult[0].Concat(securitiesResult[1]).ToList();
        
        return securities;
    }
    
    public async Task<List<Security>> GetStockTypeSecurities(CancellationToken token = default)
    {
        var securitiies = await GetAllSecurities(token: token);
        return securitiies.Where(s => s.Type == SecurityType.Stocks || s.Type == SecurityType.Adr).ToList();
    }
    
    public async Task<List<Security>> GetNordicSecurities(List<string> securitiesTickers, CancellationToken token = default)
    {
        return await nordicollection.Find(s => securitiesTickers.Contains(s.Ticker)).ToListAsync(token);
    }

    public async Task<List<Security>> GetGlobalSecurities(List<string> securitiesTickers,
        CancellationToken token = default)
    {
        return await globalCollection.Find(s => securitiesTickers.Contains(s.Ticker)).ToListAsync(token);
    }
}

