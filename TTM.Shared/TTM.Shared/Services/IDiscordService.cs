using TTM.Shared.Models.Discord;

namespace TTM.Shared.Services;

public interface IDiscordService
{
    Task SendMessageAsync(string webhookId, string webhookToken, DiscordPayload payload, CancellationToken cancellationToken = default);
}
