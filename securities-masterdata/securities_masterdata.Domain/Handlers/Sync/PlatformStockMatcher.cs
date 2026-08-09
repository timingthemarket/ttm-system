namespace securities_masterdata.Domain.Handlers.Sync;

/// <summary>
/// Looks up securities in the stock list of a single trading platform: by ticker first,
/// falling back to a fuzzy comparison of company names.
/// </summary>
public class PlatformStockMatcher
{
    private const double SimilarityThreshold = 0.8;

    private readonly HashSet<string> _tickers;
    private readonly HashSet<string> _names;
    private readonly HashSet<string> _cleanedNames;

    // Names with the company suffix stripped and lower cased, ready for edit distance
    // comparison. Precomputed because every unmatched security is compared against all of them.
    private readonly string[] _comparableNames;

    public PlatformStockMatcher(IEnumerable<(string? Ticker, string? Name)> stocks)
    {
        _tickers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _cleanedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var comparableNames = new List<string>();

        foreach (var (ticker, name) in stocks)
        {
            if (!string.IsNullOrWhiteSpace(ticker))
                _tickers.Add(ticker);

            if (string.IsNullOrWhiteSpace(name) || !_names.Add(name))
                continue;

            var cleaned = CleanName(name);
            if (_cleanedNames.Add(cleaned))
                comparableNames.Add(cleaned.ToLowerInvariant());
        }

        _comparableNames = comparableNames.ToArray();
    }

    public int TickerCount => _tickers.Count;

    /// <summary>
    /// True when the platform lists a stock with this ticker, or with a close enough name.
    /// </summary>
    public bool Matches(string? ticker, string? name)
    {
        if (!string.IsNullOrWhiteSpace(ticker) && _tickers.Contains(ticker))
            return true;

        return !string.IsNullOrWhiteSpace(name) && HasSimilarName(name);
    }

    private bool HasSimilarName(string securityName)
    {
        if (_names.Contains(securityName))
            return true;

        var cleaned = CleanName(securityName);
        if (_cleanedNames.Contains(cleaned))
            return true;

        var comparable = cleaned.ToLowerInvariant();

        foreach (var candidate in _comparableNames)
        {
            // The edit distance is at least the length difference, so a name whose length is too
            // far off can never clear the threshold. Cheap to check, and it skips almost everything.
            var maxLength = Math.Max(comparable.Length, candidate.Length);
            if (maxLength == 0)
                continue;

            var lengthDifference = Math.Abs(comparable.Length - candidate.Length);
            if (1.0 - (double)lengthDifference / maxLength < SimilarityThreshold)
                continue;

            var distance = LevenshteinDistance(comparable, candidate);
            if (1.0 - (double)distance / maxLength >= SimilarityThreshold)
                return true;
        }

        return false;
    }

    private static string CleanName(string cleanName)
    {
        foreach (var suffix in CompanySuffixes)
        {
            if (cleanName.EndsWith(" " + suffix, StringComparison.OrdinalIgnoreCase))
            {
                cleanName = cleanName.Substring(0, cleanName.Length - suffix.Length - 1).Trim();
                break; // Stop after first match
            }

            if (cleanName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                cleanName = cleanName.Substring(0, cleanName.Length - suffix.Length).Trim();
                break;
            }
        }

        return cleanName.Trim();
    }

    private static int LevenshteinDistance(string source, string target)
    {
        if (source.Length == 0) return target.Length;
        if (target.Length == 0) return source.Length;

        var previousRow = new int[target.Length + 1];
        var currentRow = new int[target.Length + 1];

        for (var j = 0; j <= target.Length; j++)
            previousRow[j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            currentRow[0] = i;

            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                currentRow[j] = Math.Min(
                    Math.Min(previousRow[j] + 1, currentRow[j - 1] + 1),
                    previousRow[j - 1] + cost);
            }

            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[target.Length];
    }

    private static readonly string[] CompanySuffixes =
    [
        // Limited Liability Companies
        "Ltd",
        "LLC",
        "GmbH",
        "Sàrl",
        "BV",
        "ApS",
        "AS",
        "AB",
        "Oy",

        // Public Limited Companies
        "PLC",
        "AG",
        "SA",
        "SpA",
        "NV",
        "A/S",

        // US Corporations
        "Inc.",
        "Inc",
        "Corp.",
        "Corp",
        "Co.",
        "Co",

        // Partnerships
        "LLP",
        "LP",

        // Other Structures
        "SE",
        "SCE",
        "REIT",
        "SICAV"
    ];
}
