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

public class Omx30FunctionTest
{
    [Theory]
    [InlineData(true, 1.0)]
    [InlineData(false, 1.0)]
    public async Task Omx30Function_Beta_Should_Succeed(bool isSame, decimal betaValue)
    {
        // Arrange
        var date = new DateOnly(2020, 4, 10);
        var lookbackPeriod = new LookBackPeriod { Period = 2};
        
        var rnd = new Random();

        var securityIds = new HashSet<long> { 1 };
        var securities = securityIds.Select(s => new Security { SecurityId = s }).ToList();
        
        var securityPrices = Enumerable.Range(0, 1000).Select(row => new SecurityPrice
        {
            SecurityId = 1,
            Close = rnd.Next(-1000, 1000),
            Date = date.AddDays(-row)
        }).ToList();

        var indexPrices = securityPrices.Select(sp => new IndexValue
        {
            IndexId = 1,
            Date = sp.Date,
            Value = isSame ? (decimal)sp.Close : -1 * (decimal)sp.Close
        }).ToList();

        var securityRepo = Substitute.For<ISecurityRepository>();
        var indexRepo = Substitute.For<IIndexRepository>();

        securityRepo.GetSecuritiesPricesHistory(Arg.Any<HashSet<long>>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns(securityPrices);
        indexRepo.GetIndexValues(1, Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns(indexPrices);

        var function = new BetaOmx30Function(securityRepo, indexRepo);

        // Act
        var indicators = await function.Process(securities, date, lookbackPeriod);

        // Assert
        indicators.Should().ContainSingle();
        indicators[0].Date.Should().Be(date);
        indicators[0].Value.Should().Be(betaValue);
    }
}