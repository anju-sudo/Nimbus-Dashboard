using Microsoft.EntityFrameworkCore;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Application.Common.Interfaces;

public interface INimbusBoardDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<Board> Boards { get; }
    DbSet<BoardColumn> BoardColumns { get; }
    DbSet<Sprint> Sprints { get; }
    DbSet<Issue> Issues { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<Label> Labels { get; }
    DbSet<IssueLabel> IssueLabels { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<BurndownSnapshot> BurndownSnapshots { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
