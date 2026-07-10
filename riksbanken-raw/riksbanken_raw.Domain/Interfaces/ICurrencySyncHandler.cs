using ttm_system.Shared.Models.RiksbankenRaw;

namespace riksbanken_raw.Domain.Interfaces;

public interface ICurrencySyncHandler
{
    Task<List<CurrencyRateDto>> HandleLatestCurrencyExchangeSync();
}