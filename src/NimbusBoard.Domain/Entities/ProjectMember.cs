using NimbusBoard.Domain.Common;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Domain.Entities;

public class ProjectMember : BaseEntity
{
    public Guid ProjectId { get; set; }
    public int MemberId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public ProjectRole Role { get; set; } = ProjectRole.Member;

    public Project Project { get; set; } = null!;
}
