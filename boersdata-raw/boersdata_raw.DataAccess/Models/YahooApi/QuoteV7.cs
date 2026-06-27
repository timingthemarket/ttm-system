using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.YahooApi;

public record QuoteV7Response(
    [property: JsonPropertyName("result")] IReadOnlyList<QuoteV7Result> result,
    [property: JsonPropertyName("error")] object? error
);

public record QuoteV7Result(
    [property: JsonPropertyName("currency")]
    string Currency,
    [property: JsonPropertyName("exchange")]
    string Exchange,
    [property: JsonPropertyName("messageBoardId")]
    string MessageBoardId,
    [property: JsonPropertyName("exchangeTimezoneName")]
    string ExchangeTimezoneName,
    [property: JsonPropertyName("exchangeTimezoneShortName")]
    string ExchangeTimezoneShortName,
    [property: JsonPropertyName("gmtOffSetMilliseconds")]
    int GmtOffSetMilliseconds,
    [property: JsonPropertyName("esgPopulated")]
    bool EsgPopulated,
    [property: JsonPropertyName("fullExchangeName")]
    string FullExchangeName,
    [property: JsonPropertyName("financialCurrency")]
    string FinancialCurrency,
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("dividendDate")]
    long? DividendDate,
    [property: JsonPropertyName("earningsTimestamp")]
    long? EarningsTimestamp,
    [property: JsonPropertyName("earningsTimestampStart")]
    long? EarningsTimestampStart,
    [property: JsonPropertyName("earningsTimestampEnd")]
    long? EarningsTimestampEnd
);

public record QuoteV7(
    [property: JsonPropertyName("quoteResponse")]
    QuoteV7Response quoteResponse
);