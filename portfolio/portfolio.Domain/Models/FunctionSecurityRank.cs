namespace portfolio.Domain.Models;

public record FunctionSecurityRank(long SecurityId, decimal Value, long Rank, int FunctionConvertedRank);