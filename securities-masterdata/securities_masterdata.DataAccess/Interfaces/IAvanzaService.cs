using securities_masterdata.DataAccess.Services.Models;

namespace securities_masterdata.DataAccess.Interfaces;

public interface IAvanzaService
{
    Task<AvanzaStockFilterResponse?> GetStocksAsync(AvanzaStockFilterRequest request, CancellationToken cancellationToken = default);
    Task<AvanzaStockFilterResponse?> GetStocksAsync(int offset = 0, int limit = 10, string sortField = "numberOfOwners", string sortOrder = "desc", CancellationToken cancellationToken = default);
}