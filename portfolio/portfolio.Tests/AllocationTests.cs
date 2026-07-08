using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;
using portfolio.Domain.Models;
using portfolio.Domain.Portfolio.Factory.StrategyModules;
using portfolio.Domain.Utils;
using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;
using Xunit;

namespace portfolio.Tests;

public class AllocationTests
{
    private readonly List<InternalSecurityRank> _data = new()
    {
        new(new SecurityDto
            {
                SecurityId = 1,
                Sector = "S1",
                Country = "A"
            }, new SecurityPriceDto
            {
                SecurityId = 1,
                Low = 100,
                High = 200
            },
            new FunctionSecurityRank(1, 0, 10, 10), // WORST RANK IN SECTOR)
            0
        ),
        new(new SecurityDto
            {
                SecurityId = 2,
                Sector = "S1",
                Country = "B"
            }, new SecurityPriceDto
            {
                SecurityId = 2,
                Low = 70,
                High = 140
            },
            new FunctionSecurityRank(2, 0, 2, 80), // BEST RANK IN SECTOR,
            0
        ),
        new(new SecurityDto
            {
                SecurityId = 3,
                Sector = "S2",
                Country = "B"
            }, new SecurityPriceDto
            {
                SecurityId = 3,
                Low = 50,
                High = 100
            },
            new FunctionSecurityRank(3, 0, 3, 70),
            0
        )
    };


    [Fact]
    public void Allocation_ShouldBe_Successful()
    {
        var input = new StrategyInput
        {
            Hash = "",
            Money = 600,
            MaxSecuritySpending = 200
        };

        // Act
        var allocator = new DiLegacyLpAllocator(input, _data);
        var allocatedValues = allocator.AllocateWithOnlySecurityConstraint(out CpSolverStatus resultStatus);

        // Assert
        allocatedValues.Count.Should().Be(_data.Count);
        allocatedValues[0].Amount.Should().Be(1);
        allocatedValues[1].Amount.Should().Be(1);
        allocatedValues[2].Amount.Should().Be(2);
        resultStatus.Should().Be(CpSolverStatus.Optimal);
    }

    [Fact]
    public void Allocation_WithLessMoney_ShouldBe_Successful()
    {
        int maxInt = 100;
        var data = Enumerable.Range(1, maxInt).Select(e => new InternalSecurityRank(new SecurityDto
            {
                SecurityId = e
            }, new SecurityPriceDto
            {
                SecurityId = e,
                Low = e,
                High = e * 2
            },
            new FunctionSecurityRank(e, e, e, maxInt - e), 0)).ToList();
        

        var money = data.Count * 10.0m;
        var input = new StrategyInput
        {
            Hash = "",
            Money = money,
            MaxSecuritySpending = (double)money / data.Count
        };

        // Act
        var allocator = new DiLegacyLpAllocator(input, data);
        var allocatedValues = allocator.AllocateWithOnlySecurityConstraint(out CpSolverStatus resultStatus);

        // Assert
        allocatedValues.Count(a => a.Amount > 0).Should().Be(6);
        resultStatus.Should().Be(CpSolverStatus.Optimal);
        allocatedValues[0].Amount.Should().Be(5);
        allocatedValues[10].Amount.Should().Be(0);
    }

    [Fact]
    public void Allocation_WithEvenSectorsAndCountries_ShouldBe_Successful()
    {
        // There are 2 sectors in the data and 3 securities->
        // MaxMoneyPerSector 600/2 = 300
        // MaxMoneyPerCountry 600/2 = 300
        var input = new StrategyInput
        {
            Hash = "",
            Money = 600,
            MaxSecuritySpending = 200
        };

        // Act
        var allocator = new DiLegacyLpAllocator(input, _data);
        var allocatedValues =
            allocator.AllocateWithSectorCountrySecuritiesConstraint(out CpSolverStatus resultStatus);

        // Assert
        allocatedValues.Count.Should().Be(_data.Count);
        resultStatus.Should().Be(CpSolverStatus.Optimal);

        PortfolioValueDto security1 = allocatedValues[0];
        security1.Amount.Should().Be(1);
        double security1TotalPrice = security1.Price * security1.Amount;
        security1TotalPrice.Should().BeLessThanOrEqualTo(200);

        PortfolioValueDto security2 = allocatedValues[1];
        security2.Amount.Should().Be(1);
        double security2TotalPrice = security2.Price * security2.Amount;
        security2TotalPrice.Should().BeLessThanOrEqualTo(200);

        (security1TotalPrice + security2TotalPrice).Should().BeLessThanOrEqualTo(300);

        PortfolioValueDto security3 = allocatedValues[2];
        security3.Amount.Should().Be(2);
        (security3.Price * security3.Amount).Should().BeLessThanOrEqualTo(200);

        double totalAllocatedSum = allocatedValues.Sum(a => a.Amount * a.Price);
        totalAllocatedSum.Should().Be(405);
    }

    [Fact]
    public void Allocation_WithSectorsWeights_ShouldBe_Successful()
    {
        // There are 2 sectors in the data and 3 securities->
        // MaxMoneyPerSector with weight S1: 1000 * 0.5 = 500; S2: 1000 * 0.2 = 200
        // MaxMoneyPerSecurity 1000/3 = 333.33
        var input = new StrategyInput
        {
            Hash = "",
            Money = 1000,
            MaxSecuritySpending = 1000.0 / 3,
            SectorWeight = new Dictionary<string, double>
            {
                { "S1", 0.8 },
                { "S2", 0.2 }
            }
        };

        // Act
        var allocator = new DiLegacyLpAllocator(input, _data);
        var allocatedValues =
            allocator.AllocateWithSectorCountrySecuritiesConstraint(out CpSolverStatus resultStatus);

        // Assert
        allocatedValues.Count.Should().Be(_data.Count);
        resultStatus.Should().Be(CpSolverStatus.Optimal);

        PortfolioValueDto security1 = allocatedValues[0];
        security1.Amount.Should().Be(1);
        double security1TotalPrice = security1.Price * security1.Amount;
        security1TotalPrice.Should().BeLessThanOrEqualTo(333.33);

        PortfolioValueDto security2 = allocatedValues[1];
        security2.Amount.Should().Be(3);
        double security2TotalPrice = security2.Price * security2.Amount;
        security2TotalPrice.Should().BeLessThanOrEqualTo(333.33);

        (security1TotalPrice + security2TotalPrice).Should().BeLessThanOrEqualTo(500);

        PortfolioValueDto security3 = allocatedValues[2];
        security3.Amount.Should().Be(2);
        (security3.Price * security3.Amount).Should().BeLessThanOrEqualTo(333.33);

        double totalAllocatedSum = allocatedValues.Sum(a => a.Amount * a.Price);
        totalAllocatedSum.Should().Be(615);
    }

    [Fact]
    public void Allocation_WithCountriesWeights_ShouldBe_Successful()
    {
        // There are 2 sectors, 2 countries in the data and 3 securities->
        // MaxMoneyPerSector with weight 1000/2 = 500
        // MaxMoneyPerCountry with weight A: 1000 * 0.1 = 100; B: 1000 * 0.4 = 400
        // MaxMoneyPerSecurity 1000/3 = 333.33
        var input = new StrategyInput
        {
            Hash = "",
            Money = 1000,
            MaxSecuritySpending = 1000.0 / 3,
            CountryWeight = new Dictionary<string, double>
            {
                { "A", 0.1 },
                { "B", 0.4 }
            }
        };

        // Act
        var allocator = new DiLegacyLpAllocator(input, _data);
        var allocatedValues =
            allocator.AllocateWithSectorCountrySecuritiesConstraint(out CpSolverStatus resultStatus);

        // Assert
        allocatedValues.Count.Should().Be(_data.Count);
        resultStatus.Should().Be(CpSolverStatus.Optimal);

        PortfolioValueDto security1 = allocatedValues[0];
        security1.Amount.Should().Be(0);
        double security1TotalPrice = security1.Price * security1.Amount;
        security1TotalPrice.Should().BeLessThanOrEqualTo(333.33);

        PortfolioValueDto security2 = allocatedValues[1];
        security2.Amount.Should().Be(3);
        double security2TotalPrice = security2.Price * security2.Amount;
        security2TotalPrice.Should().BeLessThanOrEqualTo(333.33);

        (security1TotalPrice + security2TotalPrice).Should().BeLessThanOrEqualTo(500);

        PortfolioValueDto security3 = allocatedValues[2];
        security3.Amount.Should().Be(1);
        (security3.Price * security3.Amount).Should().BeLessThanOrEqualTo(300);

        double totalAllocatedSum = allocatedValues.Sum(a => a.Amount * a.Price);
        totalAllocatedSum.Should().Be(390);
    }

    /*[Fact]
    public void Allocation_WithTanHValues_ShouldBe_Successful()
    {
        int maxInt = 1000;
        var rnd = new Random();
        var data = Enumerable.Range(1, maxInt).Select(e =>
            {
                var priceLow = rnd.NextDouble() * (100 - 50) + 50;
                var priceHigh = rnd.NextDouble() * ((priceLow + 100) - priceLow) + priceLow;
                return new InternalSecurityRank(new SecurityDto
                    {
                        SecurityId = e
                    }, new SecurityPriceDto
                    {
                        SecurityId = e,
                        Low = priceLow,
                        High = priceHigh
                    },
                    new FunctionSecurityRank(e, e, e, 1), 0);
            }
        ).ToList();

        var ranks = data.Select(r => (double)r.Rank.Rank).ToList();
        var minRank = ranks.Min();
        var maxRank = ranks.Max();

        data = data.Select(d =>
        {
            var rankNoMean = Functions.RescaleWithBounds(d.Rank.Rank, minRank, maxRank, -2, 2);
            return d with { Rank = d.Rank with { FunctionConvertedRank = Functions.TanhReversed(0.5, rankNoMean) } };
        }).ToList();

        var money = data.Count * 10.0;
        var input = new StrategyInput
        {
            Money = (int)money,
            MaxSecuritySpending = money / 10
        };

        // Act
        var allocator = new DiLegacyLpAllocator(input, data);
        var allocatedValues = allocator.AllocateWithSectorCountrySecuritiesConstraint(out CpSolverStatus resultStatus);

        // Assert
        allocatedValues.Any(a => a.Amount > 0).Should().BeTrue();
        resultStatus.Should().Be(CpSolverStatus.OPTIMAL);
    }
    */

    [Fact]
    public void Allocation_With_Cost_ShouldBe_Successful()
    {
        //Setup
        var modifiedData = new List<InternalSecurityRank>(_data);
        modifiedData[0] = modifiedData[0] with { SecurityCost = 0};
        modifiedData[1] = modifiedData[1] with { SecurityCost = 1000};
        modifiedData[2] = modifiedData[2] with { SecurityCost = 500};

        var input = new StrategyInput
        {
            Hash = "",
            Money = 10_000,
            MaxSecuritySpending = 10_000.0
        };
        
        // Act
        var allocator = new DiLegacyLpAllocator(input, modifiedData);
        var allocatedValues = allocator.AllocateWithOnlySecurityConstraint(out var resultStatus);

        // Assert
        allocatedValues.Any(a => a.Amount > 0).Should().BeTrue();
        resultStatus.Should().Be(CpSolverStatus.Optimal);

        allocatedValues[0].Amount.Should().Be(0);
        allocatedValues[1].Amount.Should().Be(0);
    }
}

public class AllocationTestsLongRun
{
    [Fact]
    public void Allocation_With_AlotOfVariables_ShouldBe_Successful()
    {
        var input = new StrategyInput
        {
            Hash = "",
            Money = 1_000_000,
            MaxSecuritySpending = 200
        };

        int maxInt = 5000;
        var rnd = new Random();
        var data = Enumerable.Range(1, maxInt).Select(e =>
            {
                var priceLow = rnd.NextDouble() * (100 - 50) + 50;
                var priceHigh = rnd.NextDouble() * ((priceLow + 100) - priceLow) + priceLow;
                var convertedRank = rnd.NextDouble() * (500 - 1) + 1;
                return new InternalSecurityRank(new SecurityDto
                    {
                        SecurityId = e
                    }, new SecurityPriceDto
                    {
                        SecurityId = e,
                        Low = priceLow,
                        High = priceHigh
                    },
                    new FunctionSecurityRank(e, e, e, (int)convertedRank), 0);
            }
        ).ToList();

        // Act
        var allocator = new DiLegacyLpAllocator(input, data);
        var allocatedValues = allocator.AllocateWithOnlySecurityConstraint(out CpSolverStatus resultStatus);

        // Assert
        allocatedValues.Count.Should().Be(data.Count);
        resultStatus.Should().Be(CpSolverStatus.Optimal);
        allocatedValues.Select(a => a.Amount).Sum().Should().BeGreaterThan(0);
    }
}