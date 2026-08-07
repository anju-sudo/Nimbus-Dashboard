using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Abstractions;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Infrastructure.Persistence;

public class NimbusBoardDbContext(DbContextOptions<NimbusBoardDbContext> options) : DbContext(options), INimbusBoardDbContext
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<IssueLabel> IssueLabels => Set<IssueLabel>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<BurndownSnapshot> BurndownSnapshots => Set<BurndownSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NimbusBoardDbContext).Assembly);
    }
}
