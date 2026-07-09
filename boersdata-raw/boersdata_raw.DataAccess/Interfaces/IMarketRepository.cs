using boersdata_raw.DataAccess.Models;

namespace boersdata_raw.DataAccess.Interfaces;

public interface IMarketRepository
{
    public Task<bool> Save(Market market, CancellationToken token = default);
    public Task<long> SaveBatch(List<Market> market, CancellationToken token = default);
    public Task Delete(string name, CancellationToken token = default);
    public Task<Market?> GetById(string ticker, CancellationToken token = default);
    public Task<IList<Market>> GetAll(CancellationToken token = default);
}