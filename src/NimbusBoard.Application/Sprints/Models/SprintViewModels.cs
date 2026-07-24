using NimbusBoard.Application.Dashboard.Models;

namespace NimbusBoard.Application.Sprints.Models;

public class SprintListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public int IssueCount { get; set; }
    public int DaysLeft { get; set; }
}

public class SprintDetailViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public int DaysLeft { get; set; }
    public BurndownChartViewModel Burndown { get; set; } = new();
    public IReadOnlyList<SprintIssueItemViewModel> Issues { get; set; } = [];
    public IReadOnlyList<SprintIssueItemViewModel> BacklogCandidates { get; set; } = [];
}

public class SprintIssueItemViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int StoryPoints { get; set; }
    public string? AssigneeInitials { get; set; }
}

public class SprintCreateFormViewModel
{
    public IReadOnlyList<SprintProjectOptionViewModel> Projects { get; set; } = [];
}

public class SprintProjectOptionViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
