using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Utils;
using NimbusBoard.Application.Common.Abstractions;
using NimbusBoard.Application.Projects;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Projects;

public sealed class GetProjectsQueryHandler(INimbusBoardDbContext db) : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectListItemViewModel>>
{
    private static readonly string[] Accents =
    [
        "bg-indigo-500",
        "bg-violet-500",
        "bg-sky-500",
        "bg-emerald-500",
        "bg-amber-500",
        "bg-rose-500"
    ];

    public async Task<IReadOnlyList<ProjectListItemViewModel>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Projects
            .Select(p => new ProjectListItemViewModel
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Description = p.Description,
                OpenIssues = p.Issues.Count(i => i.Status != IssueStatus.Done),
                DoneIssues = p.Issues.Count(i => i.Status == IssueStatus.Done),
                BoardCount = p.Boards.Count,
                MemberCount = p.Members.Count
            })
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.AccentClass = Accents[Math.Abs(item.Key.GetHashCode()) % Accents.Length];
        }

        return items;
    }
}

public sealed class GetProjectByKeyQueryHandler(INimbusBoardDbContext db) : IRequestHandler<GetProjectByKeyQuery, ProjectDetailViewModel?>
{
    public async Task<ProjectDetailViewModel?> Handle(GetProjectByKeyQuery request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim();
        var project = await db.Projects
            .Include(p => p.Members)
            .Include(p => p.Boards)
            .Include(p => p.Issues)
            .FirstOrDefaultAsync(p => p.Key.ToUpper() == key.ToUpper(), cancellationToken);

        if (project is null)
        {
            return null;
        }

        return new ProjectDetailViewModel
        {
            Id = project.Id,
            Key = project.Key,
            Name = project.Name,
            Description = project.Description,
            Members = project.Members.Select(MemberAvatarHelper.ToViewModel).ToList(),
            Boards = project.Boards.Select(b => new BoardSummaryViewModel
            {
                Id = b.Id,
                Name = b.Name
            }).ToList(),
            RecentIssues = project.Issues
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .Select(i => new IssueSummaryViewModel
                {
                    Id = i.Id,
                    Key = i.Key,
                    Title = i.Title,
                    Status = i.Status.ToString(),
                    Priority = i.Priority.ToString(),
                    AssigneeInitials = i.AssigneeInitials
                }).ToList()
        };
    }
}

public sealed class CreateProjectCommandHandler(INimbusBoardDbContext db) : IRequestHandler<CreateProjectCommand, CreateProjectResult>
{
    public async Task<CreateProjectResult> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = request.WorkspaceId;
        if (workspaceId == Guid.Empty)
        {
            var workspace = await db.Workspaces.FirstOrDefaultAsync(cancellationToken);
            if (workspace is null)
            {
                workspace = new Workspace { Name = "Acme", Slug = "acme" };
                db.Workspaces.Add(workspace);
                await db.SaveChangesAsync(cancellationToken);
            }

            workspaceId = workspace.Id;
        }

        var project = new Project
        {
            WorkspaceId = workspaceId,
            Key = request.Key.ToUpperInvariant(),
            Name = request.Name,
            Description = request.Description
        };

        var key = project.Key.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length is < 2 or > 10 || key.Any(c => !char.IsLetterOrDigit(c)))
        {
            throw new InvalidOperationException("Project key must be 2–10 letters or numbers (e.g. NIM, WEB, MOBILE).");
        }

        if (await db.Projects.AnyAsync(p => p.Key == key, cancellationToken))
        {
            throw new InvalidOperationException($"Project key '{key}' is already in use.");
        }

        project.Key = key;

        var board = new Board { Project = project, Name = $"{project.Name} Board" };
        board.Columns = new List<BoardColumn>
        {
            new() { Board = board, Name = "To Do", SortOrder = 1 },
            new() { Board = board, Name = "In Progress", SortOrder = 2 },
            new() { Board = board, Name = "Done", SortOrder = 3 }
        };

        db.Projects.Add(project);
        db.Boards.Add(board);
        await db.SaveChangesAsync(cancellationToken);
        return new CreateProjectResult(project.Id, project.Key);
    }
}

public sealed class GetDefaultWorkspaceIdQueryHandler(INimbusBoardDbContext db) : IRequestHandler<GetDefaultWorkspaceIdQuery, Guid>
{
    public async Task<Guid> Handle(GetDefaultWorkspaceIdQuery request, CancellationToken cancellationToken)
    {
        var workspace = await db.Workspaces.FirstOrDefaultAsync(cancellationToken);
        if (workspace is not null)
        {
            return workspace.Id;
        }

        workspace = new Workspace { Name = "Acme", Slug = "acme" };
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(cancellationToken);
        return workspace.Id;
    }
}

public sealed class AddProjectMemberCommandHandler(INimbusBoardDbContext db) : IRequestHandler<AddProjectMemberCommand, Guid>
{
    public async Task<Guid> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ProjectRole>(request.Role, true, out var role))
        {
            role = ProjectRole.Member;
        }

        var member = new ProjectMember
        {
            ProjectId = request.ProjectId,
            DisplayName = request.DisplayName,
            Initials = request.Initials,
            Role = role,
            MemberId = Random.Shared.Next(100, 999)
        };

        db.ProjectMembers.Add(member);
        await db.SaveChangesAsync(cancellationToken);
        return member.Id;
    }
}

public sealed class DeleteProjectCommandHandler(INimbusBoardDbContext db) : IRequestHandler<DeleteProjectCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await db.Projects
            .Include(p => p.Boards)
            .ThenInclude(b => b.Columns)
            .Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new InvalidOperationException("Project not found.");

        var issueIds = await db.Issues
            .Where(i => i.ProjectId == project.Id)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        if (issueIds.Count > 0)
        {
            var issueLabels = await db.IssueLabels
                .Where(il => issueIds.Contains(il.IssueId))
                .ToListAsync(cancellationToken);
            db.IssueLabels.RemoveRange(issueLabels);

            var comments = await db.Comments
                .Where(c => issueIds.Contains(c.IssueId))
                .ToListAsync(cancellationToken);
            db.Comments.RemoveRange(comments);

            var attachments = await db.Attachments
                .Where(a => issueIds.Contains(a.IssueId))
                .ToListAsync(cancellationToken);
            db.Attachments.RemoveRange(attachments);

            var activity = await db.ActivityLogs
                .Where(a => a.IssueId != null && issueIds.Contains(a.IssueId.Value))
                .ToListAsync(cancellationToken);
            db.ActivityLogs.RemoveRange(activity);

            var notifications = await db.Notifications
                .Where(n => n.IssueId != null && issueIds.Contains(n.IssueId.Value))
                .ToListAsync(cancellationToken);
            db.Notifications.RemoveRange(notifications);

            var issues = await db.Issues
                .Where(i => i.ProjectId == project.Id)
                .ToListAsync(cancellationToken);
            db.Issues.RemoveRange(issues);
        }

        var sprintIds = project.Sprints.Select(s => s.Id).ToList();
        if (sprintIds.Count > 0)
        {
            var snapshots = await db.BurndownSnapshots
                .Where(s => sprintIds.Contains(s.SprintId))
                .ToListAsync(cancellationToken);
            db.BurndownSnapshots.RemoveRange(snapshots);
            db.Sprints.RemoveRange(project.Sprints);
        }

        foreach (var board in project.Boards)
        {
            db.BoardColumns.RemoveRange(board.Columns);
        }

        db.Boards.RemoveRange(project.Boards);

        var members = await db.ProjectMembers
            .Where(m => m.ProjectId == project.Id)
            .ToListAsync(cancellationToken);
        db.ProjectMembers.RemoveRange(members);

        var labels = await db.Labels
            .Where(l => l.ProjectId == project.Id)
            .ToListAsync(cancellationToken);
        db.Labels.RemoveRange(labels);

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
