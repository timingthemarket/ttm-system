using TTM.Shared.Constants;

namespace portfolio.Domain.Constants;

public static class SupportedCalculationIndicators
{
    public static List<Indicators> SupportedIndicators => new()
    {
        Indicators.Dividend,
        Indicators.Pe,
        Indicators.Volatility,
        Indicators.Return,
        Indicators.RsiMomentum,
        Indicators.Roc,
        Indicators.Roic,
        Indicators.FScore
    };
}