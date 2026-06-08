using securities_masterdata.DataAccess.Entities;
using Index = securities_masterdata.DataAccess.Entities.Index;

namespace securities_masterdata.DataAccess.Interfaces;

public interface IIndexRepository
{
    Task<List<Index>> GetIndexWithSecurities(bool asNoTracking = true);
    Task InsertIndexValues(List<IndexValue> values);
    Task<List<IndexValue>> GetLatestIndexValues();
    Task<int> DeleteIndexValues(long indexId);
    Task SaveIndex(Index index);
    Task<Index?> GetIndexById(long indexId);

    Task<List<IndexValue>> GetIndexValues(long indexId, DateOnly? fromUtcDate = null,
        DateOnly? toUtcDate = null);
}