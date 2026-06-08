using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Extensions;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Models;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Factory.Functions;

public class PeFunction(ISecurityRepository securityRepositor, IIndicatorsRepository indicatorsRepository) : IFactoryFunction
{
    public Indicators Indicator => Indicators.Pe;

    public async Task<List<SecurityIndicatorDto>> Process(List<Security> securities, DateOnly date, LookBackPeriod lookBackPeriod)
    {
        var securityIds = securities.Select(s => s.SecurityId).ToHashSet();
        
        var fromDate = date.AddDays(-lookBackPeriod.Period);
        
        var prices =
            await securityRepositor.GetSecuritiesPricesHistory(securityIds, fromDate, date);
        if (prices.Count == 0)
            return new List<SecurityIndicatorDto>();

        var epsIndicators = await indicatorsRepository.GetIndicatorsByDate(date, new() { (long)Indicators.Eps }, securityIds);
        var epsIndicatorsDict = epsIndicators.ToDictionary(e => e.SecurityId);
        
        var returnList = new List<SecurityIndicatorDto>();
        foreach (var pricesGroup in prices.GroupBy(p => p.SecurityId))
        {
            if (!pricesGroup.Any())
                continue;

            if (!epsIndicatorsDict.TryGetValue(pricesGroup.Key, out var eps))
                continue;
            
            if (eps.Value == 0) // It cannot be null so it is missing data
                continue;
            
            var averagePrice = pricesGroup.Select(p => p.Close).Average();
            var peValue = (decimal)averagePrice / eps.Value;
            
            returnList.Add(new ()
            {
                SecurityId = pricesGroup.Key,
                IndicatorId = Indicator,
                Value = peValue,
                Date = date
            });
        }
        
        // Make PE comparison to securities industries
        var securityComparePeValueDict = securities
            .Where(r => r.Industry != null)
            .GroupBy(r => r.Industry)
            .SelectMany(s =>
            {
                var industrySecurities = s.Select(ss => ss.SecurityId).ToHashSet();

                var absoluteAverage = returnList.Where(r => industrySecurities.Contains(r.SecurityId))
                    .AverageBy(r => r.Value);

                return industrySecurities.Select(ss => new { SecurityId = ss, ComparePEValue = absoluteAverage });
            }).ToDictionary(s => s.SecurityId);

        foreach (var secIndicator in returnList)
        {
            if (!securityComparePeValueDict.TryGetValue(secIndicator.SecurityId, out var compareValue))
                continue;
            
            if (!compareValue.ComparePEValue.HasValue)
                continue;

            // If the security has a negative PE while the industry PE is positive, then we dont want it :)
            if (secIndicator.Value < 0 && compareValue.ComparePEValue.Value > 0)
                continue;
            
            var rankPeScore = Math.Abs(compareValue.ComparePEValue.Value) / secIndicator.Value;
            secIndicator.RankFriendlyValue = rankPeScore; // The higher the score the better ()
        }
        
        return returnList.Where(r => r.RankFriendlyValue.HasValue).ToList();
    }
}