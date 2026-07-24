using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Sprints.Commands;
using NimbusBoard.Application.Sprints.Models;
using NimbusBoard.Application.Sprints.Queries;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Sprints.Handlers;

public class GetSprintsQueryHandler(INimbusBoardDbContext db) : IRequestHandler<GetSprintsQuery, IReadOnlyList<SprintListItemViewModel>>
{
    public async Task<IReadOnlyList<SprintListItemViewModel>> Handle(GetSprintsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Sprints.Include(s => s.Project).Include(s => s.Issues).AsQueryable();
        if (request.ProjectId.HasValue)
        {
            query = query.Where(s => s.ProjectId == request.ProjectId);
        }

        var sprints = await query.OrderByDescending(s => s.IsActive).ThenByDescending(s => s.StartDate).ToListAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;

        return sprints.Select(s => new SprintListItemViewModel
        {
            Id = s.Id,
            Name = s.Name,
            Goal = s.Goal,
            ProjectId = s.ProjectId,
            ProjectKey = s.Project.Key,
            ProjectName = s.Project.Name,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            IsActive = s.IsActive,
            TotalStoryPoints = s.TotalStoryPoints,
            CompletedStoryPoints = s.CompletedStoryPoints,
            IssueCount = s.Issues.Count,
            DaysLeft = Math.Max(0, (s.EndDate.Date - today).Days)
        }).ToList();
    }
}

public class GetSprintDetailQueryHandler(INimbusBoardDbContext db, IBurndownService burndown)
    : IRequestHandler<GetSprintDetailQuery, SprintDetailViewModel?>
{
    public async Task<SprintDetailViewModel?> Handle(GetSprintDetailQuery request, CancellationToken cancellationToken)
    {
        var sprint = await db.Sprints
            .Include(s => s.Project)
            .Include(s => s.Issues)
            .FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken);

        if (sprint is null)
        {
            return null;
        }

        if (sprint.IsActive)
        {
            await burndown.EnsureTodaySnapshotAsync(sprint.Id, cancellationToken);
        }

        var backlog = await db.Issues
            .Where(i => i.ProjectId == sprint.ProjectId && i.SprintId == null && i.Status != IssueStatus.Done)
            .OrderBy(i => i.Number)
            .ToListAsync(cancellationToken);

        return new SprintDetailViewModel
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Goal = sprint.Goal,
            ProjectId = sprint.ProjectId,
            ProjectKey = sprint.Project.Key,
            ProjectName = sprint.Project.Name,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            IsActive = sprint.IsActive,
            TotalStoryPoints = sprint.TotalStoryPoints,
            CompletedStoryPoints = sprint.CompletedStoryPoints,
            DaysLeft = Math.Max(0, (sprint.EndDate.Date - DateTime.UtcNow.Date).Days),
            Burndown = await BurndownQueryHelper.BuildAsync(db, sprint.Id, cancellationToken),
            Issues = sprint.Issues.OrderBy(i => i.Status).ThenBy(i => i.SortOrder).Select(MapIssue).ToList(),
            BacklogCandidates = backlog.Select(MapIssue).ToList()
        };
    }

    private static SprintIssueItemViewModel MapIssue(Issue issue) => new()
    {
        Id = issue.Id,
        Key = issue.Key,
        Title = issue.Title,
        Status = IssueStatusMapper.ToDisplayName(issue.Status),
        Priority = issue.Priority.ToString(),
        StoryPoints = issue.StoryPoints ?? 0,
        AssigneeInitials = issue.AssigneeInitials
    };
}

public class GetSprintCreateFormQueryHandler(INimbusBoardDbContext db)
    : IRequestHandler<GetSprintCreateFormQuery, SprintCreateFormViewModel>
{
    public async Task<SprintCreateFormViewModel> Handle(GetSprintCreateFormQuery request, CancellationToken cancellationToken)
    {
        var projects = await db.Projects.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return new SprintCreateFormViewModel
        {
            Projects = projects.Select(p => new SprintProjectOptionViewModel
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name
            }).ToList()
        };
    }
}

public class CreateSprintCommandHandler(INimbusBoardDbContext db) : IRequestHandler<CreateSprintCommand, Guid>
{
    public async Task<Guid> Handle(CreateSprintCommand request, CancellationToken cancellationToken)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new InvalidOperationException("Project not found.");

        if (request.EndDate.Date < request.StartDate.Date)
        {
            throw new InvalidOperationException("End date must be on or after start date.");
        }

        var sprint = new Sprint
        {
            ProjectId = project.Id,
            Name = request.Name.Trim(),
            Goal = string.IsNullOrWhiteSpace(request.Goal) ? null : request.Goal.Trim(),
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            IsActive = false
        };

        db.Sprints.Add(sprint);
        await db.SaveChangesAsync(cancellationToken);
        return sprint.Id;
    }
}

public class StartSprintCommandHandler(
    INimbusBoardDbContext db,
    IBurndownService burndown,
    IAppNotificationService notifications) : IRequestHandler<StartSprintCommand, Unit>
{
    public async Task<Unit> Handle(StartSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await db.Sprints
            .Include(s => s.Project)
            .Include(s => s.Issues)
            .FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken)
            ?? throw new InvalidOperationException("Sprint not found.");

        var others = await db.Sprints
            .Where(s => s.ProjectId == sprint.ProjectId && s.IsActive && s.Id != sprint.Id)
            .ToListAsync(cancellationToken);

        foreach (var other in others)
        {
            other.IsActive = false;
            other.UpdatedAt = DateTime.UtcNow;
        }

        sprint.IsActive = true;
        sprint.UpdatedAt = DateTime.UtcNow;

        await burndown.RecalculateSprintPointsAsync(sprint.Id, cancellationToken);

        // Seed ideal + actual snapshots from start through today
        var today = DateTime.UtcNow.Date;
        for (var day = sprint.StartDate.Date; day <= today && day <= sprint.EndDate.Date; day = day.AddDays(1))
        {
            await burndown.TakeSnapshotAsync(sprint.Id, day, cancellationToken);
        }

        db.ActivityLogs.Add(new ActivityLog
        {
            ProjectId = sprint.ProjectId,
            ActorMemberId = request.ActorMemberId,
            ActorName = request.ActorName,
            Action = "started",
            Detail = sprint.Name
        });

        var members = await db.ProjectMembers
            .Where(m => m.ProjectId == sprint.ProjectId)
            .ToListAsync(cancellationToken);

        foreach (var member in members)
        {
            await notifications.PublishAsync(
                member.MemberId,
                NotificationType.SprintStarted,
                $"{sprint.Name} started",
                $"/app/sprints/{sprint.Id}",
                cancellationToken: cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class CompleteSprintCommandHandler(INimbusBoardDbContext db) : IRequestHandler<CompleteSprintCommand, Unit>
{
    public async Task<Unit> Handle(CompleteSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await db.Sprints.FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken)
            ?? throw new InvalidOperationException("Sprint not found.");

        sprint.IsActive = false;
        sprint.UpdatedAt = DateTime.UtcNow;

        db.ActivityLogs.Add(new ActivityLog
        {
            ProjectId = sprint.ProjectId,
            ActorMemberId = request.ActorMemberId,
            ActorName = request.ActorName,
            Action = "completed",
            Detail = sprint.Name
        });

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class AssignIssueToSprintCommandHandler(INimbusBoardDbContext db, IBurndownService burndown)
    : IRequestHandler<AssignIssueToSprintCommand, Unit>
{
    public async Task<Unit> Handle(AssignIssueToSprintCommand request, CancellationToken cancellationToken)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(i => i.Id == request.IssueId, cancellationToken)
            ?? throw new InvalidOperationException("Issue not found.");

        var previousSprintId = issue.SprintId;
        issue.SprintId = request.SprintId;
        issue.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        if (previousSprintId.HasValue)
        {
            await burndown.RecalculateSprintPointsAsync(previousSprintId.Value, cancellationToken);
            await burndown.TakeSnapshotAsync(previousSprintId.Value, cancellationToken: cancellationToken);
        }

        if (request.SprintId.HasValue)
        {
            await burndown.RecalculateSprintPointsAsync(request.SprintId.Value, cancellationToken);
            await burndown.TakeSnapshotAsync(request.SprintId.Value, cancellationToken: cancellationToken);
        }

        return Unit.Value;
    }
}
