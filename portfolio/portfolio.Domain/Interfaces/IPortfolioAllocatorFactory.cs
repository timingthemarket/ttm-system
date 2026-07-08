using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Interfaces;

public interface IPortfolioAllocatorFactory
{
    Task Allocate(List<PortfolioValueDto> portfolioValues, decimal moneyAmount, DateOnly date);
}