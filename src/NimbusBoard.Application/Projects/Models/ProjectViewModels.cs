namespace NimbusBoard.Application.Projects.Models;

public class ProjectListItemViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OpenIssues { get; set; }
    public int MemberCount { get; set; }
}

public class ProjectDetailViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<ProjectMemberViewModel> Members { get; set; } = [];
    public IReadOnlyList<BoardSummaryViewModel> Boards { get; set; } = [];
    public IReadOnlyList<IssueSummaryViewModel> RecentIssues { get; set; } = [];
}

public class ProjectMemberViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class BoardSummaryViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class IssueSummaryViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AssigneeInitials { get; set; }
}
