using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Interfaces;

public interface IQrySecuritiesPricesHandler
{
    Task<List<SecurityPriceDto>> HandleGetTickerDatePrices(DateOnly date, HashSet<long> securityIds);
}