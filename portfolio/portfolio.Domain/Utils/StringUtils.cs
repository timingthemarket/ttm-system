using portfolio.Domain.Models;

namespace portfolio.Domain.Utils;

public class StringUtils
{
    public static string GetIndicatorsString(List<SimulationFinancialVariable> variables) =>
        string.Join("|", variables.Select(v => v.IndicatorId));
}