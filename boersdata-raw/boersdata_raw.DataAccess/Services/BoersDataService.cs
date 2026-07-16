using System.Collections.Specialized;
using boersdata_raw.DataAccess.Extensions;
using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.BoersDataApi;

namespace boersdata_raw.DataAccess.Services;

public sealed class BoersDataService : IBoersDataService
{
    private const string ApiVersion = "1";
    private readonly HttpClient _client;

    private readonly string _apiKey;

    public BoersDataService(HttpClient client)
    {
        _client = client;
        _apiKey = Environment.GetEnvironmentVariable("BOERSDATA_API_KEY") ?? throw new Exception("The environment variable 'BOERSDATA_API_KEY' is null");
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL_API") ?? "https://apiservice.borsdata.se/";

        client.BaseAddress = new Uri(baseUrl);
    }

    public async Task<IReadOnlyList<BoersDataInstrument>> GetNordicInstruments()
    {
        var payload = await _client.GetJson<BoersDataInstruments>($"/v{ApiVersion}/instruments",
            new NameValueCollection { { "authKey", _apiKey } });
        return payload?.Instruments ?? new List<BoersDataInstrument>();
    }

    public async Task<IReadOnlyList<BoersDataInstrument>> GetGlobalInstruments()
    {
        var payload =
            await _client.GetJson<BoersDataInstruments>($"/v{ApiVersion}/instruments/global",
                new NameValueCollection { { "authKey", _apiKey } });
        return payload?.Instruments ?? new List<BoersDataInstrument>();
    }

    public async Task<IReadOnlyList<BoersDataIndustry>> GetIndustries()
    {
        var payload = await _client.GetJson<BoersDataIndustries>($"/v{ApiVersion}/branches",
            new NameValueCollection { { "authKey", _apiKey } });
        return payload?.Branches ?? new List<BoersDataIndustry>();
    }

    public async Task<IReadOnlyList<BoersDataCountry>> GetCountries()
    {
        var payload =
            await _client.GetJson<BoersDataCountries>($"/v{ApiVersion}/countries",
                new NameValueCollection { { "authKey", _apiKey } });
        return payload?.Countries ?? new List<BoersDataCountry>();
    }

    public async Task<IReadOnlyList<BoersDataMarket>> GetMarkets()
    {
        var payload =
            await _client.GetJson<BoersDataMarkets>($"/v{ApiVersion}/markets",
                new NameValueCollection { { "authKey", _apiKey } });
        return payload?.Markets ?? new List<BoersDataMarket>();
    }

    public async Task<IReadOnlyList<BoersDataSector>> GetSectors()
    {
        var payload =
            await _client.GetJson<BoersDataSectors>($"/v{ApiVersion}/sectors",
                new NameValueCollection { { "authKey", _apiKey } });
        return payload?.Sectors ?? new List<BoersDataSector>();
    }

    public async Task<IReadOnlyList<BoersDataTranslationMetadata>> GetTranslations()
    {
        var payload =
            await _client.GetJson<BoersDataTranslations>($"/v{ApiVersion}/translationmetadata",
                new NameValueCollection { { "authKey", _apiKey } });
        return payload?.TranslationMetadatas ?? new List<BoersDataTranslationMetadata>();
    }

    public async Task<IReadOnlyList<BoersDataInstrument>> GetInstrumentsKpiUpdateTimes()
    {
        var payload =
            await _client.GetJson<BoersDataInstruments>($"/v{ApiVersion}/instruments/updated",
                new NameValueCollection { { "authKey", _apiKey } });
        return payload?.Instruments ?? new List<BoersDataInstrument>();
    }

    /// <summary>
    ///     Normaly all markets is closed and new stockprices is updated around 20:00 (utc+1).
    ///     https://github.com/Borsdata-Sweden/API/wiki/Stockprice
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<BoersDataLatestStockPrice>> GetLatestNordicStockPrices()
    {
        var payload =
            await _client.GetJson<BoersDataLatestStockPrices>($"/v{ApiVersion}/instruments/stockprices/last",
                new NameValueCollection { { "authKey", _apiKey } }, BoersDataJsonSerializerGenerator.Default.BoersDataLatestStockPrices);
        return payload?.StockPricesList ?? new List<BoersDataLatestStockPrice>();
    }

    public async Task<IReadOnlyList<BoersDataLatestStockPrice>> GetLatestGlobalStockPrices()
    {
        var payload =
            await _client.GetJson<BoersDataLatestStockPrices>($"/v{ApiVersion}/instruments/stockprices/global/last",
                new NameValueCollection { { "authKey", _apiKey } },
                BoersDataJsonSerializerGenerator.Default.BoersDataLatestStockPrices);
        return payload?.StockPricesList ?? new List<BoersDataLatestStockPrice>();
    }

    public async Task<IReadOnlyList<BoersDataStockPriceArray>> GetHistoricalStockPrices(HashSet<long> instrumentIds)
    {
        if (!instrumentIds.Any())
            throw new ArgumentException("The list of provided instrument-ids is empty", nameof(instrumentIds));

        var dateFrom = DateTime.UtcNow.AddYears(-3).Date.ToString("yyyy-MM-dd");
        //var dateFrom = "2017-01-01";

        var qryParams = new NameValueCollection { { "authKey", _apiKey } };
        qryParams.Add(new NameValueCollection { { "instList", string.Join(",", instrumentIds) } });
        qryParams.Add(new NameValueCollection { { "from", dateFrom } });

        var payload =
            await _client.GetJson<BoersDataStockPrices>($"/v{ApiVersion}/instruments/stockprices",
                qryParams, BoersDataJsonSerializerGenerator.Default.BoersDataStockPrices);
        return payload?.StockPricesArrayList ?? new List<BoersDataStockPriceArray>();
    }

    public async Task<InstrumentsKpiHistory?> GetR12KpiHistory(int kpiId, List<long> instrumentIds)
    {
        var instruments = string.Join(",", instrumentIds);
        var payload =
            await _client.GetJson<InstrumentsKpiHistory>($"/v{ApiVersion}/instruments/kpis/{kpiId}/r12/mean/history",
                new NameValueCollection
                {
                    { "authKey", _apiKey },
                    { "instList", instruments },
                    { "maxCount", "20" }
                });
        return payload;
    }
    
    public async Task<IReadOnlyList<BoersDataReportMetadata>> GetReportMetadata()
    {
        var payload =
            await _client.GetJson<BoersDataReportMetadatas>($"/v{ApiVersion}/instruments/reports/metadata",
                new NameValueCollection { { "authKey", _apiKey } });
        return payload?.ReportMetadatas ?? new List<BoersDataReportMetadata>();
    }

    public async Task<IReadOnlyList<BoersDataReportList>> GetReports(HashSet<long> instrumentIds)
    {
        if (!instrumentIds.Any())
            throw new ArgumentException("The list of provided instrument-ids is empty", nameof(instrumentIds));

        var qryParams = new NameValueCollection
        {
            { "authKey", _apiKey },
            new NameValueCollection { { "instList", string.Join(",", instrumentIds) } },
            new NameValueCollection { { "maxYearCount", "10" } },
            new NameValueCollection { { "maxR12QCount", "32" } }
        };

        var payload =
            await _client.GetJson<BoersDataReports>($"/v{ApiVersion}/instruments/reports",
                qryParams, BoersDataJsonSerializerGenerator.Default.BoersDataReports); 
        return payload?.ReportList ?? new List<BoersDataReportList>();
    }
}