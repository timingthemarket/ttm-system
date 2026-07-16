using System.Text.Json;
using portfolio.Domain.Constants;
using portfolio.Domain.Models;
using portfolio.Domain.Serialization;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Models;

namespace portfolio.Domain.Utils;

public class IndicatorCombinationGenerator
{
    public static List<PortfolioInputIndicatorVariable> GenerateIndicators()
    {
        var rnd = new Random();

        var indicatorsSelection = GetRandomIndicators();
        var prevWeight = 0.0;

        var indicators = indicatorsSelection
            .OrderBy(i => i)
            .Select(i =>
            {
                var direction = rnd.GetItems(new[] { Direction.High, Direction.Low }, 1).First();

                var minWeight = 0.05;
                var maxWeight = 1 - prevWeight - minWeight;
                var weight = Math.Round(rnd.NextDouble() * (maxWeight - minWeight) + minWeight, 2);

                var nrLookback = GetRandomLoockBackPeriod(i);

                var inputIndicatorvalue = new PortfolioInputIndicatorVariable
                {
                    Direction = direction,
                    LookBackPeriod = nrLookback,
                    IndicatorId = i
                    //Weight = weight,
                };

                if (i == Indicators.Dividend)
                    inputIndicatorvalue.ImputationStrategy = new StrategyImputation
                        { Action = MissingDataAction.Value, ImputationValue = 0 };
                
                if (i == Indicators.FScore)
                    inputIndicatorvalue.Direction = Direction.High; // FScore is always High

                return inputIndicatorvalue;
            }).Concat([
                new PortfolioInputIndicatorVariable
                {
                    Direction = Direction.High,
                    IndicatorId = Indicators.Return,
                    LookBackPeriod = new LookBackPeriod
                    {
                        Period = GetRandomLookback([180, 365]),
                        Aggregate = Aggregator.Average
                    }
                }
            ])
            .OrderBy(i => (int)i.IndicatorId)
            .ToList();

        // TODO: calculate realistic weights

        // Normalize the weights
        /*var sumOfNewWeights = indicators.Sum(a => a.Weight.Value);
        indicators = indicators
            .Select(a =>
            {
                a.Weight = Math.Round(a.Weight.Value / sumOfNewWeights, 2);
                return a;
            })
            .ToList();*/

        return indicators;
    }

    private static List<Indicators> GetRandomIndicators()
    {
        var indicatorsCopy = SupportedCalculationIndicators.SupportedIndicators.ToArray();

        var rnd = new Random();

        var amountOfIndicators = rnd.Next(1, 4); // Minimum 1 indicators, maximum 4
        var indicators = new List<Indicators>();
        while (indicators.Count < amountOfIndicators)
        {
            rnd.Shuffle(indicatorsCopy); // Shuffle the order
            var indicator = indicatorsCopy.First(i => !indicators.Contains(i));
            indicators.Add(indicator);
        }

        return indicators;
    }

    private static LookBackPeriod GetRandomLoockBackPeriod(Indicators indicator)
    {
        var possibleLockbacks = new List<int> { 180, 365 };
        if (indicator.IsComputedIndicator())
            possibleLockbacks.AddRange(TimeIntervals.AllIntervals.Select(TimeIntervals.GetNrDaysForInterval));

        var newLookback = GetRandomLookback(possibleLockbacks.ToArray());

        if (!indicator.IsComputedIndicator()) // TODO: start off with average aggregator
            return new LookBackPeriod { Period = newLookback, Aggregate = Aggregator.Average };
        return new LookBackPeriod { Period = newLookback };
    }

    private static int GetRandomLookback(int[] array)
    {
        var rnd = new Random();
        rnd.Shuffle(array);

        return array.First();
    }

    public static List<List<PortfolioInputIndicatorVariable>> GenerateAllIndicatorCombinations()
    {
        var supportedIndicators = SupportedCalculationIndicators.SupportedIndicators;
        var allCombinations1Set = new List<List<PortfolioInputIndicatorVariable>>();

        for (var i = 1; i < 1 << supportedIndicators.Count; i++)
        {
            var currentIndicators = new List<Indicators>();

            for (var j = 0; j < supportedIndicators.Count; j++)
                if ((i & (1 << j)) != 0)
                    currentIndicators.Add(supportedIndicators[j]);

            // Skip combinations with more than 4 indicators
            if (currentIndicators.Count > 4)
                continue;

            var directionCombinations = GenerateDirectionCombinations(currentIndicators);

            foreach (var directions in directionCombinations)
            {
                var lookbackCombinations = GenerateLookbackCombinations(currentIndicators);

                foreach (var lookbacks in lookbackCombinations)
                {
                    var indicatorVariables = new List<PortfolioInputIndicatorVariable>();

                    for (var k = 0; k < currentIndicators.Count; k++)
                    {
                        var indicator = currentIndicators[k];
                        var inputIndicatorVariable = new PortfolioInputIndicatorVariable
                        {
                            Direction = directions[k],
                            LookBackPeriod = lookbacks[k],
                            IndicatorId = indicator
                        };

                        if (indicator == Indicators.Dividend)
                            inputIndicatorVariable.ImputationStrategy = new StrategyImputation
                            {
                                Action = MissingDataAction.Value,
                                ImputationValue = 0
                            };

                        indicatorVariables.Add(inputIndicatorVariable);
                    }

                    indicatorVariables = indicatorVariables
                        .OrderBy(iv => (int)iv.IndicatorId)
                        .ToList();

                    allCombinations1Set.Add(indicatorVariables);
                }
            }
        }

        // Deduplicate allCombinations1Set before adding Return indicators
        var combinationKeys = new HashSet<string>();
        var uniqueCombinations1Set = new List<List<PortfolioInputIndicatorVariable>>();
        
        foreach (var combination in allCombinations1Set)
        {
            var key = string.Join(",", combination.Select(c => c.ToStringRepresentation()));
            if (combinationKeys.Add(key))
            {
                uniqueCombinations1Set.Add(combination);
            }
        }

        // Add Return indicators for each combination
        var allCombinations = new List<List<PortfolioInputIndicatorVariable>>();
        foreach (var days in new List<int> { 180, 365 })
        foreach (var comb in uniqueCombinations1Set)
        {
            var ret = new PortfolioInputIndicatorVariable
            {
                Direction = Direction.High,
                IndicatorId = Indicators.Return,
                LookBackPeriod = new LookBackPeriod
                {
                    Period = days,
                    Aggregate = Aggregator.Average
                }
            };

            var conc = comb.Concat([ret]).ToList();
            allCombinations.Add(conc);
        }

        return allCombinations;
    }

    public static Dictionary<string, PortfolioInput> GetInputsWithHashes(DateOnly date,
        List<List<PortfolioInputIndicatorVariable>> allCombinations, double rowSimilarity, decimal initMoney,
        double maxSecuritySpending)
    {
        var result = new Dictionary<string, PortfolioInput>();

        foreach (var indicators in allCombinations)
        {
            var input = new PortfolioInput
            {
                Date = date,
                Indicators = indicators,
                RowSimilarity = rowSimilarity,
                StrategyId = 1,
                Money = initMoney,
                MaxSecuritySpending = maxSecuritySpending
            };

            var portfolioHash = Functions.GetObjectHash(input, HashSerializer.Default.PortfolioInput);
            if (result.TryGetValue(portfolioHash, out var value))
            {
                var j1 = JsonSerializer.Serialize(input);
                var j2 = JsonSerializer.Serialize(value);

            }
            
            result[portfolioHash] = input;
        }

        return result;
    }

    private static List<List<Direction>> GenerateDirectionCombinations(List<Indicators> indicators)
    {
        var combinations = new List<List<Direction>>();
        var count = indicators.Count;
        var fscoreIndex = indicators.IndexOf(Indicators.FScore);
        
        // Calculate how many non-FScore indicators we have
        var nonFScoreCount = fscoreIndex >= 0 ? count - 1 : count;
        
        // Generate all combinations for non-FScore indicators
        for (var i = 0; i < Math.Pow(2, nonFScoreCount); i++)
        {
            var combination = new List<Direction>();
            var nonFScoreCounter = 0;
            
            for (var j = 0; j < count; j++)
            {
                if (indicators[j] == Indicators.FScore || indicators[j] == Indicators.Dividend)
                {
                    // FScore is always High
                    combination.Add(Direction.High);
                }
                else
                {
                    // Other indicators can be High or Low based on the bit pattern
                    var direction = ((i >> nonFScoreCounter) & 1) == 0 ? Direction.High : Direction.Low;
                    combination.Add(direction);
                    nonFScoreCounter++;
                }
            }
            
            combinations.Add(combination);
        }

        return combinations;
    }

    private static List<List<LookBackPeriod>> GenerateLookbackCombinations(List<Indicators> indicators)
    {
        var allLookbackOptions = indicators.Select(GetAllLookbackPeriods).ToList();
        var combinations = new List<List<LookBackPeriod>>();

        GenerateLookbackCombinationsRecursive(allLookbackOptions, new List<LookBackPeriod>(), 0, combinations);

        return combinations;
    }

    private static void GenerateLookbackCombinationsRecursive(
        List<List<LookBackPeriod>> allOptions,
        List<LookBackPeriod> current,
        int index,
        List<List<LookBackPeriod>> results)
    {
        if (index == allOptions.Count)
        {
            results.Add(new List<LookBackPeriod>(current));
            return;
        }

        foreach (var option in allOptions[index])
        {
            current.Add(option);
            GenerateLookbackCombinationsRecursive(allOptions, current, index + 1, results);
            current.RemoveAt(current.Count - 1);
        }
    }

    private static List<LookBackPeriod> GetAllLookbackPeriods(Indicators indicator)
    {
        var periods = new List<LookBackPeriod>();
        var basePeriods = new List<int> { 180, 365 };

        if (indicator.IsComputedIndicator())
            basePeriods.AddRange(TimeIntervals.AllIntervals.Select(TimeIntervals.GetNrDaysForInterval));

        foreach (var period in basePeriods)
            if (indicator.IsComputedIndicator())
                periods.Add(new LookBackPeriod { Period = period });
            else
                periods.Add(new LookBackPeriod { Period = period, Aggregate = Aggregator.Average });

        return periods;
    }
}