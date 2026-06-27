using boersdata_raw.DataAccess.Models.Report;

namespace boersdata_raw.DataAccess.Interfaces;

public interface IReportRepository
{
    public Task SaveReportTypes(List<ReportTypes> types, CancellationToken token = default);
    public Task SaveHistoricalReports(string ticker, List<Report> reports, CancellationToken token = default);
    public Task<List<Report>> GetReports(string ticker, ReportType type, CancellationToken token = default);
}