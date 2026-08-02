using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using TTM.Shared.Models.ArticleNewsRaw;

namespace TTM.Shared.gRPC.Services;

[Service("ArticleNewsService")]
public interface IArticleNewsService
{
    ValueTask<ArticleNewsSentimentQryResponse> GetTickerNewsSentiments(ArticleNewsSentimentQry qry, CallContext context = default);
}
