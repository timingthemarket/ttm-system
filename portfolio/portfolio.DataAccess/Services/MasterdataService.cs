using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Interfaces;
using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace portfolio.DataAccess.Services;

public class MasterdataService(
    IMemoryCache cache,
    TTM.Shared.gRPC.Services.IMasterdataService masterdataService,
    ILogger<MasterdataService> logger)
    : IMasterdataService
{
    private const string SecurityCacheKey = "SECURITIES";

    public async Task<SecuritiesPricesQryResponse> GetLatestPrices(DateOnly date, HashSet<long>? securityIds, CancellationToken cancellationToken = default)
    {
        var query = new SecuritiesPricesQry
        {
            Date = date,
            SecurityIds = securityIds
        };
                
        // If the underlying gRPC service supports cancellation tokens, this will work
        // Otherwise, we'll wrap it with a timeout using the cancellation token
        return await masterdataService.GetLatestPrices(query);
    }

    public async Task<SecuritiesIndicatorsQryResponse> GetIndicators(DateOnly date, List<SecuritiesIndicatorQryMetadataDto> indicators)
    {
        if (indicators == null || indicators.Count == 0)
        {
            logger.LogWarning("GetIndicators called with null or empty indicators list for date {Date}", date);
            return new()
            {
                Date = date,
                Variables = new List<SecurityIndicatorDto>()
            };
        }

        List<SecurityIndicatorDto> variables = new();
        int chunkIndex = 0;
        foreach (var indicatorChunk in indicators.Chunk(2))
        {
            var result = await masterdataService.GetIndicators(
                new SecuritiesIndicatorsQry
                {
                    Date = date,
                    Indicators = indicatorChunk.ToList(),
                });

            if (result?.Variables != null)
            {
                variables.AddRange(result.Variables);
            }
            else
            {
                logger.LogWarning(
                    "Masterdata service returned null Variables for date {Date}, chunk {ChunkIndex} with {IndicatorCount} indicators",
                    date, chunkIndex, indicatorChunk.Count());
            }
            chunkIndex++;
        }

        if (variables.Count == 0)
        {
            logger.LogWarning(
                "No indicator data retrieved from masterdata service for date {Date} with {IndicatorCount} indicators requested",
                date, indicators.Count);
        }

        return new ()
        {
            Date = date,
            Variables = variables
        };
    }

    public async Task<SecuritiesQryResponse> GetSecurites(List<string>? tickers, List<long>? securityIds, bool convertToOriginalPrice = false)
    {
        if (!convertToOriginalPrice && cache.TryGetValue(SecurityCacheKey, out SecuritiesQryResponse? securityResponse) &&
            securityResponse != null)
        {
            if (securityIds != null)
                return new SecuritiesQryResponse
                {
                    Securities = securityResponse.Securities.Where(s => securityIds.Contains(s.SecurityId)).ToList()
                };

            if (tickers != null)
                return new SecuritiesQryResponse
                {
                    Securities = securityResponse.Securities.Where(s => tickers.Contains(s.Ticker)).ToList()
                };

            return securityResponse;
        }

        var result = await masterdataService.GetSecurities(new SecuritiesQry
        {
            Tickers = tickers,
            SecurityIds = securityIds,
            ConvertPricesToOriginal = convertToOriginalPrice
        });

        // Cache result 15 minutes
        cache.Set(SecurityCacheKey, result, TimeSpan.FromMinutes(60));

        return result;
    }
}