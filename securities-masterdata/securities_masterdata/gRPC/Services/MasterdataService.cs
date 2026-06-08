using ProtoBuf.Grpc;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.SecuritiesMasterdata;

namespace securities_masterdata.gRPC.Services;

public class MasterdataService(IQrySecuritiesHandler qrySecuritiesHandler,
    IQrySecuritiesPricesHandler qrySecuritiesPrices,
    IQrySecuritiesIndicatorsHandler securitiesIndicatorsHandler
    ) : IMasterdataService
{
    public async ValueTask<SecuritiesQryResponse> GetSecurities(SecuritiesQry qry, CallContext context)
    {
        var securites = await qrySecuritiesHandler.HandleGetSecurities(qry);
        return new SecuritiesQryResponse
        {
            Securities = securites
        };
    }

    public async ValueTask<SecuritiesIndicatorsQryResponse> GetIndicators(SecuritiesIndicatorsQry qry, CallContext context)
    {
        var indicators =
            await securitiesIndicatorsHandler.HandleGetIndicators(qry.Date, qry.Indicators);
        return new SecuritiesIndicatorsQryResponse
        {
            Variables = indicators,
            Date = qry.Date,
        };
    }

    public async ValueTask<SecuritiesPricesQryResponse> GetLatestPrices(SecuritiesPricesQry qry, CallContext context)
    {
        var prices =
            await qrySecuritiesPrices.HandleGetTickerDatePrices(qry.Date, qry.SecurityIds);
        return new SecuritiesPricesQryResponse
        {
            SecurityPrices = prices
        };  
    }
}