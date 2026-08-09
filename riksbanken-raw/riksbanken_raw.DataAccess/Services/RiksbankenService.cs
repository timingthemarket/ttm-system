using System.Collections.Specialized;
using riksbanken_raw.DataAccess.Extensions;
using riksbanken_raw.DataAccess.Interfaces;
using riksbanken_raw.DataAccess.Models;

namespace riksbanken_raw.DataAccess.Services;

public class RiksbankenService : IRiksbankenService
{
    private const string BaseApiUrl = "https://api.riksbank.se/swea/v1/";
    private const string FromDateHistorical = "2010-01-01";

    private readonly string _apiKey;
    private readonly HttpClient _client;

    public RiksbankenService(HttpClient client) 
    {
        _client = client;
        _apiKey = Environment.GetEnvironmentVariable("RIKSBANKEN_DATA_API_KEY") ?? 
                  throw new Exception("No environment variable 'RIKSBANKEN_DATA_API_KEY' was found");
        
        _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
        client.BaseAddress = new Uri(BaseApiUrl);
    }

    public async Task<IReadOnlyList<RiksbankenObservation>> GetHistoricalObservations(string seriesId)
    {
        string path = $"Observations/{seriesId}/{FromDateHistorical}";
        var payload = await _client.GetJson<List<RiksbankenObservation>>(path);
        return payload ?? new List<RiksbankenObservation>();
    }

    public async Task<RiksbankenObservation> GetLatestObservation(string seriesId)
    {
        string path = $"Observations/Latest/{seriesId}";
        var payload = await _client.GetJson<RiksbankenObservation>(path);
        return payload ?? new RiksbankenObservation();
    }
}