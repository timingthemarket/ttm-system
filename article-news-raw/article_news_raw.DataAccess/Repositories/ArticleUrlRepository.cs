using article_news_raw.DataAccess.Context;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace article_news_raw.DataAccess.Repositories;

public class ArticleUrlRepository(IDbContextFactory<ArticleNewsDbContext> dbContextFactory) : IArticleUrlRepository
{
    public async Task<int> SaveBatch(List<ArticleUrl> urls, CancellationToken token = default)
    {
        if (urls.Count == 0)
            return 0;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);

        await dbContext.ArticleUrls.AddRangeAsync(urls, token);
        return await dbContext.SaveChangesAsync(token);
    }
    
    public async Task<bool> ArticleSaved(string url, CancellationToken token = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
        return await dbContext.ArticleUrls.AnyAsync(a => a.Url == url, token);
    }

    public async Task<int> SaveArticle(ArticleUrl url, CancellationToken token = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);

        await dbContext.ArticleUrls.AddAsync(url, token);
        var nrChanged = await dbContext.SaveChangesAsync(token);
        dbContext.ChangeTracker.Clear();
        return nrChanged;
    }
}
