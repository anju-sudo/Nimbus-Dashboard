using MediatR;
using NimbusBoard.Application.Boards;
using NimbusBoard.Application.Issues;

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
