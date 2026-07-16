namespace portfolio.Domain.Interfaces;

public interface IPortfolioStrategyFactory
{
    IStrategy GetStrategy(long strategyId);
}