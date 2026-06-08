using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Factory.Functions;
using TTM.Shared.Models;
using Xunit;

namespace securities_masterdata.Tests.IndicatorFunctions;

public class VolatilityFunctionTest
{
    [Fact]
    public async Task VolatilityFunction()
    {
        // Arrange
        var date = new DateOnly(2020, 4, 10);
        var lookbackPeriod = new LookBackPeriod { Period = 30 };
        var fromDate1YearsBack = date.AddDays(-2 * lookbackPeriod.Period);

        var rnd = new Random();
        
        var securityIds = new HashSet<long> { 1 };
        var datePrices = Enumerable.Range(0, 1000).Select(dp =>
        {
            var value = rnd.Next(-1000, 1000);
            
            return new SecurityPrice
            {
                SecurityId = 1,
                Close = dp,
                Date = date.AddDays(-dp)
            };
        }).ToList();

        var securities = securityIds.Select(s => new Security { SecurityId = s }).ToList();
        
        var repo = Substitute.For<ISecurityRepository>();
        repo.GetSecuritiesPricesHistory(Arg.Any<HashSet<long>>(), fromDate1YearsBack, date).Returns(datePrices);

        var function = new VolatilityFunction(repo);

        // Act
        var indicators = await function.Process(securities, date, lookbackPeriod);

        // Assert
        indicators.Should().ContainSingle();
    }
    
    [Fact]
    public async Task VolatilityFunctionShouldBeSuccessful()
    {
        // Arrange
        var date = new DateOnly(2020, 4, 10);
        var lookbackPeriod = new LookBackPeriod { Period = 350 };
        var fromDate1YearsBack = date.AddDays(-2 * lookbackPeriod.Period);

        var securities = Enumerable.Range(0, 1000).Select(s => new Security { SecurityId = s }).ToList();
        var securityIds = securities.Select(l => l.SecurityId).ToHashSet();
        
        var datePrices = Helpers.GenerateSecurityPrices(securityIds.ToList(), lookbackPeriod.Period * 2, date);

        var repo = Substitute.For<ISecurityRepository>();
        repo.GetSecuritiesPricesHistory(Arg.Any<HashSet<long>>(), fromDate1YearsBack, date).Returns(datePrices);

        var function = new VolatilityFunction(repo);

        // Act
        var indicators = await function.Process(securities, date, lookbackPeriod);

        // Assert
        Assert.Equal(securityIds.Count, indicators.Count);
    }
}