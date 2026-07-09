using TTM.Shared.Models.BoersDataRaw.Reports;

namespace boersdata_raw.Domain.Interfaces;

public interface IQryHistoricalReportsHandler
{
    Task<List<HistoricalReportDto>> HandleGetReports(List<string> tickers);
}