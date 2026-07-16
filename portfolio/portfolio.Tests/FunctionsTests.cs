using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using portfolio.Domain.Constants;
using portfolio.Domain.Services;
using portfolio.Domain.Utils;
using Xunit;

namespace portfolio.Tests;

public class FunctionsTests
{
    [Fact]
    public void HashFunction_Should_Be_Consistent()
    {
        var obj1 = new { Test = "123" };
        var obj2 = new { Test = "123" };

        var hash1 = Functions.GetObjectHash(obj1);
        var hash2 = Functions.GetObjectHash(obj2);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashFunction_Should_Not_Be_The_Same()
    {
        var items1 = new List<string> { "123", "321" };
        var items2 = new List<string> { "321", "123" };
        
        var hash1 = Functions.GetObjectHash(items1);
        var hash2 = Functions.GetObjectHash(items2);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetNrPeriods_Should_Be_4()
    {
        var fromDate = new DateOnly(2020, 5, 1);
        var toDate = new DateOnly(2020, 5, 31);

        var periods = TimeIntervals.GetNrPeriods(TimeIntervals.Weekly, fromDate, toDate);

        periods.Should().Be(4);
    }

    [Fact]
    public void GenerateIndicators_Should_Generate_SameHash()
    {
        for (int i = 0; i < 1000; i++)
        {
            var test = IndicatorCombinationGenerator.GenerateIndicators();
            var hash1 = Functions.GetObjectHash(test);
            var hash2 = Functions.GetObjectHash(test);

            hash1.Should().Be(hash2);
        }
    }
}