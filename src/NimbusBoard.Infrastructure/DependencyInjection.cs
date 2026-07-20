using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NimbusBoard.Application;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;
using NimbusBoard.Infrastructure.Persistence;

namespace NimbusBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNimbusBoardInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NimbusBoardDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<INimbusBoardDbContext>(sp => sp.GetRequiredService<NimbusBoardDbContext>());
        services.AddNimbusBoardApplication();

        return services;
    }

    public static async Task EnsureNimbusBoardDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NimbusBoardDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.Projects.AnyAsync())
        {
            await SeedAsync(db);
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
            CreateIssue(project, sprint, progressCol, 87, "Board drag-and-drop", IssuePriority.High, IssueStatus.InProgress, DateTime.UtcNow.Date.AddDays(2), "JS", "Jordan Silva"),
            CreateIssue(project, sprint, progressCol, 76, "Sprint burndown chart", IssuePriority.High, IssueStatus.InProgress, DateTime.UtcNow.Date.AddDays(3), "MK", "Maya K."),
            CreateIssue(project, sprint, todoCol, 71, "Fix login redirect", IssuePriority.Medium, IssueStatus.ToDo, DateTime.UtcNow.Date.AddDays(4), "JS", "Jordan Silva"),
            CreateIssue(project, sprint, doneCol, 65, "Attachment upload", IssuePriority.Medium, IssueStatus.Done, null, "AL", "Alex L.")
        };

        foreach (var day in Enumerable.Range(0, 7))
        {
            db.BurndownSnapshotsSet.Add(new BurndownSnapshot
            {
                Sprint = sprint,
                SnapshotDate = sprint.StartDate.Date.AddDays(day),
                RemainingPoints = 20 - (day * 2) - (day > 4 ? 2 : 0),
                IdealPoints = 20 - (int)Math.Round(day * (20 / 14.0))
            });
        }

        db.NotificationsSet.AddRange(
            new Notification { RecipientMemberId = 1, Type = NotificationType.Commented, Message = "Jane commented on NIM-38", LinkUrl = "/app/issues/NIM-38", IsRead = false },
            new Notification { RecipientMemberId = 1, Type = NotificationType.SprintStarted, Message = "Sprint 14 started", IsRead = false },
            new Notification { RecipientMemberId = 1, Type = NotificationType.IssueMoved, Message = "Mike moved NIM-41 to In Progress", IsRead = false }
        );

        db.ActivityLogsSet.AddRange(
            new ActivityLog { ActorMemberId = 2, ActorName = "Jane", Action = "commented on", Detail = "NIM-38" },
            new ActivityLog { ActorMemberId = 3, ActorName = "Mike", Action = "moved", Detail = "NIM-41 to In Progress" },
            new ActivityLog { ActorMemberId = 1, ActorName = "Jordan", Action = "started", Detail = "Sprint 14" }
        );

        db.BoardsSet.Add(new Board { Project = project, Name = "Design Board" });
        db.BoardsSet.Add(new Board { Project = project, Name = "QA Board" });

        db.WorkspacesSet.Add(workspace);
        db.ProjectsSet.Add(project);
        db.SprintsSet.Add(sprint);
        db.BoardsSet.Add(board);
        db.BoardColumnsSet.AddRange(todoCol, progressCol, doneCol);
        db.IssuesSet.AddRange(issues);

        await db.SaveChangesAsync();
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
