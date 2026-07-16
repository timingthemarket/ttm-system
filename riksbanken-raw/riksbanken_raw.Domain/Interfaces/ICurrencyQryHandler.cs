using riksbanken_raw.Domain.Models;
using ttm_system.Shared.Models.RiksbankenRaw;

namespace riksbanken_raw.Domain.Interfaces;

public interface ICurrencyQryHandler
{
    Task<List<CurrencyRateDto>> GetHistoricalCurrenciesByCode(string code);
}