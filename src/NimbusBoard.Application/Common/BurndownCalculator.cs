namespace NimbusBoard.Application.Common;

public static class BurndownCalculator
{
    public static int CalculateIdealPoints(int totalStoryPoints, DateTime startDate, DateTime endDate, DateTime asOfDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        var asOf = asOfDate.Date;

        if (asOf <= start)
        {
            return totalStoryPoints;
        }

        if (asOf >= end)
        {
            return 0;
        }

        var totalDays = Math.Max(1, (end - start).Days);
        var elapsedDays = (asOf - start).Days;
        var remaining = totalStoryPoints - (elapsedDays * (double)totalStoryPoints / totalDays);
        return Math.Max(0, (int)Math.Round(remaining));
    }

    public static int CalculateRemainingPoints(IEnumerable<(int StoryPoints, bool IsDone)> issues)
    {
        return issues
            .Where(i => !i.IsDone)
            .Sum(i => Math.Max(0, i.StoryPoints));
    }

    public static IReadOnlyList<(DateTime Date, int Ideal)> BuildIdealSeries(
        int totalStoryPoints,
        DateTime startDate,
        DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        if (end < start)
        {
            end = start;
        }

        var series = new List<(DateTime Date, int Ideal)>();
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            series.Add((day, CalculateIdealPoints(totalStoryPoints, start, end, day)));
        }

        return series;
    }
}
