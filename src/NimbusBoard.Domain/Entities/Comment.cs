using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid IssueId { get; set; }
    public int AuthorMemberId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public Issue Issue { get; set; } = null!;
}
