using System.Text.Json;

namespace portfolio.DataAccess.Models.Db;

/// <summary>
/// The raw, pre-normalization numbers behind <see cref="IndicatorStrength.Strength"/> for one
/// indicator at one date, stored as JSON on the row itself. Strength is only meaningful relative
/// to the other indicators scored at the same date, so keeping the inputs alongside it is what
/// makes a single row interpretable on its own.
/// </summary>
/// <param name="Sharpe">Annualised rolling Sharpe ratio of the artificial portfolio.</param>
/// <param name="Ic">
/// Mean Information Coefficient over the same rolling window, or null when no period in the window
/// produced one.
/// </param>
public sealed record IndicatorStrengthMetadata(double Sharpe, double? Ic)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string ToJson() => JsonSerializer.Serialize(this, Options);
}
