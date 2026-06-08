using System.Runtime.Serialization;

namespace TTM.Shared.Models.BoersDataRaw.Reports;

[DataContract]
public class HistoricalReportDto
{
    [DataMember(Order = 1)]
    public string Ticker { get; set; }

    [DataMember(Order = 2)]
    public List<ReportDto> HistoricalReports { get; set; }
}