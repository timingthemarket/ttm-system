using TTM.Shared.Models.BoersDataRaw.Reports;

namespace boersdata_raw.Domain.Interfaces;

public interface ISyncSecuritiesReportsHandler
{
    Task<List<ReportDto>> HandleSyncReports();
}