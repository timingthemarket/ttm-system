using portfolio.Domain.Models;
using portfolio.Domain.Utils;
using TTM.Shared.Constants;

namespace portfolio.Domain.Portfolio.Components;

public static class Ranking
{
    private static bool IsRowsSame(decimal previousRowValue, decimal currentRowValue, decimal rowSimilarityLimit)
    {
        if (currentRowValue == 0)
            return true;

        return Math.Abs(1 - previousRowValue / currentRowValue) <= rowSimilarityLimit;
    }

    public static List<SecurityRank> Rank(this IEnumerable<IndicatorData> kpiData, StrategyInput parameters)
    {
        // Rank every INDICATOR induvidually and then sum the securitys rank together
        var securityIndicatorRanks = new List<SecurityRank>();
        foreach (var securityIndicatorGroup in kpiData.GroupBy(kd => kd.IndicatorId))
        {
            Indicators indicator = securityIndicatorGroup.Key;
            StrategyInputVariable indicatorParameters =
                parameters.StrategyVariables.First(sv => sv.IndicatorId == indicator);
            var indicatorGroupData = securityIndicatorGroup.ToList();

            List<InternalIndicatorData> orderedData;
            switch (indicatorParameters.Direction)
            {
                case Direction.Low:
                    orderedData = indicatorGroupData.OrderBy(igd => igd.Value).Select(sr =>
                        new InternalIndicatorData(sr.SecurityId, sr.Value)).ToList();
                    break;
                case Direction.High:
                    orderedData = indicatorGroupData.OrderByDescending(igd => igd.Value).Select(sr =>
                        new InternalIndicatorData(sr.SecurityId, sr.Value)).ToList();
                    break;
                default:
                    orderedData = new List<InternalIndicatorData>(indicatorGroupData.Select(sr =>
                        new InternalIndicatorData(sr.SecurityId, sr.Value)));
                    break;
            }

            var rankedIndicator = RankIndicator(orderedData, parameters.RowSimilarityLimit);
            securityIndicatorRanks.AddRange(rankedIndicator);
        }

        // Check so that all securtities have all data from all indicators
        var uniqueIndicators = parameters.StrategyVariables.DistinctBy(s => s.IndicatorId).Count();

        // Group the securities together and sum their ranks
        var newIdsToBeRanked = securityIndicatorRanks.GroupBy(sr => sr.SecurityId)
            .Where(sr => sr.Count() == uniqueIndicators) // Check so that all securities have all indicators
            .Select(sr => new InternalIndicatorData(sr.Key, sr.Sum(srr => srr.Rank)))
            .OrderBy(iid => iid.Value)
            .ToList();

        return RankIndicator(newIdsToBeRanked, 0); // RowSimilarity = 0
    }

    private static List<SecurityRank> RankIndicator(List<InternalIndicatorData> orderedData, double rowSimilarityLimit)
    {
        var securityRanks = new List<SecurityRank>();
        long rank = 1;
        SecurityRank? previousRankIncrementRow = null;
        foreach (InternalIndicatorData row in orderedData)
        {
            if (previousRankIncrementRow == null) // For the first row
            {
                previousRankIncrementRow = new SecurityRank(row.SecurityId, row.Value, rank);
                securityRanks.Add(previousRankIncrementRow);
                rank++;
                continue;
            }

            long? newRank;
            var rankRow = new SecurityRank(row.SecurityId, row.Value, long.MaxValue);
            if (IsRowsSame(previousRankIncrementRow.Value, rankRow.Value, (decimal)rowSimilarityLimit))
                // If the rows are the same then the current row should have then new rank
                newRank = previousRankIncrementRow.Rank;
            else
                newRank = rank;

            rankRow = rankRow with { Rank = newRank.Value };
            previousRankIncrementRow = rankRow;
            securityRanks.Add(rankRow);
            rank++;
        }
        
        return securityRanks;
    }

    private record InternalIndicatorData(long SecurityId, decimal Value);
}