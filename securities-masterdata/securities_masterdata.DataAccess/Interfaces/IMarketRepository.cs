using securities_masterdata.DataAccess.Entities;

namespace securities_masterdata.DataAccess.Interfaces;

public interface IMarketRepository
{
    Task<List<Market>> UpdateAllMarkets(List<Market> markets);
    Task<List<Market>> GetAllMarkets();
}