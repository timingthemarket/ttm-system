using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models.Report;
using MongoDB.Driver;

namespace boersdata_raw.DataAccess.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Report> _defaultCollection;
    private readonly IMongoCollection<ReportTypes> _defaultReportTypesCollection;
    
    public ReportRepository(IMongoClient client)
    {
        _database = client.GetDatabase(MongoDatabaseSettings.BoersdataDatabaseName);
        _defaultReportTypesCollection = _database.GetCollection<ReportTypes>("ReportTypes");
        _defaultCollection = _database.GetCollection<Report>("Reports");

        if (!IndexExist(_defaultCollection.Indexes, "InsId"))
        {
            var index = Builders<Report>.IndexKeys
                .Ascending(s => s.InsId)
                .Ascending(s => s.ReportType);

            _defaultCollection.Indexes.CreateOne(
                new CreateIndexModel<Report>(index, new CreateIndexOptions { Name = "InsId" }));
        }

        if (!IndexExist(_defaultCollection.Indexes, "ticker_1"))
        {
            var index = Builders<Report>.IndexKeys
                .Ascending(s => s.Ticker);

            _defaultCollection.Indexes.CreateOne(
                new CreateIndexModel<Report>(index, new CreateIndexOptions { Name = "ticker_1" }));
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

    public async Task SaveReportTypes(List<ReportTypes> types, CancellationToken token = default)
    {
        await _defaultReportTypesCollection.DeleteManyAsync(_ => true, token);
        await _defaultReportTypesCollection.InsertManyAsync(types, null, token);
    }

    public async Task SaveHistoricalReports(string ticker, List<Report> reports, CancellationToken token = default)
    {
        await _defaultCollection.DeleteManyAsync(s => s.Ticker == ticker, token);
        await _defaultCollection.InsertManyAsync(reports, null, token);
    }
    
    public async Task<List<Report>> GetReports(string ticker, ReportType type, CancellationToken token = default)
    {
        return await _defaultCollection
            .Find(r => r.Ticker == ticker && r.ReportType == type)
            .ToListAsync(token);
    }
}