using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using portfolio.Domain.Constants;
using portfolio.Domain.Extensions;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Utils;
using TTM.Shared.Constants;
using TTM.Shared.Functions;
using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace portfolio.Domain.Handlers;

/// <summary>
/// Scores every indicator in <see cref="IndicatorStrengthSets"/> on its own, month by month.
/// </summary>
/// <remarks>
/// For each rebalance date an artificial equal weighted portfolio is built from the top decile of
/// every sector, ranked by that indicator in its configured direction. The portfolio's realised
/// return over the following month feeds a rolling Sharpe ratio; the rank correlation between the
/// indicator's scores and the following month's returns across the whole eligible universe feeds a
/// rolling mean Information Coefficient. Both are normalized across indicators at each date and
/// combined into a single strength value.
///
/// The table this writes to is created by the migration in the portfolio API project, which is the
/// only project that runs FluentMigrator - the API must have started at least once before this runs.
/// </remarks>
public class IndicatorStrengthHandler(
    ILogger<IndicatorStrengthHandler> logger,
    IMasterdataService masterdataService,
    IIndicatorStrengthRepository indicatorStrengthRepository) : IIndicatorStrengthHandler
{
    /// <summary>Share of each sector taken into the artificial portfolio.</summary>
    private const double TopFraction = 0.10;

    /// <summary>
    /// Masterdata returns the latest price at or before the requested date, which for a security
    /// that has stopped trading is an arbitrarily old one. Anything staler than this is treated as
    /// no price at all rather than as a flat month.
    /// </summary>
    private const int MaxPriceStalenessDays = 10;

    private const int ProgressLogEveryDates = 10;

    public async Task ProcessIndicatorStrength(DateOnly today, int backfillYears = 12,
        CancellationToken cancellationToken = default)
    {
        List<DateOnly> dates = RebalanceDates.Generate(today, backfillYears);
        IReadOnlyList<IndicatorStrengthSet> sets = IndicatorStrengthSets.Sets;

        logger.LogInformation(
            "Starting indicator strength backtest over {DateCount} rebalance dates ({FirstDate} to {LastDate}) for {SetCount} indicator sets",
            dates.Count, dates.First(), dates.Last(), sets.Count);

        // The security list is date independent from masterdata's point of view, so it is fetched
        // once and reused for every date. Note this only contains securities that are still traded,
        // which introduces survivorship bias into the backfill - there is no historical universe
        // endpoint to fetch instead.
        Dictionary<long, string> sectorBySecurityId = await GetSectorMap();
        if (sectorBySecurityId.Count == 0)
        {
            logger.LogError("No securities with a sector were returned by masterdata. Aborting.");
            return;
        }

        logger.LogInformation("Loaded sectors for {SecurityCount} securities across {SectorCount} sectors",
            sectorBySecurityId.Count, sectorBySecurityId.Values.Distinct().Count());

        List<SecuritiesIndicatorQryMetadataDto> indicatorMetadata = BuildIndicatorMetadata(sets);

        // Observations are aligned with the date grid. The entry at index i covers the month that
        // starts at dates[i], so it only becomes knowable once dates[i + 1] has been fetched.
        Dictionary<string, double?[]> returnObservations =
            sets.ToDictionary(s => s.Key, _ => new double?[dates.Count]);
        Dictionary<string, double?[]> icObservations =
            sets.ToDictionary(s => s.Key, _ => new double?[dates.Count]);

        DateSnapshot? previous = null;
        var savedDates = 0;

        for (var i = 0; i < dates.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DateOnly date = dates[i];

            try
            {
                Dictionary<long, SecurityPriceDto> prices = await GetPrices(date);

                // Prices at this date close the month that opened at the previous date.
                if (previous != null)
                {
                    foreach (IndicatorStrengthSet set in sets)
                    {
                        SetObservation? observation = ComputeObservation(set, previous.IndicatorValues,
                            sectorBySecurityId, previous.Prices, prices);
                        if (observation == null) continue;

                        returnObservations[set.Key][previous.Index] = observation.PortfolioReturn;
                        icObservations[set.Key][previous.Index] = observation.InformationCoefficient;
                    }
                }

                Dictionary<Indicators, List<SecurityIndicatorDto>> indicatorValues =
                    await GetIndicatorValues(date, indicatorMetadata);

                // Only observations strictly before index i are complete, so scoring this date uses
                // the window ending at i - 1 and never looks ahead.
                List<IndicatorStrength> strengths =
                    BuildStrengths(sets, date, i, returnObservations, icObservations);

                if (strengths.Count > 0)
                {
                    await indicatorStrengthRepository.SaveMany(date, strengths);
                    savedDates++;
                }
                else
                {
                    logger.LogDebug("No indicator set had enough history to be scored at {Date}", date);
                }

                previous = new DateSnapshot(i, indicatorValues, prices);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process indicator strength for {Date}. Continuing.", date);
                // Drop the snapshot so the next date does not pair its prices with a stale month.
                previous = null;
            }

            if ((i + 1) % ProgressLogEveryDates == 0)
                logger.LogInformation("Processed {Current}/{Total} rebalance dates ({SavedDates} scored)",
                    i + 1, dates.Count, savedDates);
        }

        logger.LogInformation(
            "Indicator strength backtest complete. Scored {SavedDates}/{TotalDates} rebalance dates",
            savedDates, dates.Count);
    }

    private async Task<Dictionary<long, string>> GetSectorMap()
    {
        SecuritiesQryResponse securities = await masterdataService.GetSecurites(null, null);

        return securities.Securities
            .Where(s => !string.IsNullOrWhiteSpace(s.Sector))
            .GroupBy(s => s.SecurityId)
            .ToDictionary(g => g.Key, g => g.First().Sector);
    }

    /// <summary>
    /// One query entry per indicator. Sets that differ only by direction share the same underlying
    /// data, so they are collapsed into a single fetch.
    /// </summary>
    private List<SecuritiesIndicatorQryMetadataDto> BuildIndicatorMetadata(IReadOnlyList<IndicatorStrengthSet> sets)
    {
        var metadata = new List<SecuritiesIndicatorQryMetadataDto>();

        foreach (var group in sets.GroupBy(s => s.Indicator))
        {
            IndicatorStrengthSet first = group.First();

            // The response carries no look back period, so two sets on the same indicator with
            // different spans would be indistinguishable once the values come back.
            if (group.Any(s => s.LookBackDays != first.LookBackDays || s.Aggregate != first.Aggregate))
                logger.LogWarning(
                    "Indicator {Indicator} is configured with more than one look back period. Using {Days} days / {Aggregate} for all of its sets.",
                    first.Indicator, first.LookBackDays, first.Aggregate);

            metadata.Add(new SecuritiesIndicatorQryMetadataDto
            {
                IndicatorId = first.Indicator,
                LookBackPeriod = first.ToLookBackPeriod()
            });
        }

        return metadata;
    }

    private async Task<Dictionary<Indicators, List<SecurityIndicatorDto>>> GetIndicatorValues(DateOnly date,
        List<SecuritiesIndicatorQryMetadataDto> metadata)
    {
        SecuritiesIndicatorsQryResponse response = await masterdataService.GetIndicators(date, metadata);

        return response.Variables
            .GroupBy(v => v.IndicatorId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private async Task<Dictionary<long, SecurityPriceDto>> GetPrices(DateOnly date)
    {
        SecuritiesPricesQryResponse response = await masterdataService.GetLatestPrices(date, null);
        DateOnly oldestAccepted = date.AddDays(-MaxPriceStalenessDays);

        return response.SecurityPrices
            .Where(p => p.Date >= oldestAccepted && p.Date <= date)
            .GroupBy(p => p.SecurityId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Date).First());
    }

    /// <summary>
    /// The artificial portfolio's realised return and the Information Coefficient for one indicator
    /// set over the month between two rebalance dates.
    /// </summary>
    private static SetObservation? ComputeObservation(
        IndicatorStrengthSet set,
        IReadOnlyDictionary<Indicators, List<SecurityIndicatorDto>> indicatorValues,
        IReadOnlyDictionary<long, string> sectorBySecurityId,
        IReadOnlyDictionary<long, SecurityPriceDto> pricesAtDate,
        IReadOnlyDictionary<long, SecurityPriceDto> pricesAtNextDate)
    {
        if (!indicatorValues.TryGetValue(set.Indicator, out List<SecurityIndicatorDto>? values)) return null;

        List<Candidate> universe = BuildUniverse(values, sectorBySecurityId, pricesAtDate, pricesAtNextDate);
        if (universe.Count == 0) return null;

        List<Candidate> holdings = SectorSelection.TopPerSector(
            universe, c => c.Sector, c => c.Value, set.Direction, TopFraction);
        if (holdings.Count == 0) return null;

        // Equal weighted, so the portfolio return is the mean of its holdings' returns.
        var portfolioReturn = holdings.Average(h => h.ForwardReturn);

        // The IC is measured across the whole eligible universe rather than the selected decile -
        // that is the standard definition and far more stable than a correlation over ~10% of names.
        // Scores are signed so that "good" is always high, making the IC comparable between a High
        // set and a Low set of the same indicator.
        var sign = set.Direction == Direction.Low ? -1.0 : 1.0;
        var informationCoefficient = StrengthStatistics.InformationCoefficient(
            universe.Select(c => c.Value * sign).ToList(),
            universe.Select(c => c.ForwardReturn).ToList());

        return new SetObservation(portfolioReturn, informationCoefficient);
    }

    /// <summary>
    /// Securities that have an indicator value, a known sector, and a usable price at both ends of
    /// the month. Anything missing one of those cannot contribute to either metric.
    /// </summary>
    private static List<Candidate> BuildUniverse(
        List<SecurityIndicatorDto> values,
        IReadOnlyDictionary<long, string> sectorBySecurityId,
        IReadOnlyDictionary<long, SecurityPriceDto> pricesAtDate,
        IReadOnlyDictionary<long, SecurityPriceDto> pricesAtNextDate)
    {
        var universe = new List<Candidate>(values.Count);

        foreach (SecurityIndicatorDto indicatorValue in values)
        {
            if (!sectorBySecurityId.TryGetValue(indicatorValue.SecurityId, out string? sector)) continue;
            if (!pricesAtDate.TryGetValue(indicatorValue.SecurityId, out SecurityPriceDto? startPrice)) continue;
            if (!pricesAtNextDate.TryGetValue(indicatorValue.SecurityId, out SecurityPriceDto? endPrice)) continue;

            var start = startPrice.MedianPrice();
            if (start <= 0) continue;

            var forwardReturn = SharedFunctions.CalculateFraction(endPrice.MedianPrice(), start);
            var value = (double)indicatorValue.Value;

            if (!double.IsFinite(value) || !double.IsFinite(forwardReturn)) continue;

            universe.Add(new Candidate(sector, value, forwardReturn));
        }

        return universe;
    }

    /// <summary>
    /// Rolling Sharpe and mean IC per set, min-max normalized against each other at this date and
    /// combined into the strength metric.
    /// </summary>
    private List<IndicatorStrength> BuildStrengths(
        IReadOnlyList<IndicatorStrengthSet> sets,
        DateOnly date,
        int dateIndex,
        IReadOnlyDictionary<string, double?[]> returnObservations,
        IReadOnlyDictionary<string, double?[]> icObservations)
    {
        var scored = new List<ScoredSet>();

        foreach (IndicatorStrengthSet set in sets)
        {
            List<double> returns = StrengthStatistics.RollingWindow(returnObservations[set.Key], dateIndex);
            double? sharpe = StrengthStatistics.Sharpe(returns);
            if (sharpe == null) continue;

            List<double> ics = StrengthStatistics.RollingWindow(icObservations[set.Key], dateIndex);
            double? meanIc = ics.Count > 0 ? ics.Average() : null;

            scored.Add(new ScoredSet(set, sharpe.Value, meanIc));
        }

        if (scored.Count == 0) return new List<IndicatorStrength>();

        var minSharpe = scored.Min(s => s.Sharpe);
        var maxSharpe = scored.Max(s => s.Sharpe);

        List<ScoredSet> withIc = scored.Where(s => s.MeanIc.HasValue).ToList();
        var minIc = withIc.Count > 0 ? withIc.Min(s => s.MeanIc!.Value) : 0;
        var maxIc = withIc.Count > 0 ? withIc.Max(s => s.MeanIc!.Value) : 0;

        if (withIc.Count < scored.Count)
            logger.LogWarning(
                "{MissingCount}/{TotalCount} indicator sets had no Information Coefficient at {Date}; they are scored on Sharpe with a neutral IC.",
                scored.Count - withIc.Count, scored.Count, date);

        return scored
            .Select(s => new IndicatorStrength
            {
                IndicatorId = s.Set.Indicator,
                Direction = s.Set.Direction,
                Date = date,
                Strength = StrengthStatistics.Strength(
                    StrengthStatistics.Normalize(s.Sharpe, minSharpe, maxSharpe),
                    s.MeanIc.HasValue ? StrengthStatistics.Normalize(s.MeanIc.Value, minIc, maxIc) : 0.5),
                // The un-normalized inputs, so a strength value can be read back against the raw
                // numbers it came from rather than only against its peers at this date.
                Metadata = new IndicatorStrengthMetadata(s.Sharpe, s.MeanIc).ToJson()
            })
            .ToList();
    }

    private sealed record DateSnapshot(
        int Index,
        Dictionary<Indicators, List<SecurityIndicatorDto>> IndicatorValues,
        Dictionary<long, SecurityPriceDto> Prices);

    private sealed record Candidate(string Sector, double Value, double ForwardReturn);

    private sealed record SetObservation(double PortfolioReturn, double? InformationCoefficient);

    private sealed record ScoredSet(IndicatorStrengthSet Set, double Sharpe, double? MeanIc);
}
