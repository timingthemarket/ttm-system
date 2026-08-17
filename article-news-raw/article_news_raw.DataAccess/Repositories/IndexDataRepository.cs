using article_news_raw.DataAccess.Context;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace article_news_raw.DataAccess.Repositories;

public class IndexDataRepository(IDbContextFactory<ArticleNewsDbContext> dbContextFactory) : IIndexDataRepository
{
    public async Task<int> UpsertIndexData(List<IndexData> indexData, CancellationToken token = default)
    {
        if (indexData.Count == 0)
            return 0;

        // ON CONFLICT DO UPDATE cannot touch the same row twice in one statement, so a duplicated
        // (date, index_type) in the payload would abort the whole batch.
        var distinct = indexData
            .DistinctBy(i => (i.Date, i.IndexType))
            .ToList();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);

        const string sql =
            """
            INSERT INTO index_data (date, index_type, value)
            SELECT * FROM UNNEST(@dates, @types, @values)
            ON CONFLICT (date, index_type) DO UPDATE SET value = EXCLUDED.value
            """;

        var parameters = new object[]
        {
            new NpgsqlParameter("dates", distinct.Select(i => i.Date).ToArray()),
            new NpgsqlParameter("types", distinct.Select(i => i.IndexType).ToArray()),
            new NpgsqlParameter("values", distinct.Select(i => i.Value).ToArray())
        };

        return await dbContext.Database.ExecuteSqlRawAsync(sql, parameters, token);
    }

    public async Task<List<IndexData>> GetIndexData(string indexType, DateOnly dateFrom, DateOnly dateTo, CancellationToken token = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);

        return await dbContext.IndexData
            .AsNoTracking()
            .Where(i => i.IndexType == indexType && i.Date >= dateFrom && i.Date <= dateTo)
            .OrderBy(i => i.Date)
            .ToListAsync(token);
    }
}
