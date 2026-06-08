namespace securities_masterdata.Domain.Interfaces;

public interface IBackfillCurrencyRatesHandler
{
    Task HandleBackfillCurrencyRates();
}