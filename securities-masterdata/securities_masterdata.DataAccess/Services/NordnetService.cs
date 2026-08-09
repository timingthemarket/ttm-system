using System.Text.Json;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.DataAccess.Services.Models;

namespace securities_masterdata.DataAccess.Services;

public class NordnetService : INordnetService
{
    private readonly HttpClient _httpClient;

    public NordnetService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.nordnet.se/");

        // The stocklist endpoint is public, but Nordnet only serves it to requests that look
        // like they come from their own web client.
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:153.0) Gecko/20100101 Firefox/153.0");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.nordnet.se/");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("client-id", "NEXT");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-nn-href", "https://www.nordnet.se/aktier/kurser");
    }

    public async Task<NordnetStocklistResponse?> GetStocksAsync(
        int offset = 0,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/2/instrument_search/query/stocklist?limit={limit}&offset={offset}";

        var response = await _httpClient.GetAsync(endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<NordnetStocklistResponse>(responseContent);
    }
}
