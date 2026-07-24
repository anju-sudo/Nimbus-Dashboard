using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Projects.Commands;
using NimbusBoard.Application.Projects.Models;
using NimbusBoard.Application.Projects.Queries;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Projects.Handlers;

public class GetProjectsQueryHandler(INimbusBoardDbContext db) : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectListItemViewModel>>
{
    public async Task<IReadOnlyList<ProjectListItemViewModel>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        return await db.Projects
            .Select(p => new ProjectListItemViewModel
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Description = p.Description,
                OpenIssues = p.Issues.Count(i => i.Status != IssueStatus.Done),
                MemberCount = p.Members.Count
            })
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}

public class GetProjectByKeyQueryHandler(INimbusBoardDbContext db) : IRequestHandler<GetProjectByKeyQuery, ProjectDetailViewModel?>
{
    public async Task<ProjectDetailViewModel?> Handle(GetProjectByKeyQuery request, CancellationToken cancellationToken)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .Include(p => p.Boards)
            .Include(p => p.Issues)
            .FirstOrDefaultAsync(p => p.Key == request.Key, cancellationToken);

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
            Members = project.Members.Select(m => new ProjectMemberViewModel
            {
                DisplayName = m.DisplayName,
                Initials = m.Initials,
                Role = m.Role.ToString()
            }).ToList(),
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

public class CreateProjectCommandHandler(INimbusBoardDbContext db) : IRequestHandler<CreateProjectCommand, Guid>
{
    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            WorkspaceId = request.WorkspaceId,
            Key = request.Key.ToUpperInvariant(),
            Name = request.Name,
            Description = request.Description
        };

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
        return project.Id;
    }
}

public class AddProjectMemberCommandHandler(INimbusBoardDbContext db) : IRequestHandler<AddProjectMemberCommand, Guid>
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
