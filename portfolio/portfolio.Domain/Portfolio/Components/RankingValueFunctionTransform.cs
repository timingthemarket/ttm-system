using portfolio.Domain.Models;
using portfolio.Domain.Utils;

namespace portfolio.Domain.Portfolio.Components;

public class RankingValueFunctionTransform
{
    private const double Base = 1;
    private const double Multiplier = 1.6;
    
    private record FunctionDefinition(double ToXValue, int ResultValue);

    private static List<(double ToXValue, int YValue)> RelativeRankLimits => new()
    {
        (0.001, 35),
        (0.01, 33),
        (0.015, 31),
        (0.02, 28),
        (0.03, 25),
        (0.04, 24),
        (0.05, 23),
        (0.06, 22),
        (0.07, 21),
        (0.08, 20),
        (0.09, 17),
        (0.1, 15),
        (0.11, 14),
        (0.12, 13),
        (0.13, 12),
        (0.14, 11),
        (0.15, 10),
        (0.20, 8),
        (0.25, 5),
        (0.40, 4),
        (0.60, 3),
        (0.80, 1)
    };
    
    public List<FunctionSecurityRank> ApplyFunction(List<SecurityRank> securityRanks)
    {
        var maxRank = securityRanks.Select(r => (double)r.Rank).Max();
 //TODO: reverse this so that we minimize the funtion instead of maximize??? Or
        var rankingFunction = RelativeRankLimits.OrderDescending()
            .Select((li, index) => new FunctionDefinition(li.ToXValue, GetObjScore(li.YValue))).ToList();
        var ret = new List<FunctionSecurityRank>();
        foreach (var rank in securityRanks)
        {
            var relativeRank = rank.Rank / maxRank;
            
            var funcValue = GetFunctionValue(relativeRank, rankingFunction);
            if (funcValue <= 0)
                continue;
            
            ret.Add(new (rank.SecurityId, rank.Value, rank.Rank, funcValue));
        }

        return ret;
    }

    private static int GetObjScore(int position) => (int)(Base * Math.Pow(Multiplier, position));

    private int GetFunctionValue(double relativeRank, List<FunctionDefinition> rankingFunction)
    {
        foreach (var funcValue in rankingFunction.OrderBy(r => r.ToXValue))
        {
            if (funcValue.ToXValue >= relativeRank)
                return funcValue.ResultValue;
        }

        return (int)Base;
    }
}