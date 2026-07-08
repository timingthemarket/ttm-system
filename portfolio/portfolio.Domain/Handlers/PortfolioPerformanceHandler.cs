using Microsoft.EntityFrameworkCore;
using portfolio.DataAccess;
using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Extensions;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;

namespace portfolio.Domain.Handlers;

public class PortfolioPerformanceHandler(
    IPortfolioRepository portfolioRepository,
    IMasterdataService masterdataService) : IPortfolioPerformanceHandler
{
    public async Task<PortfolioPerformanceResponse?> GetPerformanceBySetId(string setId, DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        using var context = new PortfolioDbContext();

        // Get portfolio ID and original session date from the outcome view
        var outcome = await context.PortfolioOutcomeView
            .AsNoTracking()
            .Where(pov => pov.SetId == setId)
            .Select(pov => new { pov.PortfolioId, pov.SessionDate })
            .FirstOrDefaultAsync();

        if (outcome == null)
        {
            return null;
        }

        // Load the portfolio with its securities
        var portfolio = await portfolioRepository.GetPortfolioById(outcome.PortfolioId);
        if (portfolio == null || portfolio.PortfolioValues == null || portfolio.PortfolioValues.Count == 0)
        {
            return null;
        }

        // Calculate original portfolio value from stored prices
        var originalValue = portfolio.PortfolioValues.Sum(pv => pv.Price * pv.Amount);

        // Fetch current prices for the securities at the target date
        var securityIds = portfolio.PortfolioValues.Select(pv => pv.SecurityId).ToHashSet();
        var pricesResponse = await masterdataService.GetLatestPrices(targetDate, securityIds);

        if (pricesResponse.SecurityPrices == null || pricesResponse.SecurityPrices.Count == 0)
        {
            return null;
        }

        // Calculate current portfolio value with new prices
        var currentValue = portfolio.PortfolioValues
            .Join(pricesResponse.SecurityPrices,
                pv => pv.SecurityId,
                sp => sp.SecurityId,
                (pv, sp) => sp.MedianPrice() * pv.Amount)
            .Sum();

        // Calculate percentage change
        var percentageChange = (currentValue - originalValue) / originalValue;

        return new PortfolioPerformanceResponse
        {
            SetId = setId,
            OriginalDate = outcome.SessionDate,
            TargetDate = targetDate,
            OriginalValue = Math.Round(originalValue, 2),
            CurrentValue = Math.Round(currentValue, 2),
            PercentageChange = Math.Round(percentageChange, 4)
        };
    }
}
