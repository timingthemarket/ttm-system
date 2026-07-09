using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.BoersDataRaw.Prices;


namespace boersdata_raw.Domain.Handlers.Query;

public class QryHistoricalSecuritiesPricesHandler(ILogger<QryHistoricalSecuritiesPricesHandler> logger, IStockPricesRepository stockPricesRepository)
    : IQryHistoricalSecuritiesPricesHandler
{
    public async Task<List<HistoricalPricesDto>> HandleGetHistoricalPrices(List<string> tickers)
    {
        logger.LogInformation("Handling historical prices request {Tickers}", string.Join(",", tickers));
        
        var prices = new List<HistoricalPricesDto>();
        foreach (var ticker in tickers)
        {
            var historicalPrices = await stockPricesRepository.GetHistoricalPrices(ticker);
            var pricesDto = MakeSecurityPriceDtos(historicalPrices);
            var historicalDto = MakeHistoricalPricesDto(ticker, pricesDto);
            prices.Add(historicalDto);
        }

        return prices;
    }

    private HistoricalPricesDto MakeHistoricalPricesDto(string ticker, List<SecurityPriceDto> dtos) => new()
    {
        Ticker = ticker,
        HistoricalPrices = dtos
    };

    private List<SecurityPriceDto> MakeSecurityPriceDtos(List<StockPrice> stockPrices) => stockPrices.Select(s => new
        SecurityPriceDto
        {
            Close = s.Close,
            High = s.High,
            Low = s.Low,
            Open = s.Open,
            Volume = s.Volume,
            Ticker = s.Ticker,
            Date = DateOnly.FromDateTime(s.Date)
        }).ToList();
}