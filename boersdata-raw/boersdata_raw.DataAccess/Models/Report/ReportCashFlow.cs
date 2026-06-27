namespace boersdata_raw.DataAccess.Models.Report;

public record ReportCashFlow
{
    public double? FreeCashFlow { get; set; }
    public double? OperatingActivities { get; set; }
    public double? InvestingActivities { get; set; }
    public double? FinancingActivities { get; set; }
    public double? CashFlowForTheYear { get; set; }
}