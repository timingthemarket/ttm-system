using securities_masterdata.DataAccess.Entities;
using TTM.Shared.Constants;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Interfaces;

public interface IIndicatorsCalculationFactory
{
    Task<List<SecurityIndicatorDto>> Compute(SecuritiesIndicatorQryMetadataDto indicator, List<Security> securities, DateOnly date);
}