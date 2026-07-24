namespace NimbusBoard.Application.Common;

public static class TimeAgoHelper
{
    public static string Format(DateTime createdAt)
    {
        var span = DateTime.UtcNow - createdAt;
        if (span.TotalMinutes < 1)
        {
            return "just now";
        }

        if (span.TotalMinutes < 60)
        {
            return $"{(int)span.TotalMinutes}m ago";
        }

        if (span.TotalHours < 24)
        {
            return $"{(int)span.TotalHours}h ago";
        }

        if (span.TotalDays < 7)
        {
            return $"{(int)span.TotalDays}d ago";
        }

        return createdAt.ToString("MMM d, yyyy");
    }
}
