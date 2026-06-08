using securities_masterdata.DataAccess.Entities;

namespace securities_masterdata.DataAccess.Interfaces;

public interface ICurrencyRepository
{
    Task<List<CurrencyRate>> GetLatestCurrencyRatesByDate(DateOnly date);
    Task<List<Currency>> GetAllCurrencies();
    Task SaveRate(CurrencyRate rate);
    Task<Currency> SaveCurrency(Currency currency);
    Task WriteManyRates(List<CurrencyRate> ratesHistories);
    Task RemoveManyRates(long currencyIdFrom);
    Task<Currency?> GetSingleCurrency(string currencyCode);
    Task<List<CurrencyRate>> GetAllCurrencyRates();
}