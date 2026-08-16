using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using portfolio.Domain.Constants;
using portfolio.Domain.Utils;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using Xunit;

namespace portfolio.Tests;

public class IndicatorStrengthDateTests
{
    [Fact]
    public void ForMonth_Should_Use_The_15th_When_It_Is_A_Weekday()
    {
        // 2026-09-15 is a Tuesday
        RebalanceDates.ForMonth(2026, 9).Should().Be(new DateOnly(2026, 9, 15));
    }

    [Fact]
    public void ForMonth_Should_Step_Back_To_Friday_When_The_15th_Is_A_Saturday()
    {
        // 2026-08-15 is a Saturday
        var date = RebalanceDates.ForMonth(2026, 8);

        date.Should().Be(new DateOnly(2026, 8, 14));
        date.DayOfWeek.Should().Be(DayOfWeek.Friday);
    }

    [Fact]
    public void ForMonth_Should_Step_Back_Twice_When_The_15th_Is_A_Sunday()
    {
        // 2026-02-15 is a Sunday, the 14th a Saturday
        var date = RebalanceDates.ForMonth(2026, 2);

        date.Should().Be(new DateOnly(2026, 2, 13));
        date.DayOfWeek.Should().Be(DayOfWeek.Friday);
    }

    [Fact]
    public void Generate_Should_Never_Land_On_A_Weekend()
    {
        var dates = RebalanceDates.Generate(new DateOnly(2026, 8, 15), 12);

        dates.Should().OnlyContain(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday);
        dates.Should().OnlyContain(d => d.Day >= 13 && d.Day <= 15);
    }

    [Fact]
    public void Generate_Should_Produce_One_Date_Per_Month_Inclusive_Of_Both_Ends()
    {
        var dates = RebalanceDates.Generate(new DateOnly(2026, 8, 15), 12);

        dates.Should().HaveCount(12 * 12 + 1);
        dates.Should().BeInAscendingOrder();
        dates.Should().OnlyHaveUniqueItems();
        dates.First().Should().Be(RebalanceDates.ForMonth(2014, 8));
        dates.Last().Should().Be(RebalanceDates.ForMonth(2026, 8));
    }

    [Fact]
    public void Generate_Should_Return_The_Current_Month_Only_For_Zero_Years()
    {
        var dates = RebalanceDates.Generate(new DateOnly(2026, 8, 15), 0);

        dates.Should().ContainSingle().Which.Should().Be(new DateOnly(2026, 8, 14));
    }
}

public class SectorSelectionTests
{
    private sealed record Row(string Sector, double Value);

    private static List<Row> Sector(string name, int count) =>
        Enumerable.Range(1, count).Select(i => new Row(name, i)).ToList();

    [Fact]
    public void TopPerSector_Should_Always_Take_At_Least_One_From_A_Small_Sector()
    {
        // 7 * 0.10 = 0.7 -> ceiling 1
        var selected = SectorSelection.TopPerSector(Sector("Tech", 7),
            r => r.Sector, r => r.Value, Direction.High, 0.10);

        selected.Should().ContainSingle().Which.Value.Should().Be(7);
    }

    [Fact]
    public void TopPerSector_Should_Round_The_Decile_Up()
    {
        // 25 * 0.10 = 2.5 -> ceiling 3
        var selected = SectorSelection.TopPerSector(Sector("Tech", 25),
            r => r.Sector, r => r.Value, Direction.High, 0.10);

        selected.Select(r => r.Value).Should().BeEquivalentTo(new[] { 25.0, 24.0, 23.0 });
    }

    [Fact]
    public void TopPerSector_Should_Take_The_Smallest_Values_For_Direction_Low()
    {
        var selected = SectorSelection.TopPerSector(Sector("Tech", 25),
            r => r.Sector, r => r.Value, Direction.Low, 0.10);

        selected.Select(r => r.Value).Should().BeEquivalentTo(new[] { 1.0, 2.0, 3.0 });
    }

    [Fact]
    public void TopPerSector_Should_Pick_From_Every_Sector_Independently()
    {
        List<Row> universe = Sector("Tech", 20).Concat(Sector("Bank", 5)).ToList();

        var selected = SectorSelection.TopPerSector(universe,
            r => r.Sector, r => r.Value, Direction.High, 0.10);

        selected.Should().HaveCount(3); // 2 from Tech, 1 from Bank
        selected.Where(r => r.Sector == "Tech").Select(r => r.Value)
            .Should().BeEquivalentTo(new[] { 20.0, 19.0 });
        selected.Where(r => r.Sector == "Bank").Select(r => r.Value)
            .Should().BeEquivalentTo(new[] { 5.0 });
    }

    [Fact]
    public void TopPerSector_Should_Reject_An_Invalid_Fraction()
    {
        var act = () => SectorSelection.TopPerSector(Sector("Tech", 10),
            r => r.Sector, r => r.Value, Direction.High, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class StrengthStatisticsTests
{
    [Fact]
    public void Sharpe_Should_Match_A_Hand_Computed_Series()
    {
        var returns = new List<double> { 0.01, 0.03, 0.05, 0.01, 0.03, 0.05 };

        var sharpe = StrengthStatistics.Sharpe(returns);

        // Mean 0.03; sample variance (n - 1) = 4 * 0.0004 / 5 = 0.00032.
        var mean = returns.Sum() / returns.Count;
        var sampleStdDev = Math.Sqrt(returns.Sum(r => Math.Pow(r - mean, 2)) / (returns.Count - 1));
        var expected = mean / sampleStdDev * Math.Sqrt(12);

        sampleStdDev.Should().BeApproximately(Math.Sqrt(0.00032), 1e-12);
        sharpe.Should().NotBeNull();
        sharpe!.Value.Should().BeApproximately(expected, 1e-9);
    }

    [Fact]
    public void Sharpe_Should_Be_Null_When_A_Constant_Series_Leaves_Floating_Point_Noise()
    {
        // A repeated value does not give an exactly zero standard deviation, and dividing by that
        // residue would produce a Sharpe ratio in the 1e16 range.
        var sharpe = StrengthStatistics.Sharpe(Enumerable.Repeat(0.0123456789, 24).ToList());

        sharpe.Should().BeNull();
    }

    [Fact]
    public void Sharpe_Should_Be_Null_Below_The_Minimum_Number_Of_Observations()
    {
        var returns = Enumerable.Range(1, StrengthStatistics.MinObservations - 1)
            .Select(i => i * 0.01).ToList();

        StrengthStatistics.Sharpe(returns).Should().BeNull();
    }

    [Fact]
    public void Sharpe_Should_Be_Null_When_The_Returns_Have_No_Dispersion()
    {
        StrengthStatistics.Sharpe(Enumerable.Repeat(0.02, 12).ToList()).Should().BeNull();
    }

    [Fact]
    public void InformationCoefficient_Should_Be_One_For_A_Perfect_Rank_Match()
    {
        var scores = new List<double> { 1, 2, 3, 4, 5, 6, 7 };
        var forward = new List<double> { 0.01, 0.02, 0.05, 0.06, 0.09, 0.11, 0.30 };

        StrengthStatistics.InformationCoefficient(scores, forward)!.Value
            .Should().BeApproximately(1.0, 1e-9);
    }

    [Fact]
    public void InformationCoefficient_Should_Be_Minus_One_For_A_Perfectly_Inverted_Rank()
    {
        var scores = new List<double> { 1, 2, 3, 4, 5, 6, 7 };
        var forward = new List<double> { 0.30, 0.11, 0.09, 0.06, 0.05, 0.02, 0.01 };

        StrengthStatistics.InformationCoefficient(scores, forward)!.Value
            .Should().BeApproximately(-1.0, 1e-9);
    }

    [Fact]
    public void InformationCoefficient_Should_Be_Null_When_One_Side_Is_Entirely_Tied()
    {
        var scores = Enumerable.Repeat(1.0, 10).ToList();
        var forward = Enumerable.Range(1, 10).Select(i => i * 0.01).ToList();

        StrengthStatistics.InformationCoefficient(scores, forward).Should().BeNull();
    }

    [Fact]
    public void InformationCoefficient_Should_Be_Null_For_Mismatched_Lengths()
    {
        StrengthStatistics.InformationCoefficient(
            new List<double> { 1, 2, 3, 4, 5, 6, 7 },
            new List<double> { 1, 2, 3 }).Should().BeNull();
    }

    [Fact]
    public void RollingWindow_Should_Never_Include_The_Observation_At_The_Scored_Index()
    {
        // The observation at index i covers the month starting at date i, which is only knowable a
        // month later. Including it would be lookahead.
        var observations = Enumerable.Range(0, 30).Select(i => (double?)i).ToArray();

        var window = StrengthStatistics.RollingWindow(observations, 20);

        window.Should().NotContain(20);
        window.Should().HaveCount(StrengthStatistics.RollingWindowMonths);
        window.Should().BeEquivalentTo(Enumerable.Range(8, 12).Select(i => (double)i));
    }

    [Fact]
    public void RollingWindow_Should_Skip_Gaps_And_Clamp_At_The_Start_Of_The_Series()
    {
        var observations = new double?[] { 1, null, 3, 4 };

        StrengthStatistics.RollingWindow(observations, 3)
            .Should().BeEquivalentTo(new[] { 1.0, 3.0 });
        StrengthStatistics.RollingWindow(observations, 0).Should().BeEmpty();
    }

}

public class IndicatorStrengthSetsTests
{
    [Fact]
    public void Sets_Should_Cover_Every_Supported_Calculation_Indicator()
    {
        IndicatorStrengthSets.Sets.Select(s => s.Indicator).Distinct()
            .Should().BeEquivalentTo(SupportedCalculationIndicators.SupportedIndicators);
    }

    [Fact]
    public void Sets_Should_Test_Volatility_And_RsiMomentum_In_Both_Directions()
    {
        foreach (var indicator in new[] { Indicators.Volatility, Indicators.RsiMomentum })
            IndicatorStrengthSets.Sets.Where(s => s.Indicator == indicator).Select(s => s.Direction)
                .Should().BeEquivalentTo(new[] { Direction.High, Direction.Low });
    }

    [Fact]
    public void Sets_Should_Be_Uniquely_Keyed()
    {
        IndicatorStrengthSets.Sets.Select(s => s.Key).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Non_Computed_Indicators_Must_Aggregate_Or_Their_Look_Back_Period_Is_Ignored()
    {
        // Masterdata reads non-computed indicators straight from the indicators table and only
        // applies the look back period when the aggregator is Average or Sum.
        foreach (var set in IndicatorStrengthSets.Sets.Where(s => !s.Indicator.IsComputedIndicator()))
            set.Aggregate.Should().BeOneOf(Aggregator.Average, Aggregator.Sum);
    }

    [Fact]
    public void Look_Back_Periods_Should_Match_The_Configured_Time_Spans()
    {
        var expected = new Dictionary<Indicators, int>
        {
            [Indicators.Dividend] = 365,
            [Indicators.Volatility] = 60,
            [Indicators.Pe] = 365,
            [Indicators.Return] = 180,
            [Indicators.RsiMomentum] = 90,
            [Indicators.Roc] = 365,
            [Indicators.Roic] = 365,
            [Indicators.FScore] = 365
        };

        foreach (var set in IndicatorStrengthSets.Sets)
            set.LookBackDays.Should().Be(expected[set.Indicator], "look back for {0}", set.Indicator);
    }
}
