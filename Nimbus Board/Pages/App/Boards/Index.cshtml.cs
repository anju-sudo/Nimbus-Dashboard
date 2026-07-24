using MediatR;
using NimbusBoard.Application.Boards.Models;
using NimbusBoard.Application.Boards.Queries;

namespace Nimbus_Board.Pages.App.Boards;

public class IndexModel(IMediator mediator) : AppPageModel
{
    public IReadOnlyList<BoardListItemViewModel> Boards { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("boards", "Boards");
        Boards = await mediator.Send(new GetBoardsQuery());
    }
}
