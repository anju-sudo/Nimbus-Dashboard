using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class Attachment : BaseEntity
{
    public Guid IssueId { get; set; }
    public int MediaId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int UploadedByMemberId { get; set; }

    public Issue Issue { get; set; } = null!;
}
