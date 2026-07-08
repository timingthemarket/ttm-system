using TTM.Shared.Constants;
using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace portfolio.DataAccess.Interfaces;

public interface IMasterdataService
{
    Task<SecuritiesPricesQryResponse> GetLatestPrices(DateOnly date, HashSet<long>? securityIds, CancellationToken cancellationToken = default);
    Task<SecuritiesIndicatorsQryResponse> GetIndicators(DateOnly date, List<SecuritiesIndicatorQryMetadataDto> indicators);
    Task<SecuritiesQryResponse> GetSecurites(List<string>? tickers, List<long>? securityIds, bool convertToOriginalPrice = false);
}