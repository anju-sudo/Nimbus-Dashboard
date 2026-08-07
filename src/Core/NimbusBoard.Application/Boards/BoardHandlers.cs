using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Boards;
using NimbusBoard.Application.Common.Abstractions;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Boards;

public sealed class GetBoardsQueryHandler(INimbusBoardDbContext db)
    : IRequestHandler<GetBoardsQuery, IReadOnlyList<BoardListItemViewModel>>
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

    public async Task<IReadOnlyList<BoardListItemViewModel>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Boards
            .Include(b => b.Project)
            .Include(b => b.Columns)
            .ThenInclude(c => c.Issues)
            .Select(b => new BoardListItemViewModel
            {
                Id = b.Id,
                Name = b.Name,
                ProjectKey = b.Project.Key,
                ProjectName = b.Project.Name,
                IssueCount = b.Columns.SelectMany(c => c.Issues).Count(),
                OpenIssueCount = b.Columns.SelectMany(c => c.Issues).Count(i => i.Status != IssueStatus.Done),
                DoneIssueCount = b.Columns.SelectMany(c => c.Issues).Count(i => i.Status == IssueStatus.Done),
                ColumnCount = b.Columns.Count
            })
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.AccentClass = Accents[Math.Abs(item.ProjectKey.GetHashCode()) % Accents.Length];
        }

        return items;
    }
}

public sealed class GetBoardQueryHandler(INimbusBoardDbContext db)
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

public sealed class DeleteBoardCommandHandler(INimbusBoardDbContext db) : IRequestHandler<DeleteBoardCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await db.Boards
            .Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new InvalidOperationException("Board not found.");

        var columnIds = board.Columns.Select(c => c.Id).ToList();
        if (columnIds.Count > 0)
        {
            var issues = await db.Issues
                .Where(i => i.BoardColumnId != null && columnIds.Contains(i.BoardColumnId.Value))
                .ToListAsync(cancellationToken);

            foreach (var issue in issues)
            {
                issue.BoardColumnId = null;
                issue.UpdatedAt = DateTime.UtcNow;
            }
        }

        db.BoardColumns.RemoveRange(board.Columns);
        db.Boards.Remove(board);
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
