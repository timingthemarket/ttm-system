using article_news_raw.DataAccess.Models.Api;

namespace article_news_raw.DataAccess.Interfaces;

public interface IAlphaVantageApiNewsService
{
    Task<AlphaVantageNewsArticle> GetManufacturingNews(int limit, DateTime timeFrom, DateTime? timeTo = null);
    Task<AlphaVantageNewsArticle> GetFinancialMarketNews(int limit, DateTime timeFrom, DateTime? timeTo = null);
    Task<AlphaVantageNewsArticle> GetFinanceNews(int limit, DateTime timeFrom, DateTime? timeTo = null);
    Task<AlphaVantageNewsArticle> GetMacroEconomyNews(int limit, DateTime timeFrom, DateTime? timeTo = null);
    Task<AlphaVantageNewsArticle> GetTechnologyNews(int limit, DateTime timeFrom, DateTime? timeTo = null);
    Task<AlphaVantageNewsArticle> GetRetailWholesaleNews(int limit, DateTime timeFrom, DateTime? timeTo = null);
    Task<AlphaVantageNewsArticle> GetEarningsNews(int limit, DateTime timeFrom, DateTime? timeTo = null);
}