namespace portfolio.Domain.Utils;

/// <summary>
/// Builds the monthly grid the indicator strength backtest steps through.
/// </summary>
public static class RebalanceDates
{
    private const int PreferredDayOfMonth = 15;

    /// <summary>
    /// The rebalance date for a given month: the 15th, stepped back one day at a time until it
    /// lands on a weekday. The 15th on a Saturday gives the 14th, on a Sunday the 13th.
    /// </summary>
    public static DateOnly ForMonth(int year, int month)
    {
        var date = new DateOnly(year, month, PreferredDayOfMonth);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            date = date.AddDays(-1);

        return date;
    }

    /// <summary>
    /// Every rebalance date from <paramref name="years"/> years before <paramref name="today"/>
    /// up to and including the month of <paramref name="today"/>, in ascending order.
    /// </summary>
    public static List<DateOnly> Generate(DateOnly today, int years)
    {
        if (years < 0) throw new ArgumentOutOfRangeException(nameof(years), "Years cannot be negative");

        var dates = new List<DateOnly>();
        var cursor = new DateOnly(today.Year, today.Month, 1).AddYears(-years);
        var last = new DateOnly(today.Year, today.Month, 1);

        while (cursor <= last)
        {
            dates.Add(ForMonth(cursor.Year, cursor.Month));
            cursor = cursor.AddMonths(1);
        }

        return dates;
    }
}
