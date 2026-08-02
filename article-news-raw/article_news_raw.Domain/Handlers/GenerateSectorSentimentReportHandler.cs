using article_news_raw.Domain.Interfaces;
using article_news_raw.Domain.Models.SectorSentiment;
using MassTransit;
using Microsoft.Extensions.Logging;
using TTM.Shared.Extensions;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.ArticleNewsRaw;
using TTM.Shared.Models.Discord;
using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;
using TTM.Shared.Services;

namespace article_news_raw.Domain.Handlers;

public class GenerateSectorSentimentReportHandler(
    IQryArticleNewsSentimentHandler qryArticleNewsSentimentHandler,
    IMasterdataService masterdataService,
    IDiscordService discordService,
    IPublishEndpoint publishEndpoint,
    ILogger<GenerateSectorSentimentReportHandler> logger)
    : IGenerateSectorSentimentReportHandler
{
    private const int DiscordColor = 3447003; // Discord blurple

    private static readonly (int Days, string Label)[] Windows =
    [
        (7, "Last 7 days"),
        (14, "Last 14 days"),
        (30, "Last 30 days")
    ];

    public async Task GenerateAndSendReport()
    {
        logger.LogInformation("Running sector sentiment report");

        var webhookId = Environment.GetEnvironmentVariable("DISCORD_SENTIMENT_ID")
                        ?? throw new Exception("Environment variable 'DISCORD_SENTIMENT_ID' was not found");
        var webhookToken = Environment.GetEnvironmentVariable("DISCORD_SENTIMENT_TOKEN")
                           ?? throw new Exception("Environment variable 'DISCORD_SENTIMENT_TOKEN' was not found");

        Dictionary<string, string> tickerToSector;
        try
        {
            var securities = await masterdataService.GetSecurities(new SecuritiesQry());
            tickerToSector = BuildTickerSectorMap(securities.Securities);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to fetch securities from masterdata service");
            await publishEndpoint.SendSystemError(e, nameof(article_news_raw));
            return;
        }

        if (tickerToSector.Count == 0)
        {
            logger.LogWarning("No securities returned from masterdata service, skipping sector sentiment report");
            return;
        }

        var tickers = tickerToSector.Keys.ToList();
        var now = DateTime.UtcNow;

        foreach (var (days, label) in Windows)
        {
            try
            {
                var sentiments = await qryArticleNewsSentimentHandler.HandleGetTickerNewsSentiments(
                    tickers, now.AddDays(-days), now);

                var sectors = AggregateBySector(sentiments, tickerToSector);
                if (sectors.Count == 0)
                {
                    logger.LogWarning("No securities was found with sentiments");
                    continue;
                }
                
                var payload = BuildPayload(label, now.AddDays(-days), now, sectors);

                await discordService.SendMessageAsync(webhookId, webhookToken, payload);
                logger.LogInformation("Sent sector sentiment report for {Label}", label);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to build/send sector sentiment report for {Label}", label);
                await publishEndpoint.SendSystemError(e, nameof(article_news_raw));
            }
        }
    }

    private static Dictionary<string, string> BuildTickerSectorMap(List<SecurityDto> securities)
    {
        var map = new Dictionary<string, string>();
        foreach (var security in securities)
            map.TryAdd(security.Ticker, security.Sector);

        return map;
    }

    private static List<SectorSentimentAggregateDto> AggregateBySector(
        List<SecurityNewsSentimentDto> sentiments,
        Dictionary<string, string> tickerToSector)
    {
        return sentiments
            .Where(s => tickerToSector.ContainsKey(s.Ticker))
            .GroupBy(s => tickerToSector[s.Ticker])
            .Select(g =>
            {
                var totalOccurrences = g.Sum(s => s.NrOccurances);
                var weightedAverage = totalOccurrences == 0
                    ? 0
                    : g.Sum(s => s.AverageSentiment * s.NrOccurances) / totalOccurrences;

                return new SectorSentimentAggregateDto
                {
                    Sector = g.Key,
                    WeightedAverageSentiment = weightedAverage,
                    SimpleAverageSentiment = g.Average(s => s.AverageSentiment),
                    TotalOccurrences = totalOccurrences,
                    TopByAverageSentiment = g.OrderByDescending(s => s.AverageSentiment).Take(3).ToList(),
                    TopByOccurrences = g.OrderByDescending(s => s.NrOccurances).Take(3).ToList()
                };
            })
            .OrderByDescending(s => s.TotalOccurrences)
            .ToList();
    }

    private static DiscordPayload BuildPayload(string label, DateTime from, DateTime to, List<SectorSentimentAggregateDto> sectors)
    {
        var fields = sectors.Select(s => new DiscordField(
            $"{s.Sector} — w.avg {s.WeightedAverageSentiment:F2} | avg {s.SimpleAverageSentiment:F2} | n={s.TotalOccurrences}",
            $"Top sentiment: {FormatSecurities(s.TopByAverageSentiment)}\nTop volume: {FormatSecurities(s.TopByOccurrences)}",
            false)).ToList();

        var embed = new DiscordEmbed(
            $"Sector Sentiment — {label}",
            $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd} (UTC)",
            DiscordColor,
            DateTime.UtcNow.ToString("o"),
            fields);

        return new DiscordPayload($"Sector Sentiment Report - {label}", [embed]);
    }

    private static string FormatSecurities(List<SecurityNewsSentimentDto> securities) =>
        securities.Count == 0
            ? "-"
            : string.Join(", ", securities.Select(s => $"{s.Ticker} {s.AverageSentiment:F2}({s.NrOccurances})"));
}
