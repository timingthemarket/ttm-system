namespace TTM.Shared.Models.Discord;

public sealed record DiscordEmbed(string Title, string Description, int Color, string Timestamp, List<DiscordField> Fields);
