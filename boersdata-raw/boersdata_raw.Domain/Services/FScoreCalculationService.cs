using boersdata_raw.DataAccess.Models.Report;
using boersdata_raw.Domain.Interfaces;

namespace boersdata_raw.Domain.Services;

public class FScoreCalculationService
{
    public static double CalculatePiotroskiFScore(Report currentReport, Report? previousReport)
    {
        var score = 0;

        // Profitability Criteria (4 points)
        score += IsProfitable(currentReport) ? 1 : 0;
        score += HasPositiveROA(currentReport, previousReport) ? 1 : 0;
        score += HasPositiveOperatingCashFlow(currentReport) ? 1 : 0;
        score += IsOperatingCashFlowGreaterThanNetIncome(currentReport) ? 1 : 0;

        // Leverage/Liquidity Criteria (3 points)
        score += HasDecreasingLongTermDebtRatio(currentReport, previousReport) ? 1 : 0;
        score += HasIncreasingCurrentRatio(currentReport, previousReport) ? 1 : 0;
        score += HasNoDilution(currentReport, previousReport) ? 1 : 0;

        // Operating Efficiency Criteria (2 points)
        score += HasIncreasingGrossProfitMargin(currentReport, previousReport) ? 1 : 0;
        score += HasIncreasingAssetTurnover(currentReport, previousReport) ? 1 : 0;

        return score;
    }

    private static bool IsProfitable(Report report)
    {
        return report.BalanceSheet.ProfitToEquityHolders > 0;
    }

    private static bool HasPositiveROA(Report report, Report? previousReport)
    {
        if (previousReport == null)
            return false;
        
        if (report.BalanceSheet.TotalAssets <= 0 || previousReport.BalanceSheet.TotalAssets <= 0)
            return false;
        
        var roaPrevious = previousReport.BalanceSheet.ProfitToEquityHolders / previousReport.BalanceSheet.TotalAssets;
        var roa = report.BalanceSheet.ProfitToEquityHolders / report.BalanceSheet.TotalAssets;
        return roa > roaPrevious;
    }

    private static bool HasPositiveOperatingCashFlow(Report report)
    {
        return report.CashFlow.OperatingActivities > 0;
    }

    private static bool IsOperatingCashFlowGreaterThanNetIncome(Report report)
    {
        return report.CashFlow.OperatingActivities > report.BalanceSheet.ProfitToEquityHolders;
    }

    private static bool HasDecreasingLongTermDebtRatio(Report currentReport, Report? previousReport)
    {
        if (previousReport == null)
            return false;
        
        if (currentReport.BalanceSheet.TotalAssets <= 0 || previousReport.BalanceSheet.TotalAssets <= 0)
            return false;

        var currentLongTermDebtRatio = currentReport.BalanceSheet.NonCurrentLiabilities / currentReport.BalanceSheet.TotalAssets;
        var previousLongTermDebtRatio = previousReport.BalanceSheet.NonCurrentLiabilities / previousReport.BalanceSheet.TotalAssets;
        
        return currentLongTermDebtRatio < previousLongTermDebtRatio;
    }

    private static bool HasIncreasingCurrentRatio(Report currentReport, Report? previousReport)
    {
        if (previousReport == null)
            return false;
        
        if (currentReport.BalanceSheet.CurrentLiabilities <= 0 || previousReport.BalanceSheet.CurrentLiabilities <= 0)
            return false;

        var currentRatio = currentReport.BalanceSheet.CurrentAssets / currentReport.BalanceSheet.CurrentLiabilities;
        var previousRatio = previousReport.BalanceSheet.CurrentAssets / previousReport.BalanceSheet.CurrentLiabilities;
        
        return currentRatio > previousRatio;
    }

    private static bool HasNoDilution(Report currentReport, Report? previousReport)
    {
        if (previousReport == null)
            return false;
        return currentReport.NumberOfShares <= previousReport.NumberOfShares;
    }

    private static bool HasIncreasingGrossProfitMargin(Report currentReport, Report? previousReport)
    {
        if (previousReport == null)
            return false;
        if (currentReport.IncomeStatement.Revenues <= 0 || previousReport.IncomeStatement.Revenues <= 0)
            return false;

        var currentMargin = currentReport.IncomeStatement.GrossProfit / currentReport.IncomeStatement.Revenues;
        var previousMargin = previousReport.IncomeStatement.GrossProfit / previousReport.IncomeStatement.Revenues;
        
        return currentMargin > previousMargin;
    }

    private static bool HasIncreasingAssetTurnover(Report currentReport, Report? previousReport)
    {
        if (previousReport == null)
            return false;
        if (currentReport.BalanceSheet.TotalAssets <= 0 || previousReport.BalanceSheet.TotalAssets <= 0)
            return false;

        var currentTurnover = currentReport.IncomeStatement.Revenues / currentReport.BalanceSheet.TotalAssets;
        var previousTurnover = previousReport.IncomeStatement.Revenues / previousReport.BalanceSheet.TotalAssets;
        
        return currentTurnover > previousTurnover;
    }
}