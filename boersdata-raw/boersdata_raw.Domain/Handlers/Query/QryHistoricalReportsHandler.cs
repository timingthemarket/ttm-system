using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models.Report;
using boersdata_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using TTM.Shared.Constants;
using TTM.Shared.Models.BoersDataRaw.Reports;

namespace boersdata_raw.Domain.Handlers.Query;

public class QryHistoricalReportsHandler(
    IReportRepository reportRepository)
    : IQryHistoricalReportsHandler
{
    public async Task<List<HistoricalReportDto>> HandleGetReports(List<string> tickers)
    {
        var historicalDtos = new List<HistoricalReportDto>();
        foreach (var ticker in tickers)
        {
            var reports = await reportRepository.GetReports(ticker, ReportType.TTM);
            var historicalReports = new HistoricalReportDto
            {
                Ticker = ticker,
                HistoricalReports = reports
                    .SelectMany(MapReportDto).Where(r => r != null)
                    .Select(s => s!)
                    .ToList()
            };

            historicalDtos.Add(historicalReports);
        }

        return historicalDtos;
    }
    
    /// <summary>
    /// All of thse values are expressed in milions
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    private List<ReportDto?> MapReportDto(Report report) =>
        new()
        {
            MakeReportDto(report, Indicators.Dividend, (decimal)report.Dividend),
            MakeReportDto(report, Indicators.NumberOfShares, (decimal)report.NumberOfShares),
            MakeReportDto(report, Indicators.Eps, (decimal?)report.IncomeStatement.Eps),
            MakeReportDto(report, Indicators.OperatingIncome,
                (decimal?)report.IncomeStatement.OperatingIncome),
            MakeReportDto(report, Indicators.Revenues, (decimal?)report.IncomeStatement.Revenues),
            MakeReportDto(report, Indicators.NetSales, (decimal?)report.IncomeStatement.NetSales),
            MakeReportDto(report, Indicators.GrossProfit, (decimal?)report.IncomeStatement.GrossProfit),

            MakeReportDto(report, Indicators.FreeCashFlow, (decimal?)report.CashFlow.FreeCashFlow),
            MakeReportDto(report, Indicators.CashFlowForTheYear,
                (decimal?)report.CashFlow.CashFlowForTheYear),
            MakeReportDto(report, Indicators.FinancingActivities,
                (decimal?)report.CashFlow.FinancingActivities),
            MakeReportDto(report, Indicators.InvestingActivities,
                (decimal?)report.CashFlow.InvestingActivities),
            MakeReportDto(report, Indicators.OperatingActivities,
                (decimal?)report.CashFlow.OperatingActivities),

            MakeReportDto(report, Indicators.CurrentAssets, (decimal?)report.BalanceSheet.CurrentAssets),
            MakeReportDto(report, Indicators.CurrentLiabilities,
                (decimal?)report.BalanceSheet.CurrentLiabilities),
            MakeReportDto(report, Indicators.FinancialAssets, (decimal?)report.BalanceSheet.FinancialAssets),
            MakeReportDto(report, Indicators.GrossIncome, (decimal?)report.BalanceSheet.GrossIncome),
            MakeReportDto(report, Indicators.TangibleAssets, (decimal?)report.BalanceSheet.TangibleAssets),
            MakeReportDto(report, Indicators.IntangibleAssets,
                (decimal?)report.BalanceSheet.IntangibleAssets),
            MakeReportDto(report, Indicators.NetDebt, (decimal?)report.BalanceSheet.NetDebt),
            MakeReportDto(report, Indicators.TotalAssets, (decimal?)report.BalanceSheet.TotalAssets),
            MakeReportDto(report, Indicators.TotalEquity, (decimal?)report.BalanceSheet.TotalEquity),
            MakeReportDto(report, Indicators.CashAndEquivalents,
                (decimal?)report.BalanceSheet.CashAndEquivalents),
            MakeReportDto(report, Indicators.NonCurrentAssets,
                (decimal?)report.BalanceSheet.NonCurrentAssets),
            MakeReportDto(report, Indicators.NonCurrentLiabilities,
                (decimal?)report.BalanceSheet.NonCurrentLiabilities),
            MakeReportDto(report, Indicators.TotalLiabilitiesAndEquity,
                (decimal?)report.BalanceSheet.TotalLiabilitiesAndEquity),
            MakeReportDto(report, Indicators.ProfitToEquityHolders,
                (decimal?)report.BalanceSheet.ProfitToEquityHolders),
            
            // KPIS
            MakeReportDto(report, Indicators.Roc, (decimal?)report.ReportKpis.ROC),
            MakeReportDto(report, Indicators.Roic, (decimal?)report.ReportKpis.ROIC),
            MakeReportDto(report, Indicators.FScore, (decimal?)report.ReportKpis.FScore)
        };

    private ReportDto? MakeReportDto(Report report, Indicators indicator, decimal? value)
    {
        if (!value.HasValue)
            return null;

        return new ReportDto
        {
            Date = DateOnly.FromDateTime(report.ReportEndDate.Value),
            Ticker = report.Ticker,
            IndicatorId = indicator,
            Value = value.Value
        };
    }
}