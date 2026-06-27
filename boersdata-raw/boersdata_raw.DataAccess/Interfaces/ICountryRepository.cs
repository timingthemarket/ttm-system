using boersdata_raw.DataAccess.Models;

namespace boersdata_raw.DataAccess.Interfaces;

public interface ICountryRepository
{
    public Task<bool> Save(Country market, CancellationToken token = default);
    public Task<long> SaveBatch(List<Country> market, CancellationToken token = default);
    public Task Delete(string name, CancellationToken token = default);
    public Task<Country?> GetById(string ticker, CancellationToken token = default);
    public Task<IList<Country>> GetAll(CancellationToken token = default);
}