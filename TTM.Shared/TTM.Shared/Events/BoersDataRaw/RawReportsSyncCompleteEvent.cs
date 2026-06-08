using TTM.Shared.Models.BoersDataRaw.Reports;

namespace TTM.Shared.Events.BoersDataRaw;

public class RawReportsSyncCompleteEvent
{
    public List<ReportDto> Reports { get; set; }
}