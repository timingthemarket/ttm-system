using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using TTM.Shared.Models.ArticleNewsRaw;

namespace TTM.Shared.gRPC.Services;

[Service("MarketDataService")]
public interface IMarketDataService
{
    /// <summary>
    /// Returns the stored history of a single index over an inclusive date range, ordered by date.
    /// </summary>
    ValueTask<IndexDataQryResponse> GetIndexData(IndexDataQry qry, CallContext context = default);
}
