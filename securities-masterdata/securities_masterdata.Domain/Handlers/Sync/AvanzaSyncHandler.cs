using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.DataAccess.Services.Models;
using securities_masterdata.Domain.Interfaces;
using securities_masterdata.Domain.Models.Sync;

namespace securities_masterdata.Domain.Handlers.Sync;

public class AvanzaSyncHandler : IAvanzaSyncHandler
{
    private readonly IAvanzaService _avanzaService;
    private readonly INordnetService _nordnetService;
    private readonly ISecurityRepository _securityRepository;

    public AvanzaSyncHandler(
        IAvanzaService avanzaService,
        INordnetService nordnetService,
        ISecurityRepository securityRepository)
    {
        _avanzaService = avanzaService;
        _nordnetService = nordnetService;
        _securityRepository = securityRepository;
    }

    public async Task<AvanzaSyncResult> HandleSyncSecuritiesWithAvanza(CancellationToken cancellationToken = default)
    {
        var result = new AvanzaSyncResult();

        try
        {
            // Get all stocks from both platforms with pagination
            var avanzaStocks = await GetAllAvanzaStocks(cancellationToken);
            result.TotalAvanzaStocks = avanzaStocks.Count;

            var nordnetStocks = await GetAllNordnetStocks(cancellationToken);
            result.TotalNordnetStocks = nordnetStocks.Count;

            var avanzaMatcher = new PlatformStockMatcher(
                avanzaStocks.Select(stock => ((string?)stock.Ticker, (string?)stock.Name)));
            var nordnetMatcher = new PlatformStockMatcher(
                nordnetStocks.Select(stock => ((string?)stock.Ticker, (string?)stock.Name)));

            // Get all securities from database including inactive ones, so a security that
            // reappears on a platform can be activated again
            var dbSecurities = await _securityRepository.GetAll(includeInactive: true);
            result.TotalSecuritiesInDatabase = dbSecurities.Count;

            var tradePlatformBySecurityId = new Dictionary<long, string?>(dbSecurities.Count);

            for (var i = 0; i < dbSecurities.Count; i++)
            {
                var security = dbSecurities[i];

                var onAvanza = avanzaMatcher.Matches(security.Ticker, security.Name);
                var onNordnet = nordnetMatcher.Matches(security.Ticker, security.Name);

                tradePlatformBySecurityId[security.SecurityId] = BuildTradePlatform(onAvanza, onNordnet);

                if (onAvanza && onNordnet)
                    result.SecuritiesOnBothPlatforms++;
                else if (onAvanza)
                    result.SecuritiesOnAvanzaOnly++;
                else if (onNordnet)
                    result.SecuritiesOnNordnetOnly++;

                if (!onAvanza && !onNordnet && !security.Inactive)
                    result.SecuritiesMarkedInactive++;
                else if ((onAvanza || onNordnet) && security.Inactive)
                    result.SecuritiesMarkedActive++;

                if ((i + 1) % 500 == 0)
                    Console.WriteLine($"{i + 1}/{dbSecurities.Count}");
            }

            await _securityRepository.UpdateTradePlatforms(tradePlatformBySecurityId);

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private static string? BuildTradePlatform(bool onAvanza, bool onNordnet)
    {
        return (onAvanza, onNordnet) switch
        {
            (true, true) => TradePlatforms.Avanza + TradePlatforms.Separator + TradePlatforms.Nordnet,
            (true, false) => TradePlatforms.Avanza,
            (false, true) => TradePlatforms.Nordnet,
            // On neither platform, which is what an inactive security looks like
            _ => null
        };
    }

    private async Task<List<AvanzaStock>> GetAllAvanzaStocks(CancellationToken cancellationToken)
    {
        var allStocks = new List<AvanzaStock>();
        const int pageSize = 1000;
        var offset = 0;

        while (true)
        {
            var response = await _avanzaService.GetStocksAsync(
                offset: offset,
                limit: pageSize,
                cancellationToken: cancellationToken);

            if (response?.Stocks == null || !response.Stocks.Any())
                break;

            allStocks.AddRange(response.Stocks);

            // If we got fewer results than the page size, we've reached the end
            if (response.Stocks.Length < pageSize)
                break;

            offset += pageSize;
        }

        return allStocks;
    }

    private async Task<List<NordnetInstrumentInfo>> GetAllNordnetStocks(CancellationToken cancellationToken)
    {
        var allStocks = new List<NordnetInstrumentInfo>();
        const int pageSize = 1000;
        var offset = 0;

        while (true)
        {
            var response = await _nordnetService.GetStocksAsync(
                offset: offset,
                limit: pageSize,
                cancellationToken: cancellationToken);

            if (response?.Results == null || response.Results.Length == 0)
                break;

            allStocks.AddRange(response.Results.Select(instrument => instrument.InstrumentInfo));

            // If we got fewer results than the page size, we've reached the end
            if (response.Results.Length < pageSize)
                break;

            // Safety net so a full page on every request can never loop forever
            if (response.TotalHits > 0 && allStocks.Count >= response.TotalHits)
                break;

            offset += pageSize;
        }

        return allStocks;
    }
}
