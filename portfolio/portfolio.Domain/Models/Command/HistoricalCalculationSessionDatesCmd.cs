using System.Text.Json.Serialization;
using portfolio.Domain.Services;

namespace portfolio.Domain.Models.Command;

public class HistoricalCalculationSessionDatesCmd
{
    public List<DateOnly> Dates { get; set; }
    public int NrIterations { get; set; }
    public IndicatorSearchSpace ProcessDirection { get; set; } = IndicatorSearchSpace.Start;

}