namespace securities_masterdata.DataAccess.Entities;

/// <summary>
/// Values used in <see cref="Security.TradePlatform"/>. A security tradable on several
/// platforms stores them joined by <see cref="Separator"/> in this order, e.g. "Avanza, Nordnet".
/// </summary>
public static class TradePlatforms
{
    public const string Avanza = "Avanza";
    public const string Nordnet = "Nordnet";
    public const string Separator = ", ";
}
