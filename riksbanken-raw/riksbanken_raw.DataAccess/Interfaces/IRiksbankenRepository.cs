using riksbanken_raw.DataAccess.Models;

namespace riksbanken_raw.DataAccess.Interfaces;

public interface IRiksbankenRepository
{
    Task<List<ExchangeRateSeries>> GetExchangeRateSeries();
    Task<List<CurrencyRate>> GetCurrenciesFromCode(string code);
    Task<bool> SaveCurrency(CurrencyRate cur);
    Task<int> SaveHistoricalCurrencies(List<CurrencyRate> curriencies);
    Task<bool> UpdateLatestFetchedDate(string seriesId, DateTime lastestDate);
}