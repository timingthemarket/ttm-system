using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Constants;
using portfolio.Domain.Models;
using portfolio.Domain.Portfolio.Factory.StrategyModules;
using TTM.Shared.Constants;
using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;
using Xunit;

namespace portfolio.Tests;

public class DiLegacyStrategyTest
{
    [Fact]
    public void Test()
    {
        var test = new List<(int Rank, string Sector, int SecurityId)>
        {
            new ()
            {
                SecurityId = 111,
                 Rank = 1,
                 Sector = "123"
            },
            new()
            {
                SecurityId = 222,
                Rank = 2,
                Sector = "321"
            },
            new()
            {
                SecurityId = 333,
                Rank = 3,
                Sector = "123"
            },
            new()
            {
                SecurityId = 444,
                Rank = 4,
                Sector = "321"
            },
            new()
            {
                SecurityId = 555,
                Rank = 1,
                Sector = "abc"
            },
            new()
            {
                SecurityId = 666,
                Rank = 6,
                Sector = "321"
            }
        };


        var ordered = test.OrderBy(t => t.Rank)
            .GroupBy(t => t.Sector)
            .ToList();
    }
    
    [Fact]
    public async Task DiLegacyStrategy_Should_Be_Success()
    {
        // Arrange
        var date = new DateOnly(2000, 1, 10);

        long securityId1 = 1;
        long securityId2 = 2;
        long securityId3 = 3;

        var indicatorsResponse = new SecuritiesIndicatorsQryResponse
        {
            Variables = new List<SecurityIndicatorDto>
            {
                new()
                {
                    Date = date,
                    Value = 22,
                    IndicatorId = Indicators.Dividend,
                    SecurityId = securityId1
                },
                new()
                {
                    Date = date,
                    Value = 100,
                    IndicatorId = Indicators.Eps,
                    SecurityId = securityId1
                },
                new()
                {
                    Date = date,
                    Value = 1,
                    IndicatorId = Indicators.Dividend,
                    SecurityId = securityId2
                },
                new()
                {
                    Date = date,
                    Value = 2,
                    IndicatorId = Indicators.Eps,
                    SecurityId = securityId2
                },
                new()
                {
                    Date = date,
                    Value = 33,
                    IndicatorId = Indicators.Dividend,
                    SecurityId = securityId3
                },
                new()
                {
                    Date = date,
                    Value = 234,
                    IndicatorId = Indicators.Eps,
                    SecurityId = securityId3
                }
            },
            Date = date
        };

        var securitiesResponse = new SecuritiesQryResponse
        {
            Securities = new List<SecurityDto>
            {
                new()
                {
                    SecurityId = securityId1,
                    Country = "Sweden",
                    Sector = "Food",
                    Name = "A company 1"
                },
                new()
                {
                    SecurityId = securityId2,
                    Country = "Norway",
                    Sector = "Food",
                    Name = "A company 2"
                },
                new()
                {
                    SecurityId = securityId3,
                    Country = "Sweden",
                    Sector = "Tech",
                    Name = "A company 3"
                }
            }
        };

        var securitiesPrices = new SecuritiesPricesQryResponse
        {
            SecurityPrices = new ()
            {
                new ()
                {
                    SecurityId = securityId1,
                    Date = date,
                    High = 100,
                    Low = 50,
                    Close = 100
                },
                new ()
                {
                    SecurityId = securityId2,
                    Date = date,
                    High = 100,
                    Low = 50,
                    Close = 100
                },
                new ()
                {
                    SecurityId = securityId3,
                    Date = date,
                    High = 100,
                    Low = 50,
                    Close = 100
                }
            }
        };

        var logger = Substitute.For<ILogger<DiLegacyStrategy>>();
        var masteradataService = Substitute.For<IMasterdataService>();

        masteradataService.GetIndicators(date, Arg.Any<List<SecuritiesIndicatorQryMetadataDto>>())
            .Returns(indicatorsResponse);
        masteradataService.GetSecurites(null, null).Returns(securitiesResponse);
        masteradataService.GetLatestPrices(date, null).Returns(securitiesPrices);

        var strategy = new DiLegacyStrategy(logger, masteradataService);
        // Act

        var portfolio = await strategy.Compute(new StrategyInput
        {
            Date = date,
            RowSimilarityLimit = 0.001,
            Money = 10_000,
            Hash = "",
            MaxSecuritySpending = 1000,
            StrategyVariables = new List<StrategyInputVariable>
            {
                new()
                {
                    IndicatorId = Indicators.Dividend,
                    Direction = Direction.High
                },
                new()
                {
                    IndicatorId = Indicators.Eps,
                    Direction = Direction.High
                }
            }
        });

        // Assert
        portfolio.PortfolioValues[0].SecurityId.Should().Be(securityId3);
    }
}