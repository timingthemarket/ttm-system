using boersdata_raw.Domain.Interfaces;
using ProtoBuf.Grpc;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.BoersDataRaw;
using TTM.Shared.Models.BoersDataRaw.Prices;

namespace boersdata_raw.gRPC.Services;

public class BackfillService(
    IQryHistoricalSecuritiesPricesHandler qrySecuritiesPricesHandler,
    IQryHistoricalReportsHandler historicalReportsHandler,
    IQrySecuritiesHandler qrySecuritiesHandler) : IBackfillService
{
    public async IAsyncEnumerable<HistoricalPricesDto> BackfillHistoricalPrices(
        IAsyncEnumerable<HistoricalPricesQry> qry,
        CallContext context = new CallContext())
    {
        await foreach (var tickers in qry)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var historicalPrices = await qrySecuritiesPricesHandler.HandleGetHistoricalPrices(tickers.Tickers);
            foreach (var price in historicalPrices)
            {
                yield return price;
            }
        }
    }

    public async ValueTask<HistoricalReportsQryResponse> BackfillReports(HistoricalReportsQry qry)
    {
        var reports = await historicalReportsHandler.HandleGetReports(qry.Tickers);

        return new()
        {
            Reports = reports
        };
    }

    public async ValueTask<SecuritiesQryResponse> BackfillSecurities()
    {
        var securities = await qrySecuritiesHandler.HandleGetSecurities();
        return new()
        {
            Securities = securities
        };
    }
}