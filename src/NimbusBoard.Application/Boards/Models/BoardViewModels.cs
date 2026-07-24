namespace NimbusBoard.Application.Boards.Models;

public class BoardViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public IReadOnlyList<BoardColumnViewModel> Columns { get; set; } = [];
}

public class BoardColumnViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public IReadOnlyList<BoardIssueCardViewModel> Issues { get; set; } = [];
}

public class BoardIssueCardViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityClass { get; set; } = string.Empty;
    public string? AssigneeInitials { get; set; }
    public string? AssigneeClass { get; set; }
}

public class BoardListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public int IssueCount { get; set; }
}
