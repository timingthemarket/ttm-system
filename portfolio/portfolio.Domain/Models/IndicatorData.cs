using TTM.Shared.Constants;

namespace portfolio.Domain.Models;

public record struct IndicatorData(long SecurityId, decimal Value, Indicators IndicatorId, decimal? RankFriendlyValue);