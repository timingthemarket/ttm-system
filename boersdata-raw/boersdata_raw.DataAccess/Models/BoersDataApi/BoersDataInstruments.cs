using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public enum InstrumentType
{
    Stocks = 0,
    Pref = 1,
    Index = 2, // No sector or branch -id
    StocksA = 3,
    SectorIndex = 4, // No sector or branch -id
    IndustryIndex = 5, // No sector or branch -id
    Currency = 6,
    Commodity = 7,
    Spac = 8,
    Adr = 9,
    Unit = 10,
    GlobalIndex = 11,
    Cryptocurrencies = 12,
    OtherIndex = 13, // No sector or branch -id
}

public record BoersDataInstrument(
    [property: JsonPropertyName("insId")] long InsId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("urlName")] string UrlName,
    [property: JsonPropertyName("instrument")] InstrumentType Instrument,
    [property: JsonPropertyName("isin")] string Isin,
    [property: JsonPropertyName("ticker")] string Ticker,
    [property: JsonPropertyName("yahoo")] string Yahoo,
    [property: JsonPropertyName("sectorId")] long? SectorId,
    [property: JsonPropertyName("marketId")] long MarketId,
    [property: JsonPropertyName("branchId")] long? BranchId,
    [property: JsonPropertyName("countryId")] long CountryId,
    //[property: JsonPropertyName("listingDate")] DateTime ListingDate,
    [property: JsonPropertyName("stockPriceCurrency")] string StockPriceCurrency,
    [property: JsonPropertyName("reportCurrency")] string ReportCurrency
);

public record BoersDataInstruments(
    [property: JsonPropertyName("instruments")] IReadOnlyList<BoersDataInstrument> Instruments
);