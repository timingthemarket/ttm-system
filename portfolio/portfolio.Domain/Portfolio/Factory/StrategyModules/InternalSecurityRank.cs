using portfolio.Domain.Models;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace portfolio.Domain.Portfolio.Factory.StrategyModules;

public record InternalSecurityRank(SecurityDto Security, SecurityPriceDto Price, FunctionSecurityRank Rank, double SecurityCost);