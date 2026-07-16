using riksbanken_raw.DataAccess.Interfaces;
using riksbanken_raw.Domain.Interfaces;

namespace riksbanken_raw.Domain.Handlers.Sync;

public class SyncHistoricalCurrencyHandler : IHistoricalCurrencySyncHandler
{
    private readonly IRiksbankenRepository _riksbankenRepository;
    private readonly IRiksbankenService _riksbankenService;

    public SyncHistoricalCurrencyHandler(IRiksbankenRepository riksbankenRepository,
        IRiksbankenService riksbankenService)
    {
        _riksbankenRepository = riksbankenRepository;
        _riksbankenService = riksbankenService;
    }

    public async Task HandleHistoricalCurrencyExchangeSync()
    {
        var exchangeRateSeries = await _riksbankenRepository.GetExchangeRateSeries();
        foreach (var serie in exchangeRateSeries)
        {
            var historicalObservations = await _riksbankenService.GetHistoricalObservations(serie.SeriesId);

            var currencies = historicalObservations.Select(h => SyncCurrencyHandler.MakeCurrency(h, serie)).ToList();
            var nrErrors = await _riksbankenRepository.SaveHistoricalCurrencies(currencies);
        }
    }
}