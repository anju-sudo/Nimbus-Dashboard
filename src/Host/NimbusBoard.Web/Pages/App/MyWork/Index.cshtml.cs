using MediatR;
using NimbusBoard.Application.Boards.Models;
using NimbusBoard.Application.Boards.Queries;
using NimbusBoard.Application.Issues.Models;
using NimbusBoard.Application.Issues.Queries;

namespace Nimbus_Board.Pages.App.MyWork;

public class IndexModel(IMediator mediator) : AppPageModel
{
    public IReadOnlyList<IssueListItemViewModel> Issues { get; private set; } = [];
    public IReadOnlyList<BoardListItemViewModel> Boards { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("my-work", "My Work");
        Issues = await mediator.Send(new GetMyWorkQuery());
        Boards = await mediator.Send(new GetBoardsQuery());
    }
}
