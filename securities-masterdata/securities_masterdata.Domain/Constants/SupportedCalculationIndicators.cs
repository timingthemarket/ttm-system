using TTM.Shared.Constants;

namespace securities_masterdata.Domain.Constants;

public class SupportedCalculationIndicators
{
    public static List<Indicators> SupportedIndicators => new()
    {
        Indicators.Dividend,
        Indicators.Pe,
        Indicators.Volatility,
        Indicators.Return
    };
}