using NimbusBoard.Domain.Common;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Domain.Entities;

public class Issue : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid? BoardColumnId { get; set; }
    public Guid? SprintId { get; set; }
    public int Number { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssueType Type { get; set; } = IssueType.Task;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    public IssueStatus Status { get; set; } = IssueStatus.ToDo;
    public int? StoryPoints { get; set; }
    public DateTime? DueDate { get; set; }
    public int? AssigneeMemberId { get; set; }
    public string? AssigneeName { get; set; }
    public string? AssigneeInitials { get; set; }
    public int SortOrder { get; set; }

    public Project Project { get; set; } = null!;
    public BoardColumn? BoardColumn { get; set; }
    public Sprint? Sprint { get; set; }
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<IssueLabel> IssueLabels { get; set; } = [];
    public ICollection<ActivityLog> ActivityLogs { get; set; } = [];
}
