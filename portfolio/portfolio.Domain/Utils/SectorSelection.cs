using TTM.Shared.Constants;

namespace portfolio.Domain.Utils;

public static class SectorSelection
{
    /// <summary>
    /// Takes the best <paramref name="fraction"/> of every sector, ranked by
    /// <paramref name="value"/> in the given direction. Always at least one name per sector, so
    /// thinly populated sectors are still represented.
    /// </summary>
    public static List<T> TopPerSector<T>(
        IEnumerable<T> universe,
        Func<T, string> sector,
        Func<T, double> value,
        Direction direction,
        double fraction)
    {
        if (fraction is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(fraction), "Fraction must be within (0, 1]");

        var selected = new List<T>();

        foreach (var sectorGroup in universe.GroupBy(sector))
        {
            List<T> ordered = direction == Direction.Low
                ? sectorGroup.OrderBy(value).ToList()
                : sectorGroup.OrderByDescending(value).ToList();

            var take = Math.Max(1, (int)Math.Ceiling(ordered.Count * fraction));
            selected.AddRange(ordered.Take(take));
        }

        return selected;
    }
}
