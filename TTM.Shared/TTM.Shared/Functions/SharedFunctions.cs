namespace TTM.Shared.Functions;

public class SharedFunctions
{
    /// <summary>
    /// Calculates (numerator - denominator) / denominator
    /// </summary>
    /// <param name="numerator"></param>
    /// <param name="denominator"></param>
    /// <returns></returns>
    public static double CalculateFraction(double numerator, double denominator)
    {
        var diff = numerator - denominator;
        return diff / denominator;
    }
}