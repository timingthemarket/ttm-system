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

public class ReturnFunctionTest
{
    [Fact]
    public async Task ReturnFunctionShouldBeSuccessful()
    {
        // Arrange
        var date = new DateOnly(2020, 4, 10);
        var lookbackPeriod = new LookBackPeriod { Period = 350 };
        var fromDate1YearBack = date.AddDays(-lookbackPeriod.Period);
        var toDate1YearBack = fromDate1YearBack.AddDays(4);

        var securities = Enumerable.Range(0, 1000).Select(s => new Security { SecurityId = s }).ToList();
        var securityIds = securities.Select(s => s.SecurityId).ToList();
        
        var year1Prices = Helpers.GenerateSecurityPrices(securityIds, 10, toDate1YearBack);
        var datePrices = Helpers.GenerateSecurityPrices(securityIds, 10, date);

        var repo = Substitute.For<ISecurityRepository>();
        
        repo.GetSecuritiesPricesHistory(Arg.Any<HashSet<long>>(), fromDate1YearBack, toDate1YearBack).Returns(year1Prices);
        repo.GetSecuritiesPricesHistory(Arg.Any<HashSet<long>>(), date.AddDays(-4), date).Returns(datePrices);

        var function = new ReturnFunction(repo);

        // Act
        var indicators = await function.Process(securities, date, lookbackPeriod);

        // Assert
        Assert.Equal(securityIds.Count, indicators.Count);
    }

    [Fact]
    public async Task ReturnFunctionShouldReturnCorrectResponse()
    {
        // Arrange
        var date = new DateOnly(2020, 4, 10);
        var lookbackPeriod = new LookBackPeriod { Period = 100};
        var fromDate1YearBack = date.AddDays(-lookbackPeriod.Period);
        var toDate1YearBack = fromDate1YearBack.AddDays(4);

        var lookBackPrices = new List<SecurityPrice>
        {
            new()
            {
                SecurityId = 1,
                Date = date.AddDays(-lookbackPeriod.Period),
                Close = 20
            },
            new()
            {
                SecurityId = 2,
                Date = date.AddDays(-lookbackPeriod.Period),
                Close = 50
            },
            new()
            {
                SecurityId = 3,
                Date = date.AddDays(-lookbackPeriod.Period),
                Close = 100
            },
            new()
            {
                SecurityId = 4,
                Date = date.AddDays(-lookbackPeriod.Period),
                Close = 100
            }
        };
        
        var datePrices = new List<SecurityPrice>
        {
            new()
            {
                SecurityId = 1,
                Date = date,
                Close = 10
            },
            new()
            {
                SecurityId = 2,
                Date = date,
                Close = 50
            },
            new()
            {
                SecurityId = 3,
                Date = date,
                Close = 10
            },
            new()
            {
                SecurityId = 4,
                Date = date,
                Close = 110
            }
        };

        var answers = new List<decimal> { -0.5m, 0, -0.9m, 0.1m };

        var securities = datePrices.Select(s => new Security { SecurityId = s.SecurityId }).ToList();
        
        var repo = Substitute.For<ISecurityRepository>();

        repo.GetSecuritiesPricesHistory(Arg.Any<HashSet<long>>(), fromDate1YearBack, toDate1YearBack).Returns(lookBackPrices);
        repo.GetSecuritiesPricesHistory(Arg.Any<HashSet<long>>(), date.AddDays(-4), date).Returns(datePrices);

        var function = new ReturnFunction(repo);
        
        // Act
        var indicators = await function.Process(securities, date, lookbackPeriod);

        // Assert
        indicators.Count.Should().Be(datePrices.Count);

        for (int i = 0; i < datePrices.Count; i++)
        {
            var indicator = indicators[i];
            var securityPrice = datePrices[i];
            indicator.SecurityId.Should().Be(securityPrice.SecurityId);
            indicator.Date.Should().Be(securityPrice.Date);

            var answer = answers[i];
            indicator.Value.Should().Be(answer);
        }
    }
}