using TTM.Shared.Models.BoersDataRaw.Reports;

namespace securities_masterdata.Domain.Interfaces;

public interface IReportsHandler
{
    Task HandleReports(List<ReportDto> reports);
}