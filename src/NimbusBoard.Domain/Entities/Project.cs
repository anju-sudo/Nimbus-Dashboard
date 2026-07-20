using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class Project : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int IssueCounter { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<Board> Boards { get; set; } = [];
    public ICollection<Sprint> Sprints { get; set; } = [];
    public ICollection<Issue> Issues { get; set; } = [];
}
