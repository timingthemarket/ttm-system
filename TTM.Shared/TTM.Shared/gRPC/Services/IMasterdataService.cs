using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using TTM.Shared.Models.SecuritiesMasterdata;

namespace TTM.Shared.gRPC.Services;

[Service("MasterdataService")]
public interface IMasterdataService
{
    
    ValueTask<SecuritiesQryResponse> GetSecurities(SecuritiesQry qry, CallContext context = default);

    ValueTask<SecuritiesIndicatorsQryResponse> GetIndicators(SecuritiesIndicatorsQry qry, CallContext context = default);

    ValueTask<SecuritiesPricesQryResponse> GetLatestPrices(SecuritiesPricesQry qry, CallContext context = default);
}