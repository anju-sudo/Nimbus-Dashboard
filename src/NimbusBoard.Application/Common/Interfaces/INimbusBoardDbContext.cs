using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Application.Common.Interfaces;

public interface INimbusBoardDbContext
{
    IQueryable<Workspace> Workspaces { get; }
    IQueryable<Project> Projects { get; }
    IQueryable<ProjectMember> ProjectMembers { get; }
    IQueryable<Board> Boards { get; }
    IQueryable<BoardColumn> BoardColumns { get; }
    IQueryable<Sprint> Sprints { get; }
    IQueryable<Issue> Issues { get; }
    IQueryable<Comment> Comments { get; }
    IQueryable<Attachment> Attachments { get; }
    IQueryable<Label> Labels { get; }
    IQueryable<Notification> Notifications { get; }
    IQueryable<ActivityLog> ActivityLogs { get; }
    IQueryable<BurndownSnapshot> BurndownSnapshots { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
