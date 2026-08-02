namespace TTM.Shared.Models.Discord;

public sealed record DiscordPayload(string Username, List<DiscordEmbed> Embeds);
