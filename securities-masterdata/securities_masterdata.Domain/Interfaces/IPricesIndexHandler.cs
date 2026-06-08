namespace securities_masterdata.Domain.Interfaces;

public interface IPricesIndexHandler
{
    Task HandleDailyPricesIndex();
    Task HandleRecalculateIndexValues(long indexId);
}