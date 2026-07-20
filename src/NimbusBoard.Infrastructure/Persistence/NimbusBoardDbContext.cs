using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Infrastructure.Persistence;

public class NimbusBoardDbContext(DbContextOptions<NimbusBoardDbContext> options) : DbContext(options), INimbusBoardDbContext
{
    public DbSet<Workspace> WorkspacesSet => Set<Workspace>();
    public DbSet<Project> ProjectsSet => Set<Project>();
    public DbSet<ProjectMember> ProjectMembersSet => Set<ProjectMember>();
    public DbSet<Board> BoardsSet => Set<Board>();
    public DbSet<BoardColumn> BoardColumnsSet => Set<BoardColumn>();
    public DbSet<Sprint> SprintsSet => Set<Sprint>();
    public DbSet<Issue> IssuesSet => Set<Issue>();
    public DbSet<Comment> CommentsSet => Set<Comment>();
    public DbSet<Attachment> AttachmentsSet => Set<Attachment>();
    public DbSet<Label> LabelsSet => Set<Label>();
    public DbSet<IssueLabel> IssueLabelsSet => Set<IssueLabel>();
    public DbSet<Notification> NotificationsSet => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogsSet => Set<ActivityLog>();
    public DbSet<BurndownSnapshot> BurndownSnapshotsSet => Set<BurndownSnapshot>();

    public IQueryable<Workspace> Workspaces => WorkspacesSet.AsQueryable();
    public IQueryable<Project> Projects => ProjectsSet.AsQueryable();
    public IQueryable<ProjectMember> ProjectMembers => ProjectMembersSet.AsQueryable();
    public IQueryable<Board> Boards => BoardsSet.AsQueryable();
    public IQueryable<BoardColumn> BoardColumns => BoardColumnsSet.AsQueryable();
    public IQueryable<Sprint> Sprints => SprintsSet.AsQueryable();
    public IQueryable<Issue> Issues => IssuesSet.AsQueryable();
    public IQueryable<Comment> Comments => CommentsSet.AsQueryable();
    public IQueryable<Attachment> Attachments => AttachmentsSet.AsQueryable();
    public IQueryable<Label> Labels => LabelsSet.AsQueryable();
    public IQueryable<Notification> Notifications => NotificationsSet.AsQueryable();
    public IQueryable<ActivityLog> ActivityLogs => ActivityLogsSet.AsQueryable();
    public IQueryable<BurndownSnapshot> BurndownSnapshots => BurndownSnapshotsSet.AsQueryable();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(e =>
        {
            e.HasIndex(p => p.Key).IsUnique();
            e.HasOne(p => p.Workspace).WithMany(w => w.Projects).HasForeignKey(p => p.WorkspaceId);
        });

        modelBuilder.Entity<Issue>(e =>
        {
            e.HasIndex(i => i.Key).IsUnique();
            e.HasOne(i => i.Project).WithMany(p => p.Issues).HasForeignKey(i => i.ProjectId);
            e.HasOne(i => i.BoardColumn).WithMany(c => c.Issues).HasForeignKey(i => i.BoardColumnId);
            e.HasOne(i => i.Sprint).WithMany(s => s.Issues).HasForeignKey(i => i.SprintId);
        });

        modelBuilder.Entity<IssueLabel>(e =>
        {
            e.HasKey(il => new { il.IssueId, il.LabelId });
            e.HasOne(il => il.Issue).WithMany(i => i.IssueLabels).HasForeignKey(il => il.IssueId);
            e.HasOne(il => il.Label).WithMany(l => l.IssueLabels).HasForeignKey(il => il.LabelId);
        });

        modelBuilder.Entity<BoardColumn>(e =>
        {
            e.HasOne(c => c.Board).WithMany(b => b.Columns).HasForeignKey(c => c.BoardId);
        });

        modelBuilder.Entity<ProjectMember>(e =>
        {
            e.HasOne(m => m.Project).WithMany(p => p.Members).HasForeignKey(m => m.ProjectId);
        });

        modelBuilder.Entity<Board>(e =>
        {
            e.HasOne(b => b.Project).WithMany(p => p.Boards).HasForeignKey(b => b.ProjectId);
        });

        modelBuilder.Entity<Sprint>(e =>
        {
            e.HasOne(s => s.Project).WithMany(p => p.Sprints).HasForeignKey(s => s.ProjectId);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.HasOne(c => c.Issue).WithMany(i => i.Comments).HasForeignKey(c => c.IssueId);
        });

        modelBuilder.Entity<Attachment>(e =>
        {
            e.HasOne(a => a.Issue).WithMany(i => i.Attachments).HasForeignKey(a => a.IssueId);
        });

        modelBuilder.Entity<Label>(e =>
        {
            e.HasOne(l => l.Project).WithMany().HasForeignKey(l => l.ProjectId);
        });

        modelBuilder.Entity<ActivityLog>(e =>
        {
            e.HasOne(a => a.Issue).WithMany(i => i.ActivityLogs).HasForeignKey(a => a.IssueId);
        });

        modelBuilder.Entity<BurndownSnapshot>(e =>
        {
            e.HasOne(b => b.Sprint).WithMany(s => s.BurndownSnapshots).HasForeignKey(b => b.SprintId);
        });
    }
}
