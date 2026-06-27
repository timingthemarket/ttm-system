namespace boersdata_raw.DataAccess.Models.Report;

public record ReportBalanceSheet
{
    public double? GrossIncome { get; set; }
    public double? NetDebt { get; set; }
    public double? IntangibleAssets { get; set; }
    public double? TangibleAssets { get; set; }
    public double? CurrentAssets { get; set; }
    public double? NonCurrentAssets { get; set; }
    public double? TotalAssets { get; set; }
    public double? ProfitToEquityHolders { get; set; }
    public double? NonCurrentLiabilities { get; set; }
    public double? CurrentLiabilities { get; set; }
    public double? TotalLiabilitiesAndEquity { get; set; }
    public double? CashAndEquivalents { get; set; }
    public double? TotalEquity { get; set; }
    public double? FinancialAssets { get; set; }
}