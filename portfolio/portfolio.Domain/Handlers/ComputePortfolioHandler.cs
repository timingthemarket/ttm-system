using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;

namespace portfolio.Domain.Handlers;

public class ComputePortfolioHandler(IPortfolio portfolio)
    : IComputePortfolioHandler
{
    public async Task<SecuritiesPortfolioQryResponse> HandleComputePortfolio(DateOnly date, long strategyId,
        double rowSimilarity, List<PortfolioInputIndicatorVariable> variables, decimal money, double maxSecuritySpending)
    {
        var input = new PortfolioInput
        {
            Date = date,
            Indicators = variables,
            RowSimilarity = rowSimilarity,
            StrategyId = strategyId,
            Money = money,
            MaxSecuritySpending = maxSecuritySpending
        };
        
        var portfolio1 = await portfolio.Compute(input);

        return new SecuritiesPortfolioQryResponse
        {
            Portfolio = portfolio1,
            PortfolioId = portfolio1.Id
        };
    }
}