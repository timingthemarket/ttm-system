using portfolio.Domain.Models;
using portfolio.Domain.Utils;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace portfolio.Domain.Portfolio.Components;

public static class DataProcessing
{
    private static IEnumerable<IndicatorData> ApplyWeights(IEnumerable<IndicatorData> indicatorData,
        List<StrategyInputVariable> variables)
    {
        indicatorData = indicatorData.ToList();
        
        var weightsExists = variables.All(v => v.Weight.HasValue);
        var weightSum = weightsExists ? variables.Sum(i => i.Weight!.Value) : 0;

        var absWeightSum = Math.Abs(Math.Round(weightSum, 2) - 1);

        if (!weightsExists)
            return indicatorData;
        
        if (weightsExists && !(absWeightSum < 0.0001))
            return indicatorData;

        var variablesDict = variables.ToDictionary(v => v.IndicatorId);
        var newData = new List<IndicatorData>(indicatorData.Count());
        foreach (var data in indicatorData)
        {
            if (variablesDict.TryGetValue(data.IndicatorId, out var variable))
                newData.Add(data with { Value = data.Value * (decimal)variable.Weight!.Value });
        }

        return newData;
    }
    
    private static IEnumerable<IndicatorData> Normalize01(List<IndicatorData> values)
    {
        decimal min = values.Min(v => v.RankFriendlyValue ?? v.Value);
        decimal max = values.Max(v => v.RankFriendlyValue ?? v.Value);
        
        return values.Select(v => v with { Value = Functions.Normalize01(v.RankFriendlyValue ?? v.Value, min, max) });
    }

    private static IEnumerable<IndicatorData> ImputeData(List<IndicatorData> kpiData,
        StrategyInput parameters, List<SecurityDto> securities)
    {
        var returnData = new List<IndicatorData>(kpiData);
        foreach (var variable in parameters.StrategyVariables)
        {
            // if there the instruction is to impute a value, then we need to fins the indicators with values and remove the ones without
            if (variable.Imputation.Action == MissingDataAction.Value && variable.Imputation.ImputationValue.HasValue)
            {
                var securityIdsWithData = kpiData.Where(k => k.IndicatorId == variable.IndicatorId)
                    .Select(d => d.SecurityId).ToHashSet();
                foreach (var security in securities)
                {
                    if (securityIdsWithData.Contains(security.SecurityId))
                        continue;

                    returnData.Add(new IndicatorData
                    {
                        Value = variable.Imputation.ImputationValue.Value,
                        IndicatorId = variable.IndicatorId,
                        SecurityId = security.SecurityId
                    });
                }
            }
        }

        return returnData;
    }

    public static IEnumerable<IndicatorData> Transform(this IEnumerable<IndicatorData> kpiData,
        StrategyInput parameters, List<SecurityDto> securities)
    {
        kpiData = kpiData.ToList();
        
        // Normalize by KpiId to a 0-1 range
        /*var normalizeData = kpiData.GroupBy(a => a.IndicatorId)
            .SelectMany(a => Normalize01(a.ToList()));

        return ApplyWeights(normalizeData, parameters.StrategyVariables);*/
        kpiData = ImputeData(kpiData.ToList(), parameters, securities);
        
        var data = kpiData.Select(v => v with { Value = v.RankFriendlyValue ?? v.Value });
        return ApplyWeights(data, parameters.StrategyVariables);
    }
}