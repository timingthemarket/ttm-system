using riksbanken_raw.DataAccess.Interfaces;
using riksbanken_raw.DataAccess.Models;
using riksbanken_raw.Domain.Interfaces;
using ttm_system.Shared.Models.RiksbankenRaw;

namespace riksbanken_raw.Domain.Handlers.Query;

public class QryCurrencyHandler : ICurrencyQryHandler
{
    private readonly IRiksbankenRepository _riksbankenRepository;

    public QryCurrencyHandler(IRiksbankenRepository riksbankenRepository) =>
        _riksbankenRepository = riksbankenRepository;

    public async Task<List<CurrencyRateDto>> GetHistoricalCurrenciesByCode(string code)
    {
        var currencies = await _riksbankenRepository.GetCurrenciesFromCode(code);
        return MapToCurrencyDto(currencies);
    }

    private List<CurrencyRateDto> MapToCurrencyDto(List<CurrencyRate> rates) => 
        rates.Select(r => new CurrencyRateDto
        {
            Date = DateTime.Parse(r.Date),
            Rate = r.Rate,
            FromCode = r.FromCode,
            ToCode = r.ToCode
        }).ToList();
}