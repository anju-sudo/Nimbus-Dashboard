using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Boards.Models;
using NimbusBoard.Application.Boards.Queries;

namespace Nimbus_Board.Pages.App.Boards;

public class DetailModel(IMediator mediator) : AppPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public BoardViewModel? Board { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await SetLayoutDataAsync("boards", "Board");
        Board = await mediator.Send(new GetBoardQuery(Id));
        return Board is null ? NotFound() : Page();
    }
}
