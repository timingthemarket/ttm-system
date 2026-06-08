using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Models;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Factory.Functions;

public class ReturnFunction : IFactoryFunction
{
    private readonly ISecurityRepository _securityRepository;

    public ReturnFunction(ISecurityRepository securityRepository)
    {
        _securityRepository = securityRepository;
    }
    
    public Indicators Indicator => Indicators.Return;

    /// <summary>
    /// Return the 1 year return of the stocks
    /// </summary>
    /// <param name="securityIds">The security ids</param>
    /// <param name="date">Date to base the calculation on</param>
    /// <returns></returns>
    public async Task<List<SecurityIndicatorDto>> Process(List<Security> securities, DateOnly date, LookBackPeriod lookBackPeriod)
    {
        var securityIds = securities.Select(s => s.SecurityId).ToHashSet();
        
        // Take a window of 5 days 1 year back
        var fromDate1YearBack = date.AddDays(-lookBackPeriod.Period);
        var toDate1YearBack = fromDate1YearBack.AddDays(4);
        var year1Prices = await _securityRepository.GetSecuritiesPricesHistory(securityIds, fromDate1YearBack, toDate1YearBack);
        if (!year1Prices.Any())
            return new List<SecurityIndicatorDto>();

        // Get the last 5 days of prices from date
        var pricesForDate =
            await _securityRepository.GetSecuritiesPricesHistory(securityIds, date.AddDays(-4), date);
        if (!pricesForDate.Any())
            return new List<SecurityIndicatorDto>();

        var returnList = new List<SecurityIndicatorDto>();
        foreach (var securityId in securityIds)
        {
            var year1PricesSecurity = year1Prices.Where(p => p.SecurityId == securityId).ToList();
            if (!year1PricesSecurity.Any())
                continue;
            
            var pricesForDateSecurity = pricesForDate.Where(p => p.SecurityId == securityId).ToList();
            if (!pricesForDateSecurity.Any())
                continue;
            
            var year1PricesAverage = year1PricesSecurity.Select(y => y.Close).Average();
            var pricesForDateAverage = pricesForDateSecurity.Select(y => y.Close).Average();

            var dateForCalculation = pricesForDateSecurity.Max(p => p.Date);
            
            returnList.Add(new SecurityIndicatorDto
            {
                SecurityId = securityId,
                Date = dateForCalculation,
                IndicatorId = Indicator,
                Value = CalculateLookbackReturn(pricesForDateAverage, year1PricesAverage)
            });
        }

        return returnList;
    }

    private decimal CalculateLookbackReturn(double priceDate, double priceLookback)
    {
        var fraction = (priceDate - priceLookback) / priceLookback; 
        return (decimal)fraction;
    }
}