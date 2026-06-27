using boersdata_raw.DataAccess.Models;

namespace boersdata_raw.DataAccess.Interfaces;

public interface ISecuritiesRepository
{
    public Task<bool> Save(Security security, CancellationToken token = default);

    public Task Delete(string ticker, CancellationToken token = default);
    Task DeleteBatch(List<long> insIds, CancellationToken token = default);

    public Task<Security?> GetById(string ticker, CancellationToken token = default);

    Task DeleteAllNordic(CancellationToken token = default);
    Task DeleteAllGlobal(CancellationToken token = default);

    Task<long> SaveGlobalBatch(List<Security> security, CancellationToken token = default);

    public Task<List<Security>> GetStockTypeSecurities(CancellationToken token = default);

    public Task<List<Security>> GetNordicSecurities(int? limit = null, CancellationToken token = default);
    Task<List<Security>> GetGlobalSecurities(int? limit = null, CancellationToken token = default);
    Task<List<Security>> GetAllSecurities(int? limit = null, CancellationToken token = default);

    public Task<List<Security>> GetNordicSecurities(List<string> securitiesTickers, CancellationToken token = default);

    Task<List<Security>> GetGlobalSecurities(List<string> securitiesTickers,
        CancellationToken token = default);

    public Task<long> SaveBatch(List<Security> security, CancellationToken token = default);
}