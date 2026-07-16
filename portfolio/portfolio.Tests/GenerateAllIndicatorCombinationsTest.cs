using System;
using System.Linq;
using FluentAssertions;
using portfolio.Domain.Services;
using portfolio.Domain.Utils;
using TTM.Shared.Constants;
using Xunit;

namespace portfolio.Tests;

public class GenerateAllIndicatorCombinationsTest
{
    [Fact]
    public void GenerateAllIndicatorCombinations_ShouldReturnCorrectCombinations()
    {
        // Arrange
        // Act
        var combinations = IndicatorCombinationGenerator.GenerateAllIndicatorCombinations();

        // Assert
        var fScoreCombos = combinations.Where(c => c.Any(cc => cc.IndicatorId == Indicators.FScore))
            .ToList();
        
        fScoreCombos.Any(c => c.Count(cc => cc.IndicatorId == Indicators.FScore) > 1)
            .Should().BeFalse();
    }

    [Fact]
    public void GenerateAllIndicatorCombinations_ShouldAllBeUnique()
    {
        // Arrange
        // Act
        var combinations = IndicatorCombinationGenerator.GenerateAllIndicatorCombinations();

        // Assert
        var nrUnique = combinations
            .Select(c =>  c.Select(cc => cc.ToStringRepresentation()))
            .Distinct()
            .Count();

        nrUnique.Should().Be(combinations.Count);
    }
    
    [Fact]
    public void GenerateCombinationsSHouldBeSameAsHashes()
    {
        // Arrange
        // Act
        var combinations = IndicatorCombinationGenerator.GenerateAllIndicatorCombinations();

        var inputs = IndicatorCombinationGenerator.GetInputsWithHashes(DateOnly.FromDayNumber(20), combinations, 0.001, 10_000, 100);
        
        // Debug: Find exact duplicates
        var serializedCombinations = combinations.Select((comb, index) => new 
        { 
            Index = index,
            Serialized = string.Join(",", comb.Select(c => c.ToStringRepresentation()))
        }).ToList();
        
        var duplicateGroups = serializedCombinations
            .GroupBy(x => x.Serialized)
            .Where(g => g.Count() > 1)
            .Take(3)
            .ToList();
            
        if (duplicateGroups.Any())
        {
            var firstGroup = duplicateGroups.First();
            throw new InvalidOperationException($"Found duplicate combinations: '{firstGroup.Key}' appears {firstGroup.Count()} times at indices: {string.Join(", ", firstGroup.Select(x => x.Index))}");
        }
        
        // Assert
        var key = inputs.Keys.First();
        key.Should().Be("E442C9CFD90149250493668111020444");
        combinations.Count.Should().Be(inputs.Keys.Count);
    }
}