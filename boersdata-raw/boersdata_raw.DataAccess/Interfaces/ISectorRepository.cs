using boersdata_raw.DataAccess.Models;

namespace boersdata_raw.DataAccess.Interfaces;

public interface ISectorRepository
{
    public Task<bool> Save(Sector market, CancellationToken token = default);
    public Task<long> SaveBatch(List<Sector> market, CancellationToken token = default);
    public Task Delete(string name, CancellationToken token = default);
    public Task<Sector?> GetById(string ticker, CancellationToken token = default);
    public Task<IList<Sector>> GetAll(CancellationToken token = default);
}