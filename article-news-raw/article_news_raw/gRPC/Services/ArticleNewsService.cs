using ProtoBuf.Grpc;
using article_news_raw.Domain.Interfaces;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.gRPC.Services;

public class ArticleNewsService(IQryArticleNewsSentimentHandler qryArticleNewsSentimentHandler) : IArticleNewsService
{
    public async ValueTask<ArticleNewsSentimentQryResponse> GetTickerNewsSentiments(ArticleNewsSentimentQry qry, CallContext context)
    {
        var sentiments = await qryArticleNewsSentimentHandler.HandleGetTickerNewsSentiments(qry.Tickers, qry.From, qry.To);
        return new ArticleNewsSentimentQryResponse
        {
            SecurityNewsSentiments = sentiments
        };
    }
}
