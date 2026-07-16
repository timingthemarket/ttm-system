using System.Globalization;
using CsvHelper;
using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace portfolio.Domain.Handlers;

public class YahooCsvFileHandler(IMasterdataService masterdataService) : IYahooCsvFileHandler
{
    public async Task<Stream> HandleMakeYahooCsvFile(PortfolioDto portfolioDto)
    {
        var ids = portfolioDto.PortfolioValues.Select(p => p.SecurityId).ToList();
        var securities = (await masterdataService.GetSecurites(null, ids, true)).Securities
            .ToDictionary(s => s.SecurityId);

        var records = GetYahooExportRows(portfolioDto.SecuritiesDate, portfolioDto.PortfolioValues, securities).ToList();

        var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream, leaveOpen: true);
        await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            await csv.WriteRecordsAsync(records);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private IEnumerable<YahooExportRow> GetYahooExportRows(DateOnly securitiesDate, List<PortfolioValueDto> values, Dictionary<long, SecurityDto> securityDict)
    {
        var tradeDate = securitiesDate.ToString("yyyyMMdd");
        foreach (var v in values)
        {
            var security = securityDict[v.SecurityId];
            if (security.LatestRawPrice <= 0)
                continue;
            
            yield return new YahooExportRow
            {
                Commission = 0,
                Quantity = v.Amount,
                PurchasePrice = Math.Round(security.LatestRawPrice, 2),
                TradeDate = tradeDate,
                Symbol = security.YahooTicker
            };
        }
    }
}