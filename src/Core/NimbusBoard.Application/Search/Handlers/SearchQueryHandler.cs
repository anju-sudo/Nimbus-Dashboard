using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Search.Models;
using NimbusBoard.Application.Search.Queries;

namespace NimbusBoard.Application.Search.Handlers;

public class SearchQueryHandler(INimbusBoardDbContext db) : IRequestHandler<SearchQuery, SearchResultsViewModel>
{
    public async Task<SearchResultsViewModel> Handle(SearchQuery request, CancellationToken cancellationToken)
    {
        var term = (request.Term ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            return new SearchResultsViewModel { Term = term };
        }

        var limit = Math.Clamp(request.Limit, 1, 20);
        var like = term.ToLowerInvariant();

        var issues = await db.Issues
            .Include(i => i.Project)
            .Where(i => i.Key.ToLower().Contains(like) || i.Title.ToLower().Contains(like))
            .OrderBy(i => i.Key)
            .Take(limit)
            .Select(i => new SearchHitViewModel
            {
                Title = i.Key,
                Subtitle = i.Title,
                Url = "/app/issues/" + i.Key
            })
            .ToListAsync(cancellationToken);

        var projects = await db.Projects
            .Where(p => p.Key.ToLower().Contains(like) || p.Name.ToLower().Contains(like))
            .OrderBy(p => p.Name)
            .Take(limit)
            .Select(p => new SearchHitViewModel
            {
                Title = p.Key,
                Subtitle = p.Name,
                Url = "/app/projects/" + p.Key
            })
            .ToListAsync(cancellationToken);

        var boards = await db.Boards
            .Include(b => b.Project)
            .Where(b => b.Name.ToLower().Contains(like))
            .OrderBy(b => b.Name)
            .Take(limit)
            .Select(b => new SearchHitViewModel
            {
                Title = b.Name,
                Subtitle = b.Project.Key,
                Url = "/app/boards/" + b.Id
            })
            .ToListAsync(cancellationToken);

        return new SearchResultsViewModel
        {
            Term = term,
            Issues = issues,
            Projects = projects,
            Boards = boards
        };
    }
}
