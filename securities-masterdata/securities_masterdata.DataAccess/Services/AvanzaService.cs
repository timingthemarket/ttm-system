using System.Text;
using System.Text.Json;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.DataAccess.Services.Models;

namespace securities_masterdata.DataAccess.Services;

public class AvanzaService : IAvanzaService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AvanzaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.avanza.se/");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<AvanzaStockFilterResponse?> GetStocksAsync(AvanzaStockFilterRequest request, CancellationToken cancellationToken = default)
    {
        const string endpoint = "_api/market-stock-filter/stocks";
        
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<AvanzaStockFilterResponse>(responseContent, _jsonOptions);
    }

    public async Task<AvanzaStockFilterResponse?> GetStocksAsync(
        int offset = 0, 
        int limit = 10, 
        string sortField = "numberOfOwners", 
        string sortOrder = "desc", 
        CancellationToken cancellationToken = default)
    {
        var request = new AvanzaStockFilterRequest
        {
            Filter = new AvanzaFilter
            {
                Sectors = [],
                MarketPlaces = []
            },
            Offset = offset,
            Limit = limit,
            SortBy = new AvanzaSortBy
            {
                Field = sortField,
                Order = sortOrder
            }
        };

        return await GetStocksAsync(request, cancellationToken);
    }
}