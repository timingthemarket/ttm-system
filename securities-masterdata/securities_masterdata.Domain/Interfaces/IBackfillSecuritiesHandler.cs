namespace securities_masterdata.Domain.Interfaces;

public interface IBackfillSecuritiesHandler
{
    Task HandleBackfillSecurities();
}