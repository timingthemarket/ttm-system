using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Extensions;
using securities_masterdata.Domain.Interfaces;
using Skender.Stock.Indicators;
using TTM.Shared.Constants;
using TTM.Shared.Models;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Factory.Functions;

public class RsiMomentumFunction(
    ISecurityRepository securityRepository)
    : IFactoryFunction
{
    public Indicators Indicator => Indicators.RsiMomentum;

    /// <summary>
    /// https://dotnet.stockindicators.dev/indicators/Rsi/#content
    /// </summary>
    /// <param name="securityIds"></param>
    /// <param name="date"></param>
    /// <param name="lookBackPeriod"></param>
    /// <returns></returns>
    public async Task<List<SecurityIndicatorDto>> Process(List<Security> securities, DateOnly date, LookBackPeriod lookBackPeriod)
    {
        var securityIds = securities.Select(s => s.SecurityId).ToHashSet();

        //var recommendedLookbackBackPeriods = lookBackPeriod.Period > 100 ? lookBackPeriod.Period * 3 : 300;
        var recommendedLookbackBackPeriods = lookBackPeriod.Period * 10; // Recommended from stockindciators documentation
        var fromDate = date.AddDays(-recommendedLookbackBackPeriods);
        var prices =
            await securityRepository.GetSecuritiesPricesHistory(securityIds, fromDate, date);
        if (!prices.Any())
            return new List<SecurityIndicatorDto>();

        var returnList = new List<SecurityIndicatorDto>();
        foreach (var pricesGroup in prices.GroupBy(p => p.SecurityId))
        {
            var quotes = MakeQuotes(pricesGroup.ToList());
            var stdDevCalc = quotes.GetRsi(lookBackPeriod.Period).Condense();

            var rsi = stdDevCalc.MaxBy(s => s.Date);
            if (rsi != null)
                returnList.Add(MakeSecurityIndicator(pricesGroup.Key, rsi));
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

    private SecurityIndicatorDto MakeSecurityIndicator(long securityId, RsiResult result) =>
        new()
        {
            Date = DateOnly.FromDateTime(result.Date),
            IndicatorId = Indicator,
            SecurityId = securityId,
            Value = (decimal)result.Rsi.Value
        };
}