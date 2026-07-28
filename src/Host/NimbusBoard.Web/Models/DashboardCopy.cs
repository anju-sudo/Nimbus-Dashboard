using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Nimbus_Board.Models;

/// <summary>
/// CMS-editable dashboard labels. Values come from the Umbraco Home document;
/// defaults keep /app/dashboard working when CMS content is unavailable.
/// </summary>
public sealed class DashboardCopy
{
    public string PageTitle { get; init; } = "Dashboard";
    public string GreetingPrefix { get; init; } = "Good morning";
    public string SearchPlaceholder { get; init; } = "Search...";
    public string NewIssueLabel { get; init; } = "+ New Issue";
    public string KpiOpenIssues { get; init; } = "Open issues";
    public string KpiInProgress { get; init; } = "In progress";
    public string KpiDoneThisSprint { get; init; } = "Done this sprint";
    public string KpiOverdue { get; init; } = "Overdue";
    public string KpiUrgent { get; init; } = "Urgent";
    public string SectionUrgentTasks { get; init; } = "Urgent tasks";
    public string SectionRecentActivity { get; init; } = "Recent activity";
    public string ViewAllLabel { get; init; } = "View all";
    public string EmptyUrgentTasks { get; init; } = "No urgent tasks.";
    public string EmptyRecentActivity { get; init; } = "No recent activity.";
    public string BrandName { get; init; } = "Nimbus";

    public static DashboardCopy Defaults { get; } = new();

    public static DashboardCopy From(IPublishedContent? content)
    {
        if (content is null)
        {
            return Defaults;
        }

        return new DashboardCopy
        {
            PageTitle = Text(content, "pageTitle", Defaults.PageTitle),
            GreetingPrefix = Text(content, "greetingPrefix", Defaults.GreetingPrefix),
            SearchPlaceholder = Text(content, "searchPlaceholder", Defaults.SearchPlaceholder),
            NewIssueLabel = Text(content, "newIssueLabel", Defaults.NewIssueLabel),
            KpiOpenIssues = Text(content, "kpiOpenIssues", Defaults.KpiOpenIssues),
            KpiInProgress = Text(content, "kpiInProgress", Defaults.KpiInProgress),
            KpiDoneThisSprint = Text(content, "kpiDoneThisSprint", Defaults.KpiDoneThisSprint),
            KpiOverdue = Text(content, "kpiOverdue", Defaults.KpiOverdue),
            KpiUrgent = Text(content, "kpiUrgent", Defaults.KpiUrgent),
            SectionUrgentTasks = Text(content, "sectionUrgentTasks", Defaults.SectionUrgentTasks),
            SectionRecentActivity = Text(content, "sectionRecentActivity", Defaults.SectionRecentActivity),
            ViewAllLabel = Text(content, "viewAllLabel", Defaults.ViewAllLabel),
            EmptyUrgentTasks = Text(content, "emptyUrgentTasks", Defaults.EmptyUrgentTasks),
            EmptyRecentActivity = Text(content, "emptyRecentActivity", Defaults.EmptyRecentActivity),
            BrandName = Text(content, "brandName", Defaults.BrandName),
        };
    }

    private static string Text(IPublishedContent content, string alias, string fallback)
    {
        var value = content.Value<string>(alias);
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
