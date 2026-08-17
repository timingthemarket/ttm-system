using Grpc.Core;
using ProtoBuf.Grpc;
using article_news_raw.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.gRPC.Services;

public class MarketDataService(IQryIndexDataHandler qryIndexDataHandler) : IMarketDataService
{
    public async ValueTask<IndexDataQryResponse> GetIndexData(IndexDataQry qry, CallContext context)
    {
        if (string.IsNullOrWhiteSpace(qry.IndexType) || !IndexTypes.All.Contains(qry.IndexType))
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Unknown index '{qry.IndexType}', expected one of {string.Join(", ", IndexTypes.All)}"));

        if (qry.DateFrom > qry.DateTo)
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"DateFrom {qry.DateFrom} is after DateTo {qry.DateTo}"));

        var indexData = await qryIndexDataHandler.HandleGetIndexData(qry.IndexType, qry.DateFrom, qry.DateTo,
            context.CancellationToken);

        return new IndexDataQryResponse
        {
            IndexData = indexData
        };
    }
}
