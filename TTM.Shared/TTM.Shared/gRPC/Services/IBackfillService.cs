using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using TTM.Shared.Models.BoersDataRaw;
using TTM.Shared.Models.BoersDataRaw.Prices;

namespace TTM.Shared.gRPC.Services;

[Service("BackfillService")]
public interface IBackfillService
{
    IAsyncEnumerable<HistoricalPricesDto> BackfillHistoricalPrices(IAsyncEnumerable<HistoricalPricesQry> qry, CallContext context = default);

    ValueTask<HistoricalReportsQryResponse> BackfillReports(HistoricalReportsQry qry);

    ValueTask<SecuritiesQryResponse> BackfillSecurities();
}