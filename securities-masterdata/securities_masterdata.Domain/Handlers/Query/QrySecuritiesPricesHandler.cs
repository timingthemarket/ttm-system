using System.Collections.Frozen;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Handlers.Query;

public class QrySecuritiesPricesHandler(ISecurityRepository securityRepository) : IQrySecuritiesPricesHandler
{
    public async Task<List<SecurityPriceDto>> HandleGetTickerDatePrices(DateOnly date, HashSet<long>? securityIds)
    {
        var latestPricesByDate = await securityRepository.GetSecuritiesPricesByDate(date, securityIds);
        return MapToSecurityPriceDtos(latestPricesByDate);
    }
    
    private static List<SecurityPriceDto> MapToSecurityPriceDtos(List<SecurityPrice> prices) =>
        prices.Select(p => new SecurityPriceDto
        {
            SecurityId = p.SecurityId,
            Date = p.Date,
            Volume = p.Volume,
            Close = p.Close,
            High = p.High,
            Low = p.Low,
            Open = p.Open
        }).ToList();
}