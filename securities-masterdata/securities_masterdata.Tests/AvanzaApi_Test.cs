using System.Net.Http;
using System.Threading.Tasks;
using securities_masterdata.DataAccess.Services;
using Xunit;

namespace securities_masterdata.Tests;

public class AvanzaApi_Test
{
    [Fact]
    public async Task GetStocks_ShouldReturnStocks()
    {
        // Arrange
        var client = new HttpClient();
        var avanzaApi = new AvanzaService(client);
        
        // Act
        var stocks = await avanzaApi.GetStocksAsync();

        // Assert
        
    }
}