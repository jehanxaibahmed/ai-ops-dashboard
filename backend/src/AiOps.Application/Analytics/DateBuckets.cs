namespace AiOps.Application.Analytics;

internal static class DateBuckets
{
    /// <summary>
    /// Every UTC day from the start of the range to its end, so charts show gaps as zero
    /// instead of joining across missing days. Falls back to the data's own span.
    /// </summary>
    public static IReadOnlyList<DateOnly> Days(DateTimeOffset? from, DateTimeOffset? to, IEnumerable<DateTimeOffset> observed, DateTimeOffset now)
    {
        var points = observed.ToList();
        var start = from ?? (points.Count > 0 ? points.Min() : now);
        var end = to is { } t ? t.AddTicks(-1) : now;
        if (points.Count > 0 && to is null && points.Max() > end) end = points.Max();

        var first = DateOnly.FromDateTime(start.UtcDateTime);
        var last = DateOnly.FromDateTime(end.UtcDateTime);
        var days = new List<DateOnly>();
        for (var d = first; d <= last && days.Count < 366; d = d.AddDays(1)) days.Add(d);
        return days;
    }

    public static DateOnly DayOf(DateTimeOffset value) => DateOnly.FromDateTime(value.UtcDateTime);
}
