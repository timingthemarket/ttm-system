using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using securities_masterdata.DataAccess.Services;
using Xunit;

namespace securities_masterdata.Tests;

public class NordnetApi_Test
{
    [Fact]
    public async Task GetStocks_ShouldReturnStocksWithTickerAndName()
    {
        // Arrange
        var client = new HttpClient();
        var nordnetApi = new NordnetService(client);

        // Act
        var response = await nordnetApi.GetStocksAsync(offset: 0, limit: 10);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response!.Results.Length);
        Assert.True(response.TotalHits > 10);
        Assert.All(response.Results, instrument =>
        {
            Assert.False(string.IsNullOrWhiteSpace(instrument.InstrumentInfo.Ticker));
            Assert.False(string.IsNullOrWhiteSpace(instrument.InstrumentInfo.Name));
        });
    }

    [Fact]
    public async Task GetStocks_ShouldPageWithOffset()
    {
        // Arrange
        var client = new HttpClient();
        var nordnetApi = new NordnetService(client);

        // Act
        var firstPage = await nordnetApi.GetStocksAsync(offset: 0, limit: 5);
        var secondPage = await nordnetApi.GetStocksAsync(offset: 5, limit: 5);

        // Assert
        Assert.NotNull(firstPage);
        Assert.NotNull(secondPage);

        var firstTickers = firstPage!.Results.Select(r => r.InstrumentInfo.Ticker).ToHashSet();
        var secondTickers = secondPage!.Results.Select(r => r.InstrumentInfo.Ticker).ToHashSet();

        Assert.Empty(firstTickers.Intersect(secondTickers));
    }
}
