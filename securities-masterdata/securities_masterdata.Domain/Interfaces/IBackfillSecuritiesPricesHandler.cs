namespace securities_masterdata.Domain.Interfaces;

public interface IBackfillSecuritiesPricesHandler
{
    Task HandleBackfillSecuritiesPrices();
    Task HandleBackfillSecurityPrices(List<string> tickers);
}