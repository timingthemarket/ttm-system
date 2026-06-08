using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Extensions;
using securities_masterdata.Domain.Interfaces;
using Skender.Stock.Indicators;
using TTM.Shared.Constants;
using TTM.Shared.Models;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Factory.Functions;

public class VolatilityFunction : IFactoryFunction
{
    private readonly ISecurityRepository _securityRepository;

    public VolatilityFunction(ISecurityRepository securityRepository)
    {
        _securityRepository = securityRepository;
    }

    public Indicators Indicator => Indicators.Volatility;

    public async Task<List<SecurityIndicatorDto>> Process(List<Security> securities, DateOnly date, LookBackPeriod lookBackPeriod)
    {
        var securityIds = securities.Select(s => s.SecurityId).ToHashSet();
        
        var fromDate1YearsBack = date.AddDays(-2 * lookBackPeriod.Period);
        var year1Prices =
            await _securityRepository.GetSecuritiesPricesHistory(securityIds, fromDate1YearsBack, date);
        if (!year1Prices.Any())
            return new List<SecurityIndicatorDto>();

        var returnList = new List<SecurityIndicatorDto>();
        foreach (var pricesGroup in year1Prices.GroupBy(p => p.SecurityId))
        {
            var quotes = MakeQuotes(pricesGroup.ToList());
            var stdDevCalc = quotes.GetStdDev(lookBackPeriod.Period).Condense();

            var volatility = stdDevCalc.MaxBy(s => s.Date);
            if (volatility != null)
                returnList.Add(MakeSecurityIndicator(pricesGroup.Key, volatility));
        }
        
        return returnList;
    }

    private List<Quote> MakeQuotes(List<SecurityPrice> prices) =>
        prices.Select(p => new Quote
        {
            Date = p.Date.ToDateWithNoTime(),
            Close = (decimal)p.Close,
            High = (decimal)p.High,
            Low = (decimal)p.Low,
            Open = (decimal)p.Open,
            Volume = p.Volume
        }).ToList();

    private SecurityIndicatorDto MakeSecurityIndicator(long securityId, StdDevResult result) =>
        new ()
        {
            Date = DateOnly.FromDateTime(result.Date),
            IndicatorId = Indicator,
            SecurityId = securityId,
            Value = (decimal)result.StdDev.Value
        };
}