using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using portfolio.Domain.Constants;
using portfolio.Domain.Models;
using portfolio.Domain.Portfolio.Components;
using portfolio.Domain.Utils;
using TTM.Shared.Constants;
using Xunit;

namespace portfolio.Tests;

public class RankingTest
{
    [Fact]
    public void Ranking2Indicators_ShouldReturnCorrectRank()
    {
        // Setup
        long security1 = 1;
        long security2 = 2;

        var parameters = new StrategyInput
        {
            Date = new DateOnly(2022, 02, 10),
            RowSimilarityLimit = 0,
            Money = 0,
            Hash = "",
            MaxSecuritySpending = 0,
            StrategyVariables = new()
            {
                new ()
                {
                    IndicatorId = Indicators.Dividend,
                    Direction = Direction.High
                },
                new()
                {
                    IndicatorId = Indicators.Pe,
                    Direction = Direction.Low
                }
            }
        };
        
        var indicatorData = new List<IndicatorData>
        {
            new ()
            {
                SecurityId = security1,
                IndicatorId = Indicators.Dividend,
                Value = 10
            },
            new()
            {
                SecurityId = security1,
                IndicatorId = Indicators.Pe,
                Value = 2
            },
            new()
            {
                SecurityId = security2,
                IndicatorId = Indicators.Dividend,
                Value = 10
            },
            new()
            {
                SecurityId = security2,
                IndicatorId = Indicators.Pe,
                Value = 5
            }
        };

        // Act
        var ranks = indicatorData.Rank(parameters);

        // Assert
        ranks[0].Rank.Should().Be(1);
        ranks[0].SecurityId.Should().Be(security1);
        ranks[1].Rank.Should().Be(2);
        ranks[1].SecurityId.Should().Be(security2);
    }

    [Fact]
    public void Ranking1Indicator_Random_ShouldReturnCorrectRank()
    {
        // Setup
        var rnd = new Random();
        
        var securities = Enumerable.Range(0, 1000).Select(i => (long)i).ToList();

        var indicator = Indicators.GrossProfit;
        var parameters = new StrategyInput
        {
            Hash = "",
            Date = new DateOnly(2022, 02, 10),
            RowSimilarityLimit = 0,
            Money = 0,
            MaxSecuritySpending = 1000,
            StrategyVariables = new()
            {
                new()
                {
                    IndicatorId = indicator,
                    Direction = Direction.Low
                }
            }
        };

        var indicatorData = securities.Select(s => new IndicatorData
        {
            SecurityId = s,
            IndicatorId = indicator,
            Value = (decimal)rnd.NextDouble()
        }).ToList();

        // Act
        var ranks = indicatorData.Rank(parameters);

        // Assert
        long rank = 1;
        foreach (var rankVar in ranks)
        {
            rankVar.Rank.Should().Be(rank);
            securities.Should().Contain(rankVar.SecurityId);
            rank++;
        }

        var maxData = indicatorData.MaxBy(id => id.Value);
        var maxRank = ranks.Last();
        maxData.SecurityId.Should().Be(maxRank.SecurityId);

        var lowData = indicatorData.MinBy(id => id.Value);
        var lowRank = ranks.First();
        lowData.SecurityId.Should().Be(lowRank.SecurityId);
    }

    [Fact]
    public void 
        Ranking2Indicators_Random_ShouldReturnCorrectRank()
    {
        // Setup
        var rnd = new Random();

        var securities = Enumerable.Range(0, 1000).Select(i => (long)i).ToList();

        var indicator1 = Indicators.NetDebt;
        var indicator2 = Indicators.Return;
        var parameters = new StrategyInput
        {
            Hash = "",
            Date = new DateOnly(2022, 02, 10),
            RowSimilarityLimit = 0,
            Money = 0,
            MaxSecuritySpending = 0,
            StrategyVariables = new()
            {
                new()
                {
                    IndicatorId = indicator1,
                    Direction = Direction.Low
                },
                new()
                {
                    IndicatorId = indicator2,
                    Direction = Direction.High
                }
            }
        };

        var indicator1Data = securities.Select(s => new IndicatorData
        {
            SecurityId = s,
            IndicatorId = indicator1,
            Value = rnd.Next(10, 44)
        }).ToList();

        var indicator2Data = securities.Select(s => new IndicatorData
        {
            SecurityId = s,
            IndicatorId = indicator2,
            Value = rnd.Next(1000, 5000)
        }).ToList();

        var indicatorData = indicator1Data.Concat(indicator2Data).ToList();

        // Act
        var ranks = indicatorData.Rank(parameters);

        // Assert
        long prevRank = 0;
        decimal prevValue = 0;
        foreach (var rankVar in ranks)
        {
            securities.Should().Contain(rankVar.SecurityId);
            rankVar.Rank.Should().BeLessThanOrEqualTo(prevRank);
            rankVar.Value.Should().BeLessThanOrEqualTo(prevValue);
            prevRank = rankVar.Rank;
            prevValue = rankVar.Value;
        }
    }
}