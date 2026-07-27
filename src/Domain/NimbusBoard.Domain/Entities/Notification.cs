using NimbusBoard.Domain.Common;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Domain.Entities;

public class Notification : BaseEntity
{
    public int RecipientMemberId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public Guid? IssueId { get; set; }
}
