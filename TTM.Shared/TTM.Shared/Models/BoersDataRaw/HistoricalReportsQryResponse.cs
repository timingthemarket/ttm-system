using System.Runtime.Serialization;
using TTM.Shared.Models.BoersDataRaw.Reports;

namespace TTM.Shared.Models.BoersDataRaw;

[DataContract]
public class HistoricalReportsQryResponse
{
    [DataMember(Order = 1)]
    public List<HistoricalReportDto> Reports { get; set; }
}