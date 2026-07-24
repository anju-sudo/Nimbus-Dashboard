using NimbusBoard.Application.Collaboration.Models;

namespace NimbusBoard.Application.Issues.Models;

public class IssueDetailViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssigneeName { get; set; }
    public string? AssigneeInitials { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public Guid? BoardColumnId { get; set; }
    public Guid ProjectId { get; set; }
    public IReadOnlyList<CommentViewModel> Comments { get; set; } = [];
    public IReadOnlyList<AttachmentViewModel> Attachments { get; set; } = [];
    public IReadOnlyList<LabelViewModel> Labels { get; set; } = [];
    public IReadOnlyList<IssueActivityViewModel> Activity { get; set; } = [];
}

public class IssueListItemViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityClass { get; set; } = string.Empty;
    public string? AssigneeInitials { get; set; }
    public string? DueLabel { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
}

public class CreateIssueFormModel
{
    public Guid ProjectId { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public Guid? BoardColumnId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Task";
    public string Priority { get; set; } = "Medium";
    public int? StoryPoints { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssigneeName { get; set; }
    public string? AssigneeInitials { get; set; }
}
