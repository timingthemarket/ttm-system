using System.Runtime.Serialization;
using TTM.Shared.Constants;

namespace TTM.Shared.Models.BoersDataRaw.Reports;

[DataContract]
public class ReportDto
{
    [DataMember(Order = 1)]
    public string Ticker { get; set; }

    [DataMember(Order = 2)]
    public Indicators IndicatorId { get; set; }

    [DataMember(Order = 3)]
    public DateOnly Date { get; set; }

    [DataMember(Order = 4)]
    public decimal Value { get; set; }
}