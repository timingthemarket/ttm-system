namespace securities_masterdata.Domain.Extensions;

public static class DateExtensions
{
    public static DateTime ToDateWithNoTime(this DateOnly date) => date.ToDateTime(new TimeOnly());
}