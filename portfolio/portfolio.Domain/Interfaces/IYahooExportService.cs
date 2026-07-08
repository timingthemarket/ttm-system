namespace portfolio.Domain.Interfaces;

public interface IYahooExportService
{
    Task<Stream> ExportYahooDataToFile(decimal money, Guid simulationId);
    Task<Stream> ExportYahooDataToFileBySetId(decimal money, string setId);
}