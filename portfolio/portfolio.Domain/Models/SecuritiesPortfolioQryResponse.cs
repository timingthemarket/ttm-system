using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Models;

public class SecuritiesPortfolioQryResponse
{
    public required Guid PortfolioId { get; set; }
    public required PortfolioDto Portfolio { get; set; }
}