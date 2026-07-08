using portfolio.Domain.Models;
using TTM.Shared.Constants;
using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Interfaces;

public interface IStrategy
{
    public Strategy Strategy { get; }
    Task<DataAccess.Models.Db.Portfolio> Compute(StrategyInput input);
}