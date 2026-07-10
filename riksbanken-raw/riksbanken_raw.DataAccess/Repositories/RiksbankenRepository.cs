using Marten;
using riksbanken_raw.DataAccess.Interfaces;
using riksbanken_raw.DataAccess.Models;

namespace riksbanken_raw.DataAccess.Repositories;

public class RiksbankenRepository : IRiksbankenRepository
{
    private readonly IDocumentStore _store;

    public RiksbankenRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<List<ExchangeRateSeries>> GetExchangeRateSeries()
    {
        await using var session = _store.LightweightSession();
        var series = await session.Query<ExchangeRateSeries>().ToListAsync();
        return series.ToList();
    }

    public async Task<bool> UpdateLatestFetchedDate(string seriesId, DateTime latestDate)
    {
        await using var session = _store.LightweightSession();
        var serie = await session.Query<ExchangeRateSeries>()
            .FirstOrDefaultAsync(s => s.SeriesId == seriesId);
        if (serie is null)
            return false;

        serie.LastFetched = latestDate;
        session.Store(serie);
        await session.SaveChangesAsync();
        return true;
    }

    public async Task<List<CurrencyRate>> GetCurrenciesFromCode(string code)
    {
        await using var session = _store.LightweightSession();
        var rates = await session.Query<CurrencyRate>()
            .Where(c => c.FromCode == code)
            .ToListAsync();
        return rates.ToList();
    }

    public async Task<bool> SaveCurrency(CurrencyRate cur)
    {
        await using var session = _store.LightweightSession();
        var existing = await session.Query<CurrencyRate>()
            .FirstOrDefaultAsync(c => c.FromCode == cur.FromCode && c.Date == cur.Date);
        if (existing is not null)
            cur.Id = existing.Id;

        session.Store(cur);
        await session.SaveChangesAsync();
        return true;
    }

    public async Task<int> SaveHistoricalCurrencies(List<CurrencyRate> curriencies)
    {
        if (curriencies.Count == 0)
            return 0;

        await using var session = _store.LightweightSession();

        var fromCodes = curriencies.Select(c => c.FromCode).Distinct().ToList();
        var existing = await session.Query<CurrencyRate>()
            .Where(c => c.FromCode.IsOneOf(fromCodes))
            .ToListAsync();

        var seenKeys = existing
            .Select(e => (e.Date, e.ToCode, e.FromCode))
            .ToHashSet();

        var duplicates = 0;
        foreach (var cur in curriencies)
        {
            if (!seenKeys.Add((cur.Date, cur.ToCode, cur.FromCode)))
            {
                duplicates++;
                continue;
            }

            session.Store(cur);
        }

        await session.SaveChangesAsync();
        return duplicates;
    }
}
