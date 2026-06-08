using TTM.Shared.Constants;

namespace TTM.Shared.Extensions;

public static class IndicatorExtensions
{
    public static bool IsComputedIndicator(this Indicators indicator)
    {
        switch (indicator)
        {
            case Indicators.BetaOmx30:
            case Indicators.BetaNordic40:
            case Indicators.Return:
            case Indicators.Pe:
            case Indicators.Volatility:
            case Indicators.RsiMomentum:
                return true;
            default:
                return false;
        }
    }
}