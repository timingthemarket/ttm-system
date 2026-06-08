using article_news_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace article_news_raw.DataAccess.Context;

public class ArticleNewsDbContext(DbContextOptions<ArticleNewsDbContext> options) : DbContext(options)
{
    public DbSet<ArticleUrl> ArticleUrls => Set<ArticleUrl>();
    public DbSet<ArticleTickerSentiment> ArticleTickerSentiments => Set<ArticleTickerSentiment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticleUrl>(entity =>
        {
            entity.ToTable("article_url");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(e => e.Url)
                .HasColumnName("url")
                .IsRequired();

            entity.Property(e => e.DateUrlFetched)
                .HasColumnName("date_url_fetched")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.DateArticlePublished)
                .HasColumnName("date_article_published");

            entity.Property(e => e.IsContentRead)
                .HasColumnName("is_content_read")
                .HasDefaultValue(false);

            entity.Property(e => e.IsParsed)
                .HasColumnName("is_parsed")
                .HasDefaultValue(false);

            entity.Property(e => e.IsBad)
                .HasColumnName("is_bad")
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<ArticleTickerSentiment>(entity =>
        {
            entity.ToTable("article_ticker_sentiment");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(e => e.ArticleUrlId)
                .HasColumnName("article_url_id")
                .IsRequired();

            entity.Property(e => e.Ticker)
                .HasColumnName("ticker")
                .IsRequired();

            entity.Property(e => e.SentimentScore)
                .HasColumnName("sentiment_score")
                .IsRequired();

            entity.Property(e => e.RelevanceScore)
                .HasColumnName("relevance_score")
                .IsRequired();

            entity.HasOne(e => e.ArticleUrl)
                .WithMany(u => u.TickerSentiments)
                .HasForeignKey(e => e.ArticleUrlId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
