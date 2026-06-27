using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataReport(
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("period")] int Period,
    [property: JsonPropertyName("revenues")] double? Revenues,
    [property: JsonPropertyName("gross_Income")] double? GrossIncome,
    [property: JsonPropertyName("operating_Income")] double? OperatingIncome,
    [property: JsonPropertyName("profit_Before_Tax")] double? ProfitBeforeTax,
    [property: JsonPropertyName("profit_To_Equity_Holders")] double? ProfitToEquityHolders,
    [property: JsonPropertyName("earnings_Per_Share")] double? EarningsPerShare,
    [property: JsonPropertyName("number_Of_Shares")] double NumberOfShares,
    [property: JsonPropertyName("dividend")] double Dividend,
    [property: JsonPropertyName("intangible_Assets")] double? IntangibleAssets,
    [property: JsonPropertyName("tangible_Assets")] double? TangibleAssets,
    [property: JsonPropertyName("financial_Assets")] double? FinancialAssets,
    [property: JsonPropertyName("non_Current_Assets")] double? NonCurrentAssets,
    [property: JsonPropertyName("cash_And_Equivalents")] double? CashAndEquivalents,
    [property: JsonPropertyName("current_Assets")] double? CurrentAssets,
    [property: JsonPropertyName("total_Assets")] double? TotalAssets,
    [property: JsonPropertyName("total_Equity")] double? TotalEquity,
    [property: JsonPropertyName("non_Current_Liabilities")] double? NonCurrentLiabilities,
    [property: JsonPropertyName("current_Liabilities")] double? CurrentLiabilities,
    [property: JsonPropertyName("total_Liabilities_And_Equity")] double? TotalLiabilitiesAndEquity,
    [property: JsonPropertyName("net_Debt")] double? NetDebt,
    [property: JsonPropertyName("cash_Flow_From_Operating_Activities")] double? CashFlowFromOperatingActivities,
    [property: JsonPropertyName("cash_Flow_From_Investing_Activities")] double? CashFlowFromInvestingActivities,
    [property: JsonPropertyName("cash_Flow_From_Financing_Activities")] double? CashFlowFromFinancingActivities,
    [property: JsonPropertyName("cash_Flow_For_The_Year")] double? CashFlowForTheYear,
    [property: JsonPropertyName("free_Cash_Flow")] double? FreeCashFlow,
    [property: JsonPropertyName("stock_Price_Average")] double StockPriceAverage,
    [property: JsonPropertyName("stock_Price_High")] double StockPriceHigh,
    [property: JsonPropertyName("stock_Price_Low")] double StockPriceLow,
    [property: JsonPropertyName("report_Start_Date")] DateTime? ReportStartDate,
    [property: JsonPropertyName("report_End_Date")] DateTime? ReportEndDate,
    [property: JsonPropertyName("broken_Fiscal_Year")] bool? BrokenFiscalYear,
    [property: JsonPropertyName("currency")] string? Currency,
    [property: JsonPropertyName("currency_Ratio")] double CurrencyRatio,
    [property: JsonPropertyName("net_Sales")] double? NetSales,
    [property: JsonPropertyName("report_Date")] DateTime? ReportDate
);

public record BoersDataReportList(
    [property: JsonPropertyName("instrument")] long Instrument,
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("reportsYear")] IReadOnlyList<BoersDataReport>? ReportsYear,
    [property: JsonPropertyName("reportsQuarter")] IReadOnlyList<BoersDataReport>? ReportsQuarter,
    [property: JsonPropertyName("reportsR12")] IReadOnlyList<BoersDataReport>? ReportsR12
);

public record BoersDataReports(
    [property: JsonPropertyName("reportList")] IReadOnlyList<BoersDataReportList> ReportList
);


