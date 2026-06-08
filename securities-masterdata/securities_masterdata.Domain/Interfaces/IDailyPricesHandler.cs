using TTM.Shared.Models.BoersDataRaw.Prices;

namespace securities_masterdata.Domain.Interfaces;

public interface IDailyPricesHandler
{
    Task<List<TTM.Shared.Models.SecuritiesMasterdata.Dto.SecurityPriceDto>> HandleDailyPrices(List<SecurityPriceDto> securityPrices);
}