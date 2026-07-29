using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Collaboration.Handlers;
using NimbusBoard.Application.Collaboration.Models;
using NimbusBoard.Application.Common;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Issues.Commands;
using NimbusBoard.Application.Issues.Models;
using NimbusBoard.Application.Issues.Queries;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Issues.Handlers;

public sealed class GetIssueByKeyQueryHandler(INimbusBoardDbContext db, IAttachmentStorage storage) : IRequestHandler<GetIssueByKeyQuery, IssueDetailViewModel?>
{
    public async Task<IssueDetailViewModel?> Handle(GetIssueByKeyQuery request, CancellationToken cancellationToken)
    {
        var issue = await db.Issues
            .Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.Key == request.Key, cancellationToken);

        if (issue is null)
        {
            return null;
        }

        var members = await db.ProjectMembers
            .Where(m => m.ProjectId == issue.ProjectId)
            .OrderBy(m => m.DisplayName)
            .ToListAsync(cancellationToken);

        var vm = MapDetail(issue);
        vm.AssignableMembers = members.Select(MemberAvatarHelper.ToViewModel).ToList();
        vm.CurrentMemberId = 1;
        vm.Comments = await CollaborationQueryHelper.GetCommentsAsync(db, issue.Id, cancellationToken);
        vm.Attachments = await CollaborationQueryHelper.GetAttachmentsAsync(db, storage, issue.Id, cancellationToken);
        vm.Labels = await CollaborationQueryHelper.GetProjectLabelsAsync(db, issue.ProjectId, issue.Id, cancellationToken);
        vm.Activity = await CollaborationQueryHelper.GetActivityAsync(db, issue.Id, cancellationToken);
        return vm;
    }

    internal static IssueDetailViewModel MapDetail(Issue issue) => new()
    {
        Id = issue.Id,
        Key = issue.Key,
        Title = issue.Title,
        Description = issue.Description,
        Type = issue.Type.ToString(),
        Priority = issue.Priority.ToString(),
        Status = IssueStatusMapper.ToDisplayName(issue.Status),
        StoryPoints = issue.StoryPoints,
        DueDate = issue.DueDate,
        AssigneeMemberId = issue.AssigneeMemberId,
        AssigneeName = issue.AssigneeName,
        AssigneeInitials = issue.AssigneeInitials,
        AssigneeAvatarClass = MemberAvatarHelper.ClassFor(issue.AssigneeMemberId ?? 0),
        ProjectKey = issue.Project.Key,
        ProjectName = issue.Project.Name,
        BoardColumnId = issue.BoardColumnId,
        ProjectId = issue.ProjectId
    };
}

public sealed class GetMyWorkQueryHandler(INimbusBoardDbContext db) : IRequestHandler<GetMyWorkQuery, IReadOnlyList<IssueListItemViewModel>>
{
    public async Task<IReadOnlyList<IssueListItemViewModel>> Handle(GetMyWorkQuery request, CancellationToken cancellationToken)
    {
        return await db.Issues
            .Include(i => i.Project)
            .Where(i => i.AssigneeMemberId == request.MemberId && i.Status != IssueStatus.Done)
            .OrderBy(i => i.DueDate ?? DateTime.MaxValue)
            .Select(i => new IssueListItemViewModel
            {
                Id = i.Id,
                Key = i.Key,
                Title = i.Title,
                Status = i.Status.ToString(),
                Priority = i.Priority.ToString(),
                PriorityClass = i.Priority >= IssuePriority.High ? "bg-amber-100 text-amber-700" : "bg-slate-100 text-slate-600",
                AssigneeInitials = i.AssigneeInitials,
                ProjectKey = i.Project.Key
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetIssueCreateFormQueryHandler(INimbusBoardDbContext db) : IRequestHandler<GetIssueCreateFormQuery, CreateIssueFormModel?>
{
    public async Task<CreateIssueFormModel?> Handle(GetIssueCreateFormQuery request, CancellationToken cancellationToken)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var members = await db.ProjectMembers
            .Where(m => m.ProjectId == request.ProjectId)
            .OrderBy(m => m.DisplayName)
            .ToListAsync(cancellationToken);

        return new CreateIssueFormModel
        {
            ProjectId = request.ProjectId,
            ProjectKey = project.Key,
            BoardColumnId = request.BoardColumnId,
            AssignableMembers = members.Select(MemberAvatarHelper.ToViewModel).ToList(),
            AssigneeMemberId = 1
        };
    }
}

public sealed class CreateIssueCommandHandler(
    INimbusBoardDbContext db,
    IIssueKeyFactory keyFactory,
    IBurndownService burndown,
    IAppNotificationService notifications) : IRequestHandler<CreateIssueCommand, CreateIssueResult>
{
    public async Task<CreateIssueResult> Handle(CreateIssueCommand request, CancellationToken cancellationToken)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new InvalidOperationException("Project not found.");

        BoardColumn? column = null;
        if (request.BoardColumnId.HasValue)
        {
            column = await db.BoardColumns.FirstOrDefaultAsync(c => c.Id == request.BoardColumnId, cancellationToken);
        }

        if (!Enum.TryParse<IssueType>(request.Type, true, out var type))
        {
            type = IssueType.Task;
        }

        if (!Enum.TryParse<IssuePriority>(request.Priority, true, out var priority))
        {
            priority = IssuePriority.Medium;
        }

        var (number, key) = await keyFactory.CreateNextKeyAsync(project, cancellationToken);
        var status = column is null ? IssueStatus.ToDo : IssueStatusMapper.FromColumnName(column.Name);

        string? assigneeName = null;
        string? assigneeInitials = null;
        int? assigneeMemberId = request.AssigneeMemberId;
        if (assigneeMemberId.HasValue)
        {
            var member = await db.ProjectMembers.FirstOrDefaultAsync(
                m => m.ProjectId == project.Id && m.MemberId == assigneeMemberId.Value,
                cancellationToken);
            if (member is null)
            {
                throw new InvalidOperationException("Assignee is not a member of this project.");
            }

            assigneeName = member.DisplayName;
            assigneeInitials = member.Initials;
        }

        var issue = new Issue
        {
            ProjectId = project.Id,
            BoardColumnId = column?.Id,
            SprintId = request.SprintId,
            Number = number,
            Key = key,
            Title = request.Title,
            Description = request.Description,
            Type = type,
            Priority = priority,
            Status = status,
            StoryPoints = request.StoryPoints,
            DueDate = request.DueDate,
            AssigneeMemberId = assigneeMemberId,
            AssigneeName = assigneeName,
            AssigneeInitials = assigneeInitials
        };

        db.Issues.Add(issue);
        db.ActivityLogs.Add(new ActivityLog
        {
            Issue = issue,
            ProjectId = project.Id,
            ActorMemberId = 1,
            ActorName = "Anjumol Babu",
            Action = "created",
            Detail = key
        });

        await db.SaveChangesAsync(cancellationToken);

        if (assigneeMemberId is int assigneeId)
        {
            await notifications.PublishAsync(
                assigneeId,
                NotificationType.Assigned,
                $"You were assigned to {key}",
                $"/app/issues/{key}",
                issue.Id,
                cancellationToken: cancellationToken);
        }

        if (request.SprintId.HasValue)
        {
            await burndown.RecalculateSprintPointsAsync(request.SprintId.Value, cancellationToken);
            await burndown.TakeSnapshotAsync(request.SprintId.Value, cancellationToken: cancellationToken);
        }

        return new CreateIssueResult(key, column?.BoardId);
    }
}

public sealed class UpdateIssueCommandHandler(
    INimbusBoardDbContext db,
    IBurndownService burndown,
    IAppNotificationService notifications) : IRequestHandler<UpdateIssueCommand, Unit>
{
    public async Task<Unit> Handle(UpdateIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(i => i.Key == request.Key, cancellationToken)
            ?? throw new InvalidOperationException("Issue not found.");

        if (!Enum.TryParse<IssueType>(request.Type, true, out var type))
        {
            type = issue.Type;
        }

        if (!Enum.TryParse<IssuePriority>(request.Priority, true, out var priority))
        {
            priority = issue.Priority;
        }

        var previousAssigneeId = issue.AssigneeMemberId;
        var previousPoints = issue.StoryPoints;

        issue.Title = request.Title;
        issue.Description = request.Description;
        issue.Type = type;
        issue.Priority = priority;
        issue.StoryPoints = request.StoryPoints;
        issue.DueDate = request.DueDate;
        issue.UpdatedAt = DateTime.UtcNow;

        if (request.AssigneeMemberId is null)
        {
            issue.AssigneeMemberId = null;
            issue.AssigneeName = null;
            issue.AssigneeInitials = null;
        }
        else
        {
            var member = await db.ProjectMembers.FirstOrDefaultAsync(
                m => m.ProjectId == issue.ProjectId && m.MemberId == request.AssigneeMemberId.Value,
                cancellationToken)
                ?? throw new InvalidOperationException("Assignee is not a member of this project.");

            issue.AssigneeMemberId = member.MemberId;
            issue.AssigneeName = member.DisplayName;
            issue.AssigneeInitials = member.Initials;
        }

        db.ActivityLogs.Add(new ActivityLog
        {
            IssueId = issue.Id,
            ProjectId = issue.ProjectId,
            ActorMemberId = 1,
            ActorName = "Anjumol Babu",
            Action = previousAssigneeId != issue.AssigneeMemberId ? "reassigned" : "updated",
            Detail = issue.AssigneeName is null
                ? $"{issue.Key} (unassigned)"
                : $"{issue.Key} → {issue.AssigneeName}"
        });

        await db.SaveChangesAsync(cancellationToken);

        if (previousAssigneeId != issue.AssigneeMemberId && issue.AssigneeMemberId is int assigneeId)
        {
            await notifications.PublishAsync(
                assigneeId,
                NotificationType.Assigned,
                $"You were assigned to {issue.Key}",
                $"/app/issues/{issue.Key}",
                issue.Id,
                cancellationToken: cancellationToken);
        }

        if (issue.SprintId.HasValue && previousPoints != request.StoryPoints)
        {
            await burndown.RecalculateSprintPointsAsync(issue.SprintId.Value, cancellationToken);
            await burndown.TakeSnapshotAsync(issue.SprintId.Value, cancellationToken: cancellationToken);
        }

        return Unit.Value;
    }
}

public sealed class MoveIssueCommandHandler(
    INimbusBoardDbContext db,
    IBurndownService burndown,
    IAppNotificationService notifications) : IRequestHandler<MoveIssueCommand, Unit>
{
    public async Task<Unit> Handle(MoveIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(i => i.Id == request.IssueId, cancellationToken)
            ?? throw new InvalidOperationException("Issue not found.");

        var column = await db.BoardColumns.FirstOrDefaultAsync(c => c.Id == request.BoardColumnId, cancellationToken)
            ?? throw new InvalidOperationException("Column not found.");

        var newStatus = IssueStatusMapper.FromColumnName(column.Name);
        IssueStatusStateMachine.EnsureCanTransition(issue.Status, newStatus);

        var actorId = issue.AssigneeMemberId ?? 1;
        var actorName = issue.AssigneeName ?? "User";

        issue.BoardColumnId = column.Id;
        issue.Status = newStatus;
        issue.SortOrder = request.SortOrder;
        issue.UpdatedAt = DateTime.UtcNow;

        db.ActivityLogs.Add(new ActivityLog
        {
            IssueId = issue.Id,
            ProjectId = issue.ProjectId,
            ActorMemberId = actorId,
            ActorName = actorName,
            Action = "moved",
            Detail = $"{issue.Key} to {column.Name}"
        });

        await db.SaveChangesAsync(cancellationToken);

        if (issue.AssigneeMemberId is int recipient && recipient != actorId)
        {
            await notifications.PublishAsync(
                recipient,
                NotificationType.IssueMoved,
                $"{actorName} moved {issue.Key} to {column.Name}",
                $"/app/issues/{issue.Key}",
                issue.Id,
                cancellationToken: cancellationToken);
        }
        else
        {
            // Notify project admins / self for visibility in the feed
            await notifications.PublishAsync(
                1,
                NotificationType.IssueMoved,
                $"{actorName} moved {issue.Key} to {column.Name}",
                $"/app/issues/{issue.Key}",
                issue.Id,
                cancellationToken: cancellationToken);
        }

        if (issue.SprintId.HasValue)
        {
            await burndown.RecalculateSprintPointsAsync(issue.SprintId.Value, cancellationToken);
            await burndown.TakeSnapshotAsync(issue.SprintId.Value, cancellationToken: cancellationToken);
        }

        return Unit.Value;
    }
}
