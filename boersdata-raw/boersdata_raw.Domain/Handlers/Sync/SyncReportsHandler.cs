using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.BoersDataApi;
using boersdata_raw.DataAccess.Models.Report;
using boersdata_raw.Domain.Interfaces;
using boersdata_raw.Domain.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using TTM.Shared.Models.BoersDataRaw.Reports;

namespace boersdata_raw.Domain.Handlers.Sync;

public class SyncReportsHandler(
    ILogger<SyncReportsHandler> logger,
    IBoersDataService boersDataService,
    ISecuritiesRepository securitiesRepository,
    IReportRepository reportRepository)
    : ISyncSecuritiesReportsHandler
{
    public async Task<List<ReportDto>> HandleSyncReports()
    {
        var allowedSecurityTypes = new List<SecurityType> { SecurityType.Adr, SecurityType.Stocks };
        var allSecurities = await securitiesRepository.GetAllSecurities();

        var securities = allSecurities
            .Where(s => allowedSecurityTypes.Contains(s.Type))
            .ToList();
        logger.LogInformation("Start syncing reports for {Count} securities", securities.Count);
        foreach (var securitiesChunk in securities.Chunk(50))
        {
            var instrumentIds = securitiesChunk.Select(c => c.InsId).ToHashSet();

            logger.LogInformation("Syncing reports for instruments {Instruments}", instrumentIds);

            List<int> kpiIds = [KpiId.ROC, KpiId.ROIC];
            List<InstrumentsKpiHistory> kpiData = new();
            foreach (var kpiId in kpiIds)
            {
                var kpis = await boersDataService.GetR12KpiHistory(kpiId, instrumentIds.ToList());
                if (kpis != null)
                    kpiData.AddRange(kpis);
            }
            
            var reports = await boersDataService.GetReports(instrumentIds);
            foreach (var report in reports)
            {
                var security = securitiesChunk.FirstOrDefault(s => s.InsId == report.Instrument);
                if (security is null)
                    continue;
                
                // R12
                var r12Reports = report.ReportsR12?.Select(r =>
                    MapToReport(security.Ticker, security.InsId, r, ReportType.TTM, kpiData)).ToList();
                if (r12Reports is not null && r12Reports.Any())
                {
                    // Calculate F-Score for each report
                    await CalculateFScoresForReports(security.Ticker, r12Reports);
                    await reportRepository.SaveHistoricalReports(security.Ticker, r12Reports);
                }
            }
        }
        
        return new List<ReportDto>();
    }
    
    private Report MapToReport(string ticker, long insId, BoersDataReport report, ReportType type,
        List<InstrumentsKpiHistory> kpiData)
    {
        var balanceSheet = new ReportBalanceSheet
        {
            CurrentLiabilities = report.CurrentLiabilities,
            CurrentAssets = report.CurrentAssets,
            FinancialAssets = report.FinancialAssets,
            GrossIncome = report.GrossIncome,
            IntangibleAssets = report.IntangibleAssets,
            NetDebt = report.NetDebt,
            TangibleAssets = report.TangibleAssets,
            TotalAssets = report.TotalAssets,
            TotalEquity = report.TotalEquity,
            CashAndEquivalents = report.CashAndEquivalents,
            NonCurrentAssets = report.NonCurrentAssets,
            NonCurrentLiabilities = report.NonCurrentLiabilities,
            ProfitToEquityHolders = report.ProfitToEquityHolders,
            TotalLiabilitiesAndEquity = report.TotalLiabilitiesAndEquity
        };

        var cashFlow = new ReportCashFlow
        {
            FinancingActivities = report.CashFlowFromFinancingActivities,
            InvestingActivities = report.CashFlowFromInvestingActivities,
            OperatingActivities = report.CashFlowFromOperatingActivities,
            CashFlowForTheYear = report.CashFlowForTheYear,
            FreeCashFlow = report.FreeCashFlow
        };

        var incomeStatement = new ReportIncomeStatement
        {
            Eps = report.EarningsPerShare,
            NetSales = report.NetSales,
            OperatingIncome = report.OperatingIncome,
            GrossProfit = report.ProfitBeforeTax,
            Revenues = report.Revenues
        };
        
        var kpiDataPeriod = kpiData
            .SelectMany(k => k.KpisList.Select(kl => new { k.KpiId, kl.InsId, kl.KpiValues }))
            .Where(k => k.InsId == insId)
            .SelectMany(k => k.KpiValues.Select(kv => new { k.KpiId, KpiValue = kv }))
            .Where(k => k.KpiValue.Year == report.Year && k.KpiValue.Quarter == report.Period)
            .ToList();
        var reportKpis = new ReportKpis
        {
            ROC = kpiDataPeriod.FirstOrDefault(k => k.KpiId == KpiId.ROC)?.KpiValue.Value,
            ROIC = kpiDataPeriod.FirstOrDefault(k => k.KpiId == KpiId.ROIC)?.KpiValue.Value,
        };
        
        return new Report
        {
            Ticker = ticker,
            InsId = insId,
            ReportType = type,
            Year = report.Year,
            Period = report.Period,
            Dividend = report.Dividend,
            CurrencyRatio = report.CurrencyRatio,
            Currency = report.Currency,
            ReportDate = report.ReportDate,
            BrokenFiscalYear = report.BrokenFiscalYear ?? false,
            NumberOfShares = report.NumberOfShares,
            StockPriceAverage = report.StockPriceAverage,
            StockPriceHigh = report.StockPriceHigh,
            StockPriceLow = report.StockPriceLow,
            ReportEndDate = report.ReportEndDate,
            ReportStartDate = report.ReportStartDate,
            BalanceSheet = balanceSheet,
            CashFlow = cashFlow,
            IncomeStatement = incomeStatement,
            ReportKpis = reportKpis
        };
    }

    private async Task CalculateFScoresForReports(string ticker, List<Report> reports)
    {
        // Get existing reports for comparison
        var existingReports = await reportRepository.GetReports(ticker, ReportType.TTM);
        
        foreach (var report in reports)
        {
            // Find previous year report for comparison
            var previousReport = existingReports
                .Where(r => r.Year == report.Year - 1 && r.Period == report.Period)
                .OrderByDescending(r => r.ReportEndDate)
                .FirstOrDefault();

            // Calculate F-Score
            var fScore = FScoreCalculationService.CalculatePiotroskiFScore(report, previousReport);
            report.ReportKpis.FScore = fScore;
        }
    }

    /// <summary>
    /// Börsdata KPI IDs mapping
    /// https://github.com/Borsdata-Sweden/API/wiki/KPI-History
    /// </summary>
    private class KpiId
    {
        public const int ROC = 36;
        public const int ROIC = 37;
    }
}