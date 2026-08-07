using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Boards;

namespace Nimbus_Board.Pages.App.Boards;

public class IndexModel(IMediator mediator) : AppPageModel
{
    public IReadOnlyList<BoardListItemViewModel> Boards { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("boards", "Boards");
        Boards = await mediator.Send(new GetBoardsQuery());
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid boardId)
    {
        await mediator.Send(new DeleteBoardCommand(boardId));
        return RedirectToPage();
    }
}
