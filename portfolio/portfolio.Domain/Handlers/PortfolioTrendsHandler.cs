using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MathNet.Numerics;
using portfolio.DataAccess;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using portfolio.Domain.Interfaces;

namespace portfolio.Domain.Handlers;

public class PortfolioTrendsHandler(
    ILogger<PortfolioTrendsHandler> logger,
    IPortfolioTrendsRepository portfolioTrendsRepository)
    : IPortfolioTrendsHandler
{
    public async Task ProcessPortfolioTrends()
    {
        logger.LogInformation("Starting portfolio trends processing...");
        
        try
        {
            using var context = new PortfolioDbContext();
            
            var outcomeData = await context.PortfolioOutcomeView
                .AsNoTracking()
                .OrderBy(pov => pov.SetId)
                .ThenBy(pov => pov.SessionDate)
                .ToListAsync();
                
            logger.LogInformation("Retrieved {Count} records from portfolio_outcome_view", outcomeData.Count);

            var groupedData = outcomeData
                .GroupBy(pov => pov.SetId)
                .Where(g => g.Count() > 2)
                .ToList();
                
            logger.LogInformation("Found {GroupCount} set_ids with sufficient data for regression", groupedData.Count);

            int savedTrends = 0;
            var revision = await portfolioTrendsRepository.GetPortfolioTrensSetIdLatestRevision(groupedData.First().Key);
            var newRevision = revision.HasValue ? revision.Value + 1 : 1;
            foreach (var group in groupedData)
            {
                try
                {
                    var setId = group.Key;
                    var orderedData = group.OrderBy(x => x.SessionDate).ToList();
                    
                    var xValues = Enumerable.Range(1, orderedData.Count).Select(x => (double)x).ToArray();
                    var yValues = orderedData.Select(x => x.PercentageChange).ToArray();

                    if (xValues.Length != yValues.Length || xValues.Length < 2)
                    {
                        logger.LogWarning("Insufficient data points for set_id {SetId}: {Count} points", setId, xValues.Length);
                        continue;
                    }

                    var regression = Fit.Line(xValues, yValues);
                    var beta0 = regression.A; // Intercept
                    var beta1 = regression.B; // Slope
                    
                    var portfolioTrends = new PortfolioTrends
                    {
                        Id = Guid.NewGuid(),
                        Revision = newRevision,
                        SetId = setId,
                        Beta0 = beta0,
                        Beta1 = beta1,
                        Beta2 = null,
                        Timestamp = DateTime.UtcNow
                    };
                    
                    await portfolioTrendsRepository.SavePortfolioTrends(portfolioTrends);
                    savedTrends++;
                    if (savedTrends % 1000 == 0)
                    {
                        logger.LogInformation("Calculated trends for {Count}/{Total} set_ids", savedTrends, groupedData.Count);
                    }
                    
                    logger.LogDebug("Calculated trends for set_id {SetId}: Beta0={Beta0:F6}, Beta1={Beta1:F6}", 
                        setId, beta0, beta1);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing regression for set_id {SetId}", group.Key);
                    continue;
                }
            }
            
            logger.LogInformation("Successfully processed and saved portfolio trends");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing portfolio trends");
            throw;
        }
    }
}