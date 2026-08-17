using System.Diagnostics.CodeAnalysis;
using article_news_raw.DataAccess.Context;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Repositories;
using article_news_raw.DataAccess.Services;
using article_news_raw.Domain.Handlers;
using article_news_raw.Domain.Handlers.FetchMarketData;
using article_news_raw.Domain.Handlers.FetchNews;
using article_news_raw.Domain.Handlers.Query;
using article_news_raw.Domain.Interfaces;
using article_news_raw.Triggers;
using FluentMigrator.Runner;
using Grpc.Core;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.ClientFactory;
using TTM.Shared.Extensions;
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
            x.AddConsumer<FetchMarketDataTrigger>();
            x.AddConsumer<SectorSentimentReportTrigger>();

            x.SetKebabCaseEndpointNameFormatter();
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
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
        service.AddScoped<ICommodityRepository, CommodityRepository>();
        service.AddScoped<IIndexDataRepository, IndexDataRepository>();
        service.AddScoped<IEconomicIndicatorRepository, EconomicIndicatorRepository>();

        service.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(DiContainer).Assembly).For.Migrations());

        // Services
        service.AddScoped<IFinnhubApiNewsService, FinnhubApiNewsService>();
        service.AddScoped<IAlphaVantageApiNewsService, AlphaVantageApiNewsService>();
        service.AddScoped<IAlphaVantageCommoditiesService, AlphaVantageCommoditiesService>();
        service.AddScoped<IAlphaVantageIndexDataService, AlphaVantageIndexDataService>();
        service.AddScoped<IAlphaVantageEconomicIndicatorsService, AlphaVantageEconomicIndicatorsService>();


        //service.AddScoped<IFetchNewsHandler, FetchWebNewsArticlesHandler>();
        //service.AddScoped<IFetchNewsHandler, FetchRssNewsArticlesHandler>();
        //service.AddScoped<IFetchNewsUrlsHandler, FetchFinnhubApiUrlNewsHandler>();
        service.AddScoped<IFetchNewsUrlsHandler, FetchAlphavantageApiUrlsNewsHandler>();

        service.AddScoped<FetchNewsUrlsHandler>();

        // Market data sources - add new IFetchMarketDataHandler implementations here.
        service.AddScoped<IFetchMarketDataHandler, FetchCommoditiesHandler>();
        service.AddScoped<IFetchMarketDataHandler, FetchIndexDataHandler>();
        service.AddScoped<IFetchMarketDataHandler, FetchEconomicIndicatorsHandler>();

        service.AddScoped<FetchMarketDataHandler>();

        service.AddScoped<IQryArticleNewsSentimentHandler, QryArticleNewsSentimentHandler>();
        service.AddScoped<IQryIndexDataHandler, QryIndexDataHandler>();
        service.AddScoped<IQryEconomicIndicatorHandler, QryEconomicIndicatorHandler>();

        // GRPC Clients
        var masterdataUrl = Environment.GetEnvironmentVariable("MASTERDATA_URL") ?? "http://localhost:5101";

        GrpcClientFactory.AllowUnencryptedHttp2 = true;
        service.AddCodeFirstGrpcClient<TTM.Shared.gRPC.Services.IMasterdataService>(o =>
        {
            o.Address = new Uri(masterdataUrl);
            o.CallOptionsActions.Add(op =>
            {
                op.CallOptions = new CallOptions(deadline: DateTime.UtcNow.AddSeconds(60));
            });
            o.ChannelOptionsActions.Add(options =>
            {
                options.MaxRetryAttempts = 3;
                options.DisposeHttpClient = true;
                options.HttpHandler = new SocketsHttpHandler()
                {
                    // keeps connection alive
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    ConnectTimeout = TimeSpan.FromSeconds(30),
                    // allows channel to add additional HTTP/2 connections
                    EnableMultipleHttp2Connections = true
                };
            });
        });

        service.AddTtmDiscordService();

        service.AddScoped<IGenerateSectorSentimentReportHandler, GenerateSectorSentimentReportHandler>();
    }
}