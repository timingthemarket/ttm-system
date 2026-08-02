using System.Net.Http.Json;
using TTM.Shared.Models.Discord;

namespace TTM.Shared.Services;

public class DiscordService(HttpClient httpClient) : IDiscordService
{
    public async Task SendMessageAsync(string webhookId, string webhookToken, DiscordPayload payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/webhooks/{webhookId}/{webhookToken}", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
