using TTM.Shared.Constants;

namespace TTM.Shared.Models.PortfolioSimulation;

public class PortfolioDto
{
    public Guid Id { get; set; }
    public DateOnly SecuritiesDate { get; set; }
    public DateTime CalculationDate { get; set; }
    public Strategy Strategy { get; set; }
    public List<PortfolioValueDto> PortfolioValues { get; set; }

    public decimal GetInvestedMoney() => PortfolioValues.Sum(pv => pv.Amount * (decimal)pv.Price);
}