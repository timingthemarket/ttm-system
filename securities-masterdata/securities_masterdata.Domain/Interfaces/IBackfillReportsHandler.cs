namespace securities_masterdata.Domain.Interfaces;

public interface IBackfillReportsHandler
{
    Task HandleBackfillReports();
    Task HandleBackfillReports(List<string> tickers);
}