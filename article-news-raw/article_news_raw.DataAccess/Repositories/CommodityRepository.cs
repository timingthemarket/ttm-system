using article_news_raw.DataAccess.Context;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace article_news_raw.DataAccess.Repositories;

public class CommodityRepository(IDbContextFactory<ArticleNewsDbContext> dbContextFactory) : ICommodityRepository
{
    public async Task<int> UpsertCommodities(List<Commodity> commodities, CancellationToken token = default)
    {
        if (commodities.Count == 0)
            return 0;

        // ON CONFLICT DO UPDATE cannot touch the same row twice in one statement, so a duplicated
        // (date, commodity_type) in the payload would abort the whole batch.
        var distinct = commodities
            .DistinctBy(c => (c.Date, c.CommodityType))
            .ToList();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);

        const string sql =
            """
            INSERT INTO commodities (date, commodity_type, value)
            SELECT * FROM UNNEST(@dates, @types, @values)
            ON CONFLICT (date, commodity_type) DO UPDATE SET value = EXCLUDED.value
            """;

        var parameters = new object[]
        {
            new NpgsqlParameter("dates", distinct.Select(c => c.Date).ToArray()),
            new NpgsqlParameter("types", distinct.Select(c => c.CommodityType).ToArray()),
            new NpgsqlParameter("values", distinct.Select(c => c.Value).ToArray())
        };

        return await dbContext.Database.ExecuteSqlRawAsync(sql, parameters, token);
    }
}
