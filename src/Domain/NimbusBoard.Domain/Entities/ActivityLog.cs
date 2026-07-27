using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public Guid? IssueId { get; set; }
    public Guid? ProjectId { get; set; }
    public int ActorMemberId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Detail { get; set; }

    public Issue? Issue { get; set; }
}
