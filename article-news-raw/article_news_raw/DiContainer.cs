using System.Diagnostics.CodeAnalysis;
using article_news_raw.DataAccess.Context;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Repositories;
using article_news_raw.DataAccess.Services;
using article_news_raw.Domain.Handlers;
using article_news_raw.Domain.Handlers.FetchNews;
using article_news_raw.Domain.Interfaces;
using article_news_raw.Triggers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TTM.Shared.Filters;

namespace article_news_raw;

[ExcludeFromCodeCoverage]
public static class DiContainer
{
    public static void ConfigureMasstransit(this IServiceCollection service, IWebHostEnvironment env)
    {
        service.AddMassTransit(x =>
        {
            x.AddConsumer<FetchNewsUrlsTrigger>();

            x.SetKebabCaseEndpointNameFormatter();
            x.UsingInMemory();
        });
    }

    public static void AddCustomServices(this IServiceCollection service, IWebHostEnvironment env)
    {
        service.ConfigureMasstransit(env);
        service.AddMemoryCache();

        // Database
        var connectionString = Environment.GetEnvironmentVariable("POSTGRESSQL_CONN")
                               ?? throw new Exception("POSTGRESSQL_CONN is null");
        service.AddDbContextFactory<ArticleNewsDbContext>(options => options.UseNpgsql(connectionString));
        service.AddScoped<IArticleUrlRepository, ArticleUrlRepository>();

        // Services
        service.AddScoped<IFinnhubApiNewsService, FinnhubApiNewsService>();
        service.AddScoped<IAlphaVantageApiNewsService, AlphaVantageApiNewsService>();


        //service.AddScoped<IFetchNewsHandler, FetchWebNewsArticlesHandler>();
        //service.AddScoped<IFetchNewsHandler, FetchRssNewsArticlesHandler>();
        //service.AddScoped<IFetchNewsUrlsHandler, FetchFinnhubApiUrlNewsHandler>();
        service.AddScoped<IFetchNewsUrlsHandler, FetchAlphavantageApiUrlsNewsHandler>();

        service.AddScoped<FetchNewsUrlsHandler>();
    }
}