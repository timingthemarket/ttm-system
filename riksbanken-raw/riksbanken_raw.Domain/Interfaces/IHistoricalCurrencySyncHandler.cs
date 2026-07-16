namespace riksbanken_raw.Domain.Interfaces;

public interface IHistoricalCurrencySyncHandler
{
    Task HandleHistoricalCurrencyExchangeSync();
}