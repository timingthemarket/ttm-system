using System.Collections.Frozen;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Constants;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.BoersDataRaw;
using TTM.Shared.Models.BoersDataRaw.Reports;

namespace securities_masterdata.Domain.Handlers.Sync.Backfill;

public class BackfillReportsHandler(
    ILogger<BackfillReportsHandler> logger,
    ISecurityRepository securityRepository,
    IIndicatorsRepository indicatorsRepository,
    IBackfillService backfillService,
    ICurrencyRepository currencyRepository,
    IPublishEndpoint publishEndpoint)
    : IBackfillReportsHandler
{
    public async Task HandleBackfillReports()
    {
        logger.LogInformation("Starting to backfill reports...");

        var securitiesFull = await securityRepository.GetAll();
        await HandleBackfillReportsForSecurities(securitiesFull);
    }

    public async Task HandleBackfillReports(List<string> tickers)
    {
        var securities = await securityRepository.GetSecuritiesByTickers(tickers.ToHashSet(), true);
        await HandleBackfillReportsForSecurities(securities);
    }

    private async Task HandleBackfillReportsForSecurities(List<Security> securitiesFull)
    {
        logger.LogInformation("Starting to backfill reports...");

        var rates = await currencyRepository.GetAllCurrencyRates();
        var ratesDict = rates.GroupBy(r => r.CurrencyIdFrom)
            .ToDictionary(r => r.Key, r => r.OrderByDescending(r => r.Date).ToList());
        
        foreach (var securities in securitiesFull.Chunk(10))
        {
            var tickers = securities.Select(s => s.Ticker).ToList();
            var reports = await backfillService.BackfillReports(
                new HistoricalReportsQry
                {
                    Tickers = tickers
                });

            foreach (var report in reports.Reports)
            {
                if (report.HistoricalReports == null || !report.HistoricalReports.Any())
                    continue;
                
                var indicators = MapIndicators(report.HistoricalReports, securities.ToList(), ratesDict);

                var indicator = indicators.FirstOrDefault();
                if (indicator != null)
                    await indicatorsRepository.UpdateAndReplaceAllIndicators(indicator.SecurityId, indicators, true);

                try
                {
                    await publishEndpoint.Increment(MetricNames.BACKFILL_REPORTS_CHUNK);
                } catch {}
            }
            
            logger.LogInformation("Added reports for securities {Tickers}",
                string.Join(",", securities.Select(u => u.Ticker)));
        }

        logger.LogInformation("Reports backfill complete!");
    }

    private List<Indicator> MapIndicators(List<ReportDto> reports, List<Security> securities, Dictionary<long, List<CurrencyRate>> rates)
    {
        var securityDict = securities.ToDictionary(s => s.Ticker);

        var indicators = new List<Indicator>();
        foreach (var report in reports)
        {
            if (!securityDict.TryGetValue(report.Ticker, out var security))
                continue;

            if (indicators.Any(i =>
                    i.IndicatorId == report.IndicatorId && i.SecurityId == security.SecurityId &&
                    i.Date == report.Date))
                continue;

            if (!rates.TryGetValue(security.CurrencyId, out var securityRates))
            {
                // Swedish rates wont exist in the DB
                if (security.Currency.CurrencyCode == FinanceConstants.BaseCurrencyCode)
                {
                    securityRates = new()
                    {
                        new()
                        {
                            Rate = 1,
                            Date = new()
                        }
                    };
                }
                else
                {
                    continue;
                }
            }

            // If the oldest report is older then what we have exhange data on, then we skip that report
            if (securityRates.Select(s => s.Date).Min() > report.Date)
                continue;

            var sRate = GetReportValuerate(report.IndicatorId, report.Date, securityRates);
            indicators.Add(new Indicator
            {
                Date = report.Date,
                Value = report.Value * sRate,
                IndicatorId = report.IndicatorId,
                SecurityId = security.SecurityId
            });
        }

        return indicators;
    }

    private static decimal GetReportValuerate(Indicators indicatorId, DateOnly reportDate, List<CurrencyRate> rates)
    {
        var sRate = rates.First(rate => rate.Date <= reportDate);
        List<Indicators> npConvertCurrencies = [Indicators.Roc, Indicators.Roic, Indicators.FScore];
        if (npConvertCurrencies.Contains(indicatorId))
            return 1;

        return (decimal)sRate.Rate;
    }
}