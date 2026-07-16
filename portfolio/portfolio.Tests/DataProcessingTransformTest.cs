using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using portfolio.Domain.Models;
using portfolio.Domain.Portfolio.Components;
using portfolio.Domain.Utils;
using TTM.Shared.Constants;
using Xunit;

namespace portfolio.Tests;

public class DataProcessingTransformTest
{
    public IEnumerable<IndicatorData> Data => new List<IndicatorData>
    {
        new() { SecurityId = 1, Value = 112, IndicatorId = Indicators.Dividend },
        new() { SecurityId = 1, Value = 2.12m, IndicatorId = Indicators.BetaOmx30 },
        new() { SecurityId = 1, Value = 0.222m, IndicatorId = Indicators.Revenues },
        new() { SecurityId = 2, Value = 13, IndicatorId = Indicators.Dividend },
        new() { SecurityId = 2, Value = 5.12m, IndicatorId = Indicators.BetaOmx30 },
        new() { SecurityId = 2, Value = 1233, IndicatorId = Indicators.Revenues },
        new() { SecurityId = 3, Value = -111, IndicatorId = Indicators.Dividend },
        new() { SecurityId = 3, Value = 343, IndicatorId = Indicators.BetaOmx30 },
        new() { SecurityId = 3, Value = 34433, IndicatorId = Indicators.Revenues }
    };

    /*[Theory]
    [InlineData(0.3333, 0.3333, 0.3333, true)]
    [InlineData(0.0001, 1, 0.9999, false)]
    public void Transform_data_ShouldBeBetween_0_1(double weightA, double weightB, double weightC, bool validWeights)
    {
        //setup
        var parameters = new StrategyInput
        {
            Money = 0,
            Hash = "",
            MaxSecuritySpending = 0,
            StrategyVariables = new List<StrategyInputVariable>
            {
                new StrategyInputVariable
                {
                    IndicatorId = Indicators.Dividend,
                    Weight = weightA,
                    Direction = default
                },
                new StrategyInputVariable
                {
                    IndicatorId = Indicators.BetaOmx30,
                    Weight = weightB,
                    Direction = default
                },
                new StrategyInputVariable
                {
                    IndicatorId = Indicators.Revenues,
                    Weight = weightC,
                    Direction = default
                }
            }
        };

        // act
        var kpiData = Data.Transform(parameters).ToList();

        //assert
        kpiData.Count.Should().Be(Data.Count());

        foreach (var grp in kpiData.GroupBy(k => k.IndicatorId))
        {
            StrategyInputVariable maxWeight = parameters.StrategyVariables.First(s => s.IndicatorId == grp.Key);

            if (validWeights)
                grp.Max(k => k.Value).Should().BeApproximately((decimal)maxWeight.Weight.Value, 0.00001m);
            else
                grp.Max(k => k.Value).Should().Be(1);

            grp.Min(k => k.Value).Should().Be(0);
        }
    }*/

    [Fact]
    public void RankingValueFunctionTransform_Should_Have_ExponentialValues()
    {
        // setup
        var rnd = new Random();
        var dummyValues = Enumerable.Range(1, 1000).ToArray();
        rnd.Shuffle(dummyValues);
        
        var securityRanks = dummyValues.Select(i => new SecurityRank(i, 0.05m, i)).ToList();
        
        //act
        var transformedRanks = new RankingValueFunctionTransform().ApplyFunction(securityRanks);
        
        //assert
        // Values should be in descending order
        for (int i = 1; i < transformedRanks.Count; i++)
        {
            transformedRanks[i].Value.Should().BeLessThanOrEqualTo(transformedRanks[i - 1].Value);
        }
    }
}