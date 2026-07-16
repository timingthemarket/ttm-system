using System;
using portfolio.Domain.Models.Command;
using portfolio.Domain.Services;

namespace portfolio.Domain.Models;

public record HistoricalExplorerCalculationRequest(
    DateOnly SessionDate,
    int NrIterations,
    IndicatorSearchSpace ProcessDirection)
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}