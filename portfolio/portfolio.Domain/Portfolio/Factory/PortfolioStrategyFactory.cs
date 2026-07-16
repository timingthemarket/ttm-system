using portfolio.Domain.Interfaces;
using TTM.Shared.Constants;

namespace portfolio.Domain.Portfolio.Factory;

public class PortfolioStrategyFactory : IPortfolioStrategyFactory
{
    private readonly IEnumerable<IStrategy> _strategies;

    public PortfolioStrategyFactory(IEnumerable<IStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IStrategy GetStrategy(long strategyId)
    {
        if (strategyId == (long)Strategy.DiLegacy)
            return _strategies.Single(s => s.Strategy == Strategy.DiLegacy);

        string validValues = string.Join(",", Enum.GetValues(typeof(Strategy)));
        throw new Exception($"Invalid strategy id {strategyId}. Allowed ids are {validValues}");
    }
}