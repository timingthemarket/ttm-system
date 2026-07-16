using System.Net.Http.Json;
using System.Text;
using portfolio.DataAccess.Models.Services;
using TTM.Shared.Constants;
using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.DataAccess.Services;

public class DiscordService
{
    private const int DiscordColor = 15548997; // https://gist.github.com/thomasbnt/b6f455e2c7d743b796917fa3c205f812

    private readonly HttpClient _client;
    private readonly string _discordId;
    private readonly string _discordToken;
    
    public DiscordService()
    {
        _client = new HttpClient();
        _client.Timeout = TimeSpan.FromSeconds(10);
        _client.BaseAddress = new Uri("https://discord.com/");
        
        _discordId = Environment.GetEnvironmentVariable("DISCORD_ID")
                    ?? throw new Exception("Environment variable 'DISCORD_ID' was not found");
        _discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN")
                       ?? throw new Exception("Environment variable 'DISCORD_TOKEN' was not found");
    }
    
    /// <summary>
    /// https://discord.com/developers/docs/resources/channel#embed-object-embed-field-structure
    /// </summary>
    public async Task SendPortfolioUpdateNotification(PortfolioUpdateNotificationDto notification)
    {
        string discordUsername = $"Best indicators update - {notification.SessionDate}";
        
        var bodySummary = $"Indicators that has produced the best results has been calculated parameters:" +
                          $"\nDate: {notification.SessionPortfolio.SecuritiesDate}" +
                          $"\nRow similarity: {notification.SessionPortfolio.RowSimilarity}" +
                          $"\nMoney: {notification.SessionPortfolio.Money}";

        bodySummary += "\nThe result was based on the following portfolio:";
        bodySummary += $"\nId: {notification.SessionPortfolio.Id}";
        
        var resultTable = CreateResultsTable(notification.SessionPortfolio);
        
        bodySummary += $"\n {resultTable}";

        var portfolioChange = Math.Round(notification.SessionPortfolio.PortfolioPercentageChange * 100, 2);
        bodySummary += $"\n Portfolio change {portfolioChange}%";
        
        var oldIndicatorsString = string.Join("\n", notification.OldIndicators.OrderBy(i => i.Indicator).Select(CreateIndicatorString));
        var newIndicatorsString = string.Join("\n", notification.NewIndicators.OrderBy(i => i.Indicator).Select(CreateIndicatorString));
        
        var oldIndicatorsField = new DiscordField("Previous indicators", oldIndicatorsString, false);
        var newIndicatorsField = new DiscordField("New indicators", newIndicatorsString, false);
        
        var embed = new DiscordEmbed("Indicators summary", bodySummary, DiscordColor, DateTime.UtcNow.ToString("o"), new () { oldIndicatorsField, newIndicatorsField });
        
        var embedHistorical = new DiscordEmbed("Historical statistics", "test", DiscordColor, DateTime.UtcNow.ToString("o"), new ());
        
        var payload = new DiscordPayload(discordUsername, new List<DiscordEmbed> { embed, embedHistorical });

        var url = $"api/webhooks/{_discordId}/{_discordToken}";
        await _client.PostAsJsonAsync(url, payload); 
    }

    private static string CreateResultsTable(SessionPortfolio portfolio)
    {
        var sb = new StringBuilder();
        var header = "Security | Amount | Rank";
        var separator = new string('-', header.Length);
        sb.AppendLine("```");
        sb.AppendLine(header);
        sb.AppendLine(separator);

        foreach (var sectorSecurites in portfolio.Securities.GroupBy(p => p.Sector))
        {
            var sectorName = sectorSecurites.Key;
            sb.AppendLine($"{sectorName}");
            foreach (var security in sectorSecurites)
            {
                sb.AppendLine($"{security.Ticker,-8} | {security.Amount,-6} | {security.Rank}");
            }
            sb.AppendLine(separator);
        }

        sb.AppendLine("```");

        return sb.ToString();
    }

    private string CreateIndicatorString(SessionIndicator dto)
    {
        var str = $"{dto.Indicator} ({(int)dto.Indicator}) - {dto.Direction}";
        if (dto is { LookBackPeriod: not null, LookBackAggregator: not null })
        {
            str += $" - {dto.LookBackPeriod}|{dto.LookBackAggregator}";
        }
        
        return str;
    }
    
    private sealed record DiscordField(string name, string value, bool inline);
    private sealed record DiscordEmbed(string title, string description, int color, string timestamp,
        List<DiscordField> fields);
    private sealed record DiscordPayload(string username, List<DiscordEmbed> embeds);
}