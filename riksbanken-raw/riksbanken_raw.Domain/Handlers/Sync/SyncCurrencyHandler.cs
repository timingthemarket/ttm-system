using System.Net;
using Microsoft.Extensions.Logging;
using riksbanken_raw.DataAccess.Interfaces;
using riksbanken_raw.DataAccess.Models;
using riksbanken_raw.Domain.Cache;
using riksbanken_raw.Domain.Interfaces;
using ttm_system.Shared.Constants;
using ttm_system.Shared.Models.RiksbankenRaw;

namespace riksbanken_raw.Domain.Handlers.Sync;

public class SyncCurrencyHandler : ICurrencySyncHandler
{
    private readonly ILogger<SyncCurrencyHandler> _logger;
    private readonly IRiksbankenRepository _riksbankenRepository;
    private readonly IRiksbankenService _riksbankenService;

    public SyncCurrencyHandler(ILogger<SyncCurrencyHandler> logger, IRiksbankenRepository riksbankenRepository, IRiksbankenService riksbankenService)
    {
        _logger = logger;
        _riksbankenRepository = riksbankenRepository;
        _riksbankenService = riksbankenService;
    }
    
    public async Task<List<CurrencyRateDto>> HandleLatestCurrencyExchangeSync()
    {
        var exchangeRateSeries = await _riksbankenRepository.GetExchangeRateSeries();

        bool error = false;
        var currencyRates = new List<CurrencyRateDto>();
        foreach (var serie in exchangeRateSeries)
        {
            RiksbankenObservation? latestObservation = null;
            
            try
            {
                latestObservation = await _riksbankenService.GetLatestObservation(serie.SeriesId);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode != HttpStatusCode.ServiceUnavailable)
                    throw;
                
                Errors.NrServiceNotAvaliableErrors++;
                if (Errors.NrServiceNotAvaliableErrors % 3 == 0)
                    throw;

                _logger.LogError(e, e.Message);
                error = true;
            }

            // if there is a null
            if (error)
                break;
            
            if (latestObservation == null || !DateTime.TryParse(latestObservation.Date, out var latestObservationDate))
                continue;
            if (latestObservationDate.Date <= serie.LastFetched?.Date)
                continue;

            _logger.LogInformation("New date {Date} was found from Riksbanken", latestObservationDate.Date);
            
            var currency = MakeCurrency(latestObservation, serie);
            var saved = await _riksbankenRepository.SaveCurrency(currency);
            if (saved)
                await _riksbankenRepository.UpdateLatestFetchedDate(serie.SeriesId, latestObservationDate);

            currencyRates.Add(MakeCurrencyRateDto(currency));
        }

        return currencyRates;  
    }

    private CurrencyRateDto MakeCurrencyRateDto(CurrencyRate currency) =>
        new()
        {
            Date = DateTime.Parse(currency.Date),
            FromCode = currency.FromCode,
            ToCode = currency.ToCode,
            Rate = currency.Rate
        };

    public static CurrencyRate MakeCurrency(RiksbankenObservation observation, ExchangeRateSeries serie) =>
        new()
        {
            Date = observation.Date,
            Rate = observation.Value,
            FromCode = serie.ShortDescription,
            ToCode = FinanceConstants.BaseCurrencyCode
        };
}