using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NimbusBoard.Application;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;
using NimbusBoard.Infrastructure.Persistence;
using NimbusBoard.Infrastructure.Services;

namespace NimbusBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNimbusBoardInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NimbusBoardDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<INimbusBoardDbContext>(sp => sp.GetRequiredService<NimbusBoardDbContext>());
        services.AddScoped<IIssueKeyFactory, IssueKeyFactory>();
        services.AddScoped<IBurndownService, BurndownService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IAppNotificationService, NotificationPublisher>();
        services.AddOptions<SmtpOptions>();
        services.AddNimbusBoardApplication();

        return services;
    }

    public static async Task EnsureNimbusBoardDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NimbusBoardDbContext>();

        await db.Database.EnsureCreatedAsync();

        if (!await SchemaIsPresentAsync(db))
        {
            // Stale/empty SQLite file exists — EnsureCreated does not update it.
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        if (!await db.Projects.AnyAsync())
        {
            await SeedAsync(db);
        }
        else if (!await db.Labels.AnyAsync())
        {
            await SeedCollaborationIfMissingAsync(db);
        }
    }

    private static async Task<bool> SchemaIsPresentAsync(NimbusBoardDbContext db)
    {
        try
        {
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Projects'";
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
        catch (SqliteException)
        {
            return false;
        }
    }

    private static async Task SeedAsync(NimbusBoardDbContext db)
    {
        var workspace = new Workspace { Name = "Acme", Slug = "acme" };
        var project = new Project
        {
            Workspace = workspace,
            Key = "NIM",
            Name = "Nimbus Platform",
            Description = "Core platform delivery",
            IssueCounter = 104
        };

        var sprint = new Sprint
        {
            Project = project,
            Name = "Sprint 14 · Checkout revamp",
            Goal = "Checkout revamp",
            StartDate = DateTime.UtcNow.AddDays(-8),
            EndDate = DateTime.UtcNow.AddDays(6),
            IsActive = true,
            TotalStoryPoints = 20,
            CompletedStoryPoints = 12
        };

        var board = new Board { Project = project, Name = "Engineering Kanban" };
        var todoCol = new BoardColumn { Board = board, Name = "To Do", SortOrder = 1 };
        var progressCol = new BoardColumn { Board = board, Name = "In Progress", SortOrder = 2 };
        var doneCol = new BoardColumn { Board = board, Name = "Done", SortOrder = 3 };

        var issues = new List<Issue>
        {
            CreateIssue(project, sprint, todoCol, 104, "Payment gateway timing out for EU users", IssuePriority.Highest, IssueStatus.ToDo, DateTime.UtcNow.Date, "PR", "Priya R."),
            CreateIssue(project, sprint, todoCol, 99, "Security patch for auth token leak", IssuePriority.Highest, IssueStatus.ToDo, DateTime.UtcNow.Date.AddDays(-1), "AL", "Alex L."),
            CreateIssue(project, sprint, progressCol, 87, "Board drag-and-drop", IssuePriority.High, IssueStatus.InProgress, DateTime.UtcNow.Date.AddDays(2), "AB", "Anjumol Babu"),
            CreateIssue(project, sprint, progressCol, 76, "Sprint burndown chart", IssuePriority.High, IssueStatus.InProgress, DateTime.UtcNow.Date.AddDays(3), "MK", "Maya K."),
            CreateIssue(project, sprint, todoCol, 71, "Fix login redirect", IssuePriority.Medium, IssueStatus.ToDo, DateTime.UtcNow.Date.AddDays(4), "AB", "Anjumol Babu"),
            CreateIssue(project, sprint, doneCol, 65, "Attachment upload", IssuePriority.Medium, IssueStatus.Done, null, "AL", "Alex L.")
        };

        var labelBug = new Label { Project = project, Name = "bug", Color = "#ef4444" };
        var labelFrontend = new Label { Project = project, Name = "frontend", Color = "#6366f1" };
        var labelBackend = new Label { Project = project, Name = "backend", Color = "#10b981" };
        var labelUrgent = new Label { Project = project, Name = "urgent", Color = "#f59e0b" };

        issues[0].Comments.Add(new Comment
        {
            AuthorMemberId = 2,
            AuthorName = "Jane Doe",
            Body = "Reproduced on EU staging — timeout after 30s on checkout."
        });
        issues[2].Comments.Add(new Comment
        {
            AuthorMemberId = 3,
            AuthorName = "Mike Chen",
            Body = "Sortable.js integration looks good, testing cross-column moves."
        });

        db.Labels.AddRange(labelBug, labelFrontend, labelBackend, labelUrgent);

        foreach (var day in Enumerable.Range(0, 7))
        {
            db.BurndownSnapshots.Add(new BurndownSnapshot
            {
                Sprint = sprint,
                SnapshotDate = sprint.StartDate.Date.AddDays(day),
                RemainingPoints = 20 - (day * 2) - (day > 4 ? 2 : 0),
                IdealPoints = 20 - (int)Math.Round(day * (20 / 14.0))
            });
        }

        db.Notifications.AddRange(
            new Notification { RecipientMemberId = 1, Type = NotificationType.Commented, Message = "Jane commented on NIM-38", LinkUrl = "/app/issues/NIM-38", IsRead = false },
            new Notification { RecipientMemberId = 1, Type = NotificationType.SprintStarted, Message = "Sprint 14 started", IsRead = false },
            new Notification { RecipientMemberId = 1, Type = NotificationType.IssueMoved, Message = "Mike moved NIM-41 to In Progress", IsRead = false }
        );

        db.ActivityLogs.AddRange(
            new ActivityLog { ActorMemberId = 2, ActorName = "Jane", Action = "commented on", Detail = "NIM-38" },
            new ActivityLog { ActorMemberId = 3, ActorName = "Mike", Action = "moved", Detail = "NIM-41 to In Progress" },
            new ActivityLog { ActorMemberId = 1, ActorName = "Anju", Action = "started", Detail = "Sprint 14" }
        );

        db.Boards.Add(new Board { Project = project, Name = "Design Board" });
        db.Boards.Add(new Board { Project = project, Name = "QA Board" });

        db.ProjectMembers.AddRange(
            new ProjectMember { Project = project, MemberId = 1, DisplayName = "Anjumol Babu", Initials = "AB", Role = ProjectRole.Admin },
            new ProjectMember { Project = project, MemberId = 2, DisplayName = "Jane Doe", Initials = "JD", Role = ProjectRole.Member },
            new ProjectMember { Project = project, MemberId = 3, DisplayName = "Mike Chen", Initials = "MC", Role = ProjectRole.Member }
        );

        db.Workspaces.Add(workspace);
        db.Projects.Add(project);
        db.Sprints.Add(sprint);
        db.Boards.Add(board);
        db.BoardColumns.AddRange(todoCol, progressCol, doneCol);
        db.Issues.AddRange(issues);

        await db.SaveChangesAsync();

        db.IssueLabels.AddRange(
            new IssueLabel { IssueId = issues[0].Id, LabelId = labelBug.Id },
            new IssueLabel { IssueId = issues[0].Id, LabelId = labelUrgent.Id },
            new IssueLabel { IssueId = issues[2].Id, LabelId = labelFrontend.Id }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedCollaborationIfMissingAsync(NimbusBoardDbContext db)
    {
        var project = await db.Projects.FirstAsync();
        var issues = await db.Issues.Where(i => i.ProjectId == project.Id).OrderBy(i => i.Number).ToListAsync();
        if (issues.Count == 0)
        {
            return;
        }

        var labelBug = new Label { ProjectId = project.Id, Name = "bug", Color = "#ef4444" };
        var labelFrontend = new Label { ProjectId = project.Id, Name = "frontend", Color = "#6366f1" };
        var labelBackend = new Label { ProjectId = project.Id, Name = "backend", Color = "#10b981" };
        var labelUrgent = new Label { ProjectId = project.Id, Name = "urgent", Color = "#f59e0b" };
        db.Labels.AddRange(labelBug, labelFrontend, labelBackend, labelUrgent);
        await db.SaveChangesAsync();

        if (!await db.IssueLabels.AnyAsync())
        {
            db.IssueLabels.AddRange(
                new IssueLabel { IssueId = issues[0].Id, LabelId = labelBug.Id },
                new IssueLabel { IssueId = issues[0].Id, LabelId = labelUrgent.Id });
            if (issues.Count > 2)
            {
                db.IssueLabels.Add(new IssueLabel { IssueId = issues[2].Id, LabelId = labelFrontend.Id });
            }

            await db.SaveChangesAsync();
        }

        if (!await db.Comments.AnyAsync())
        {
            issues[0].Comments.Add(new Comment
            {
                AuthorMemberId = 2,
                AuthorName = "Jane Doe",
                Body = "Reproduced on EU staging — timeout after 30s on checkout."
            });
            if (issues.Count > 2)
            {
                issues[2].Comments.Add(new Comment
                {
                    AuthorMemberId = 3,
                    AuthorName = "Mike Chen",
                    Body = "Sortable.js integration looks good, testing cross-column moves."
                });
            }

            await db.SaveChangesAsync();
        }
    }

    private static Issue CreateIssue(
        Project project,
        Sprint sprint,
        BoardColumn column,
        int number,
        string title,
        IssuePriority priority,
        IssueStatus status,
        DateTime? dueDate,
        string initials,
        string assigneeName)
    {
        return new Issue
        {
            Project = project,
            Sprint = sprint,
            BoardColumn = column,
            Number = number,
            Key = $"{project.Key}-{number}",
            Title = title,
            Priority = priority,
            Status = status,
            DueDate = dueDate,
            AssigneeMemberId = 1,
            AssigneeName = assigneeName,
            AssigneeInitials = initials,
            StoryPoints = priority >= IssuePriority.High ? 5 : 3
        };
    }
}
