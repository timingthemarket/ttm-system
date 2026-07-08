namespace portfolio.Domain.Constants;

public enum LogVerbosity
{
    All,
    Warning,
    None
}

public static class InternalSettings
{
    public static LogVerbosity Verbosity { get; set; } = LogVerbosity.All;
    public const double DefaultRowSimilarity = 0.0001;
}