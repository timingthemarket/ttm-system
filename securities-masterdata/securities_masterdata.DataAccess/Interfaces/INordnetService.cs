using securities_masterdata.DataAccess.Services.Models;

namespace securities_masterdata.DataAccess.Interfaces;

public interface INordnetService
{
    Task<NordnetStocklistResponse?> GetStocksAsync(int offset = 0, int limit = 100, CancellationToken cancellationToken = default);
}
