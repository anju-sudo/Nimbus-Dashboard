namespace NimbusBoard.Application.Dashboard.Models;

public class DashboardViewModel
{
    public string UserName { get; set; } = "Jordan";
    public string UserInitials { get; set; } = "JS";
    public string WorkspaceName { get; set; } = "Acme";
    public DateTime Today { get; set; } = DateTime.Today;
    public int UnreadNotifications { get; set; }
    public DashboardStatsViewModel Stats { get; set; } = new();
    public IReadOnlyList<UrgentTaskViewModel> UrgentTasks { get; set; } = [];
    public SprintPreviewViewModel? ActiveSprint { get; set; }
    public BurndownChartViewModel Burndown { get; set; } = new();
    public IReadOnlyList<ActivityItemViewModel> RecentActivity { get; set; } = [];
    public IReadOnlyList<BoardQuickLinkViewModel> BoardLinks { get; set; } = [];
}

public class DashboardStatsViewModel
{
    public int OpenIssues { get; set; }
    public int InProgress { get; set; }
    public int DoneThisSprint { get; set; }
    public int Overdue { get; set; }
    public int Urgent { get; set; }
}

public class UrgentTaskViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityClass { get; set; } = string.Empty;
    public string StatusDotClass { get; set; } = string.Empty;
    public string DueLabel { get; set; } = string.Empty;
    public string AssigneeInitials { get; set; } = string.Empty;
    public string AssigneeClass { get; set; } = string.Empty;
}

public class SprintPreviewViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public int DaysLeft { get; set; }
    public IReadOnlyList<SprintColumnPreviewViewModel> Columns { get; set; } = [];
}

public class SprintColumnPreviewViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public IReadOnlyList<SprintCardPreviewViewModel> Cards { get; set; } = [];
}

public class SprintCardPreviewViewModel
{
    public string Title { get; set; } = string.Empty;
    public bool IsHighlighted { get; set; }
}

public class BurndownChartViewModel
{
    public IReadOnlyList<string> Labels { get; set; } = [];
    public IReadOnlyList<int> Actual { get; set; } = [];
    public IReadOnlyList<int> Ideal { get; set; } = [];
}

public class ActivityItemViewModel
{
    public string Message { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
}

public class BoardQuickLinkViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
