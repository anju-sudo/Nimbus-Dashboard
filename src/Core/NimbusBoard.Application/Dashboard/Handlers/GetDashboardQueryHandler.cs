using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Dashboard.Models;
using NimbusBoard.Application.Dashboard.Queries;
using NimbusBoard.Application.Sprints;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Dashboard.Handlers;

public class GetDashboardQueryHandler(INimbusBoardDbContext db, IBurndownService burndown)
    : IRequestHandler<GetDashboardQuery, DashboardViewModel>
{
    public async Task<DashboardViewModel> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var issues = await db.Issues
            .Include(i => i.Sprint)
            .Include(i => i.BoardColumn)
            .ToListAsync(cancellationToken);

        var activeSprint = await db.Sprints
            .Include(s => s.Issues)
            .ThenInclude(i => i.BoardColumn)
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        if (activeSprint is not null)
        {
            await burndown.EnsureTodaySnapshotAsync(activeSprint.Id, cancellationToken);
        }

        var workspace = await db.Workspaces.FirstOrDefaultAsync(cancellationToken);
        var defaultProject = await db.Projects.OrderBy(p => p.Name).FirstOrDefaultAsync(cancellationToken);
        var boards = await db.Boards.Take(3).ToListAsync(cancellationToken);
        var activity = await db.ActivityLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var unread = await db.Notifications.CountAsync(n => !n.IsRead && n.RecipientMemberId == 1, cancellationToken);

        var openIssues = issues.Count(i => i.Status is not IssueStatus.Done);
        var inProgress = issues.Count(i => i.Status == IssueStatus.InProgress);
        var doneThisSprint = activeSprint?.Issues.Count(i => i.Status == IssueStatus.Done) ?? 0;
        var overdue = issues.Count(i => i.DueDate < today && i.Status != IssueStatus.Done);
        var urgent = issues.Count(i => i.Priority >= IssuePriority.High && i.Status != IssueStatus.Done);

        var urgentTasks = issues
            .Where(i => i.Priority >= IssuePriority.High && i.Status != IssueStatus.Done)
            .OrderBy(i => i.DueDate ?? DateTime.MaxValue)
            .Take(3)
            .Select(MapUrgentTask)
            .ToList();

        var burndownChart = await BurndownQueryHelper.BuildAsync(db, activeSprint?.Id, cancellationToken);

        return new DashboardViewModel
        {
            UserName = "Anju",
            UserInitials = "AB",
            WorkspaceName = workspace?.Name ?? "Acme",
            Today = today,
            UnreadNotifications = unread,
            Stats = new DashboardStatsViewModel
            {
                OpenIssues = openIssues,
                InProgress = inProgress,
                DoneThisSprint = doneThisSprint,
                Overdue = overdue,
                Urgent = urgent
            },
            UrgentTasks = urgentTasks,
            ActiveSprint = activeSprint is null ? null : MapSprintPreview(activeSprint),
            Burndown = burndownChart,
            RecentActivity = activity.Select(a => new ActivityItemViewModel
            {
                Message = $"{a.ActorName} {a.Action}" + (a.Detail is null ? "" : $" {a.Detail}"),
                TimeAgo = ToTimeAgo(a.CreatedAt)
            }).ToList(),
            BoardLinks = boards.Select(b => new BoardQuickLinkViewModel
            {
                Id = b.Id,
                Name = b.Name
            }).ToList(),
            DefaultProjectId = defaultProject?.Id
        };
    }

    private static UrgentTaskViewModel MapUrgentTask(Domain.Entities.Issue issue)
    {
        var isHighest = issue.Priority == IssuePriority.Highest;
        return new UrgentTaskViewModel
        {
            Key = issue.Key,
            Title = issue.Title,
            Priority = isHighest ? "Highest" : "High",
            PriorityClass = isHighest ? "bg-red-100 text-red-700" : "bg-amber-100 text-amber-700",
            StatusDotClass = isHighest ? "bg-red-500" : "bg-orange-400",
            DueLabel = FormatDueLabel(issue.DueDate),
            AssigneeInitials = issue.AssigneeInitials ?? "??",
            AssigneeClass = "bg-violet-100 text-violet-700"
        };
    }

    private static SprintPreviewViewModel MapSprintPreview(Domain.Entities.Sprint sprint)
    {
        var daysLeft = Math.Max(0, (sprint.EndDate.Date - DateTime.UtcNow.Date).Days);
        var columns = new[]
        {
            IssueStatus.ToDo,
            IssueStatus.InProgress,
            IssueStatus.Done
        };

        return new SprintPreviewViewModel
        {
            Name = sprint.Name,
            Goal = sprint.Goal ?? string.Empty,
            DaysLeft = daysLeft,
            Columns = columns.Select(status =>
            {
                var columnIssues = sprint.Issues.Where(i => i.Status == status).ToList();
                return new SprintColumnPreviewViewModel
                {
                    Name = status switch
                    {
                        IssueStatus.ToDo => "TO DO",
                        IssueStatus.InProgress => "IN PROGRESS",
                        IssueStatus.Done => "DONE",
                        _ => status.ToString().ToUpperInvariant()
                    },
                    Count = columnIssues.Count,
                    Cards = columnIssues
                        .OrderBy(i => i.SortOrder)
                        .Take(1)
                        .Select(i => new SprintCardPreviewViewModel
                        {
                            Title = i.Title,
                            IsHighlighted = status == IssueStatus.InProgress
                        })
                        .ToList()
                };
            }).ToList()
        };
    }

    private static string FormatDueLabel(DateTime? dueDate)
    {
        if (dueDate is null)
        {
            return "No due date";
        }

        var days = (dueDate.Value.Date - DateTime.UtcNow.Date).Days;
        return days switch
        {
            < 0 => $"Overdue {Math.Abs(days)}d",
            0 => "Due today",
            1 => "Due tomorrow",
            _ => $"Due in {days}d"
        };
    }

    private static string ToTimeAgo(DateTime createdAt)
    {
        var span = DateTime.UtcNow - createdAt;
        if (span.TotalMinutes < 60)
        {
            return $"{Math.Max(1, (int)span.TotalMinutes)}m ago";
        }

        if (span.TotalHours < 24)
        {
            return $"{(int)span.TotalHours}h ago";
        }

        return $"{(int)span.TotalDays}d ago";
    }
}
