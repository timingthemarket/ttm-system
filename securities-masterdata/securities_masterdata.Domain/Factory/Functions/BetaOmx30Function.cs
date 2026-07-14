using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Extensions;
using securities_masterdata.Domain.Interfaces;
using Skender.Stock.Indicators;
using TTM.Shared.Constants;
using TTM.Shared.Models;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Factory.Functions;

public class BetaOmx30Function(ISecurityRepository securityRepository, IIndexRepository indexRepository)
    : IFactoryFunction
{
    public Indicators Indicator => Indicators.BetaOmx30;

    public async Task<List<SecurityIndicatorDto>> Process(List<Security> securities, DateOnly date, LookBackPeriod lookBackPeriod)
    {
        var securityIds = securities.Select(s => s.SecurityId).ToHashSet();
        
        var fromDate1YearBack = date.AddDays(-lookBackPeriod.Period).AddMonths(-3); // add 3 months of extra data to get a more precise estimates
        
        var omx30Values = await indexRepository.GetIndexValues(1, fromDate1YearBack, date);
        var omx30IndexQuotes = MakeOrderedIndexQuotes(omx30Values);
        var indexDates = omx30Values.Select(o => o.Date).ToHashSet();
        
        var securitiesPrices = await securityRepository.GetSecuritiesPricesHistory(securityIds, fromDate1YearBack, date);

        var indicators = new List<SecurityIndicatorDto>();
        foreach (var securityPrices in securitiesPrices.GroupBy(sp => sp.SecurityId))
        {
            var filteredDateMissmatches = FilterPricesNotMatchingIndex(indexDates, securityPrices.ToList());
            var quotes = MakeOrderedQuotes(filteredDateMissmatches);

            var betaLookback = quotes.GetBeta(omx30IndexQuotes, lookBackPeriod.Period).Where(q => q.Beta.HasValue).MaxBy(q => q.Date);
            if (betaLookback != null)
            {
                indicators.Add(new SecurityIndicatorDto
                {
                    SecurityId = securityPrices.Key,
                    IndicatorId = Indicator,
                    Date = DateOnly.FromDateTime(betaLookback.Date),
                    Value = (decimal)betaLookback.Beta.Value
                });
            }
        }
        
        return indicators;
    }
    
    private List<Quote> MakeOrderedIndexQuotes(List<IndexValue> indexValues) => indexValues.Select(p => new Quote
    {
        Date = p.Date.ToDateTime(new TimeOnly()),
        Close = p.Value
    }).OrderBy(p => p.Date).ToList();    

    private List<SecurityPrice> FilterPricesNotMatchingIndex(HashSet<DateOnly> indexDates, List<SecurityPrice> prices)
    {
        return prices.Where(p => indexDates.Contains(p.Date)).ToList();
    }

    private List<Quote> MakeOrderedQuotes(List<SecurityPrice> prices) =>
        prices.Select(p => new Quote
        {
            Date = p.Date.ToDateWithNoTime(),
            Close = (decimal)p.Close,
            High = (decimal)p.High,
            Low = (decimal)p.Low,
            Open = (decimal)p.Open,
            Volume = p.Volume
        }).OrderBy(p => p.Date).ToList();
}