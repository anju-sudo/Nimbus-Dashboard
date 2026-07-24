using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Boards.Models;
using NimbusBoard.Application.Boards.Queries;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Boards.Handlers;

public class GetBoardsQueryHandler(INimbusBoardDbContext db)
    : IRequestHandler<GetBoardsQuery, IReadOnlyList<BoardListItemViewModel>>
{
    public async Task<IReadOnlyList<BoardListItemViewModel>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        return await db.Boards
            .Include(b => b.Project)
            .Include(b => b.Columns)
            .ThenInclude(c => c.Issues)
            .Select(b => new BoardListItemViewModel
            {
                Id = b.Id,
                Name = b.Name,
                ProjectKey = b.Project.Key,
                ProjectName = b.Project.Name,
                IssueCount = b.Columns.SelectMany(c => c.Issues).Count()
            })
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }
}

public class GetBoardQueryHandler(INimbusBoardDbContext db)
    : IRequestHandler<GetBoardQuery, BoardViewModel?>
{
    public async Task<BoardViewModel?> Handle(GetBoardQuery request, CancellationToken cancellationToken)
    {
        var board = await db.Boards
            .Include(b => b.Project)
            .Include(b => b.Columns)
            .ThenInclude(c => c.Issues)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken);

        if (board is null)
        {
            return null;
        }

        return new BoardViewModel
        {
            Id = board.Id,
            Name = board.Name,
            ProjectId = board.ProjectId,
            ProjectKey = board.Project.Key,
            ProjectName = board.Project.Name,
            Columns = board.Columns
                .OrderBy(c => c.SortOrder)
                .Select(c => new BoardColumnViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    SortOrder = c.SortOrder,
                    Issues = c.Issues
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new BoardIssueCardViewModel
                        {
                            Id = i.Id,
                            Key = i.Key,
                            Title = i.Title,
                            Priority = i.Priority.ToString(),
                            PriorityClass = GetPriorityClass(i.Priority),
                            AssigneeInitials = i.AssigneeInitials,
                            AssigneeClass = "bg-violet-100 text-violet-700"
                        }).ToList()
                }).ToList()
        };
    }

    private static string GetPriorityClass(IssuePriority priority) => priority switch
    {
        IssuePriority.Highest => "bg-red-100 text-red-700",
        IssuePriority.High => "bg-amber-100 text-amber-700",
        IssuePriority.Medium => "bg-yellow-100 text-yellow-700",
        _ => "bg-slate-100 text-slate-600"
    };
}
