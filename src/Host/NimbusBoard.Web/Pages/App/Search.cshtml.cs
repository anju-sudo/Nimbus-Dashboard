using MediatR;
using NimbusBoard.Application.Search;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nimbus_Board.Pages.App;

public class SearchModel(IMediator mediator) : PageModel
{
    public SearchResultsViewModel Results { get; private set; } = new();

    public async Task OnGetAsync(string? q)
    {
        Results = await mediator.Send(new SearchQuery(q ?? string.Empty));
    }
}
