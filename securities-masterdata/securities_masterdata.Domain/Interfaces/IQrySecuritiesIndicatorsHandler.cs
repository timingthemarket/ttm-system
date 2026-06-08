using TTM.Shared.Constants;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Interfaces;

public interface IQrySecuritiesIndicatorsHandler
{
    Task<List<SecurityIndicatorDto>> HandleGetIndicators(DateOnly date, List<SecuritiesIndicatorQryMetadataDto> indicators);
}