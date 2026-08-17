using article_news_raw.DataAccess.Context;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace article_news_raw.DataAccess.Repositories;

public class EconomicIndicatorRepository(IDbContextFactory<ArticleNewsDbContext> dbContextFactory) : IEconomicIndicatorRepository
{
    public async Task<int> UpsertEconomicIndicators(List<EconomicIndicator> economicIndicators, CancellationToken token = default)
    {
        if (economicIndicators.Count == 0)
            return 0;

        // ON CONFLICT DO UPDATE cannot touch the same row twice in one statement, so a duplicated
        // (date, indicator_type) in the payload would abort the whole batch.
        var distinct = economicIndicators
            .DistinctBy(e => (e.Date, e.IndicatorType))
            .ToList();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);

        const string sql =
            """
            INSERT INTO economic_indicator (date, indicator_type, value)
            SELECT * FROM UNNEST(@dates, @types, @values)
            ON CONFLICT (date, indicator_type) DO UPDATE SET value = EXCLUDED.value
            """;

        var parameters = new object[]
        {
            new NpgsqlParameter("dates", distinct.Select(e => e.Date).ToArray()),
            new NpgsqlParameter("types", distinct.Select(e => e.IndicatorType).ToArray()),
            new NpgsqlParameter("values", distinct.Select(e => e.Value).ToArray())
        };

        return await dbContext.Database.ExecuteSqlRawAsync(sql, parameters, token);
    }

    public async Task<List<EconomicIndicator>> GetEconomicIndicators(string indicatorType, DateOnly dateFrom, DateOnly dateTo, CancellationToken token = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);

        return await dbContext.EconomicIndicators
            .AsNoTracking()
            .Where(e => e.IndicatorType == indicatorType && e.Date >= dateFrom && e.Date <= dateTo)
            .OrderBy(e => e.Date)
            .ToListAsync(token);
    }
}
