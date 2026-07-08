namespace portfolio.Domain.Constants;

public class TimeIntervals
{
    public const string Weekly = "Weekly";
    public const string BiWeekly = "BiWeekly";
    public const string Monthly = "Monthly";
    public const string BiMonthly = "BiMonthly";
    public const string Quarterly = "Quarterly";

    public static List<string> AllIntervals => new()
    {
        /*Weekly,*/ BiWeekly, Monthly, BiMonthly, Quarterly
    };

    public static int GetNrDaysForInterval(string interval) => interval switch
    {
        Weekly => 7,
        BiWeekly => 14,
        Monthly => 30,
        BiMonthly => 60,
        Quarterly => 91,
        _ => throw new Exception($"{interval} does not exist")
    };

    public static DateOnly GetSimulationDateStart(string interval)
    {
        var dt = DateTime.UtcNow;
        
        return interval switch
        {
            Weekly => DateOnly.FromDateTime(dt.AddMonths(-3)),
            BiWeekly => DateOnly.FromDateTime(dt.AddMonths(-6)),
            Monthly => DateOnly.FromDateTime(dt.AddYears(-1)),
            BiMonthly => DateOnly.FromDateTime(dt.AddYears(-2)),
            Quarterly => DateOnly.FromDateTime(dt.AddYears(-2)),
            _ => throw new Exception($"{interval} does not exist")
        };
    }

    public static int GetNrPeriods(string interval, DateOnly fromDate, DateOnly toDate)
    {
        var nrDaysInterval = GetNrDaysForInterval(interval);

        var fromDateIter = fromDate;
        int nrPeriods = 0;
        while (fromDateIter < toDate)
        {
            fromDateIter = fromDateIter.AddDays(nrDaysInterval);
            if (fromDateIter > toDate)
                break;
            nrPeriods++;
        }

        return nrPeriods;
    }
}