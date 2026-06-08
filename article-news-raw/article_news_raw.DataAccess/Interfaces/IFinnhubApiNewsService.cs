using article_news_raw.DataAccess.Models;
using article_news_raw.DataAccess.Models.Api;

namespace article_news_raw.DataAccess.Interfaces;

public interface IFinnhubApiNewsService
{
    Task<List<FinnHubNewsArticle>> FetchArticles();
}