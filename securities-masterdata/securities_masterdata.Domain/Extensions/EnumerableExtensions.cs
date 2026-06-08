namespace securities_masterdata.Domain.Extensions;

public static class EnumerableExtensions
{
    public static double? AverageBy<T>(this IEnumerable<T> list, Func<T, double> func)
    {
        var values = list.Select(func).ToList();
        if (values.Count > 0)
            return values.Average();

        return null;
    }
    
    public static decimal? AverageBy<T>(this IEnumerable<T> list, Func<T, decimal> func)
    {
        var values = list.Select(func).ToList();
        if (values.Count > 0)
            return values.Average();

        return null;
    }
}