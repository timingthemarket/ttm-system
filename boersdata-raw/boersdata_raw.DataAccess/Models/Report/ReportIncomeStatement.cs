namespace boersdata_raw.DataAccess.Models.Report;

public record ReportIncomeStatement
{
    public double? Eps { get; set; }
    public double? OperatingIncome { get; set; }
    public double? Revenues { get; set; }
    public double? GrossProfit { get; set; }
    public double? NetSales { get; set; }
}
