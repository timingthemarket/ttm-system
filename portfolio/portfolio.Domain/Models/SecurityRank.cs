namespace portfolio.Domain.Models;

/// <summary>
/// Lowest rank is considered the best
/// </summary>
/// <param name="SecurityId"></param>
/// <param name="Value"></param>
/// <param name="Rank"></param>
public record SecurityRank(long SecurityId, decimal Value, long Rank);