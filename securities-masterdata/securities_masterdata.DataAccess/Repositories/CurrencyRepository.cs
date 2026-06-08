using System.Text;
using Microsoft.EntityFrameworkCore;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;

namespace securities_masterdata.DataAccess.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly MasterdataDbContext _dbContext;

    public CurrencyRepository(MasterdataDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CurrencyRate>> GetLatestCurrencyRatesByDate(DateOnly date)
    {
        var dateString = date.ToString();
        var qry = $"""
                   select c.* from currency_rates c INNER JOIN
                   (select cr.currency_id_from, MAX(cr.date) as max_date from currency_rates cr
                   WHERE cr.date <= '{dateString}' group by cr.currency_id_from) cc
                   ON cc.currency_id_from = c.currency_id_from AND cc.max_date = c.date
                   """;

        return await _dbContext.CurrencyRates.FromSqlRaw(qry).ToListAsync();
    }
    
    public async Task<List<Currency>> GetAllCurrencies()
    {
        return await _dbContext.Currencies.ToListAsync();
    }

    public async Task<List<CurrencyRate>> GetAllCurrencyRates()
    {
        return await _dbContext.CurrencyRates.AsNoTracking().ToListAsync();
    }

    public async Task<Currency> SaveCurrency(Currency currency)
    {
        _dbContext.Currencies.Add(currency);
        await _dbContext.SaveChangesAsync();
        return currency;
    }

    public async Task SaveRate(CurrencyRate rate)
    {
        bool exists = _dbContext.CurrencyRates
            .Any(cr => cr.CurrencyIdTo == rate.CurrencyIdTo && cr.CurrencyIdFrom == rate.CurrencyIdFrom &&
                       cr.Date == rate.Date);
        
        if (exists)
            _dbContext.CurrencyRates.Update(rate);
        else
            _dbContext.CurrencyRates.Add(rate);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task WriteManyRates(List<CurrencyRate> ratesHistories)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.CurrencyRates.AddRange(ratesHistories);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task RemoveManyRates(long currencyIdFrom)
    {
        await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM currency_rates WHERE currency_id_from = {currencyIdFrom}");
    }
    
    public async Task<Currency?> GetSingleCurrency(string currencyCode) =>
        await _dbContext.Currencies.FirstOrDefaultAsync(c => c.CurrencyCode == currencyCode);
}