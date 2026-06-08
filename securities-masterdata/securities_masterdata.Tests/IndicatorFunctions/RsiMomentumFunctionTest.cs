using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Factory.Functions;
using TTM.Shared.Models;
using Xunit;

namespace securities_masterdata.Tests.IndicatorFunctions;

public class RsiMomentumFunctionTest
{
    [Fact]
    public async Task RsiMomentumFunctionShouldBeSuccessful()
    {
        // Arrange
        var date = new DateOnly(2020, 4, 10);
        var lookbackPeriod = new LookBackPeriod { Period = 30 };

        var securities = Enumerable.Range(0, 1000).Select(s => new Security {SecurityId = s} ).ToList();

        var securityIds = securities.Select(s => s.SecurityId).ToList();
        var prices = Helpers.GenerateSecurityPrices(securityIds, lookbackPeriod.Period * 10, date);

        var repo = Substitute.For<ISecurityRepository>();

        repo.GetSecuritiesPricesHistory(Arg.Any<HashSet<long>>(), Arg.Any<DateOnly>(), date).Returns(prices);

        var function = new RsiMomentumFunction(repo);

        // Act
        var indicators = await function.Process(securities, date, lookbackPeriod);

        // Assert
        Assert.Equal(securityIds.Count, indicators.Count);
    }
}