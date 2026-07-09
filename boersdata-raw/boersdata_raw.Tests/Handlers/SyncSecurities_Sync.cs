using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Services;
using boersdata_raw.Domain.Handlers.Sync;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace boersdata_raw.Tests.Handlers;

public class SyncSecurities_Sync
{
    private readonly IBoersDataService _mockBoersDataService = Substitute.For<IBoersDataService>();
    private readonly ICountryRepository _mockCountryRepo = Substitute.For<ICountryRepository>();
    private readonly IMarketRepository _mockMarketRepo = Substitute.For<IMarketRepository>();

    private readonly ISectorRepository _mockSectorRepo = Substitute.For<ISectorRepository>();
    private readonly ISecuritiesRepository _mockSecuritiesRepo = Substitute.For<ISecuritiesRepository>();

    [Fact]
    public async Task ShouldHandleSyncSecurities()
    {
        // Arrange
        var _logger = Substitute.For<ILogger<SyncSecuritiesHandler>>();

        var client = new HttpClient();
        var bService = new BoersDataService(client);

        _mockSecuritiesRepo.SaveBatch(Arg.Any<List<Security>>(), Arg.Any<CancellationToken>()).Returns(2);

        var handler = new SyncSecuritiesHandler(_logger, bService, _mockSecuritiesRepo);

        // Act
        await handler.HandleSyncSecurities();

        // Assert
    }

    [Fact]
    public async Task ShouldHandleSyncSecuritiesMetadata()
    {
        // Arrange
        var _logger = Substitute.For<ILogger<SyncSecuritiesMetadataHandler>>();

        var client = new HttpClient();
        var bService = new BoersDataService(client);

        _mockSectorRepo.SaveBatch(Arg.Any<List<Sector>>(), Arg.Any<CancellationToken>()).Returns(2);
        _mockCountryRepo.SaveBatch(Arg.Any<List<Country>>(), Arg.Any<CancellationToken>()).Returns(2);
        _mockMarketRepo.SaveBatch(Arg.Any<List<Market>>(), Arg.Any<CancellationToken>()).Returns(2);

        var handler = new SyncSecuritiesMetadataHandler(_logger, bService, _mockCountryRepo,
            _mockMarketRepo, _mockSectorRepo);

        // Act
        await handler.HandleSyncMetadata();

        // Assert
    }
}