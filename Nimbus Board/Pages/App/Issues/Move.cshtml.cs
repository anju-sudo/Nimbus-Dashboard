using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Issues.Commands;

namespace Nimbus_Board.Pages.App.Issues;

[IgnoreAntiforgeryToken]
public class MoveModel(IMediator mediator) : PageModel
{
    public async Task<IActionResult> OnPostAsync(Guid issueId, Guid boardColumnId, int sortOrder = 0)
    {
        await mediator.Send(new MoveIssueCommand(issueId, boardColumnId, sortOrder));
        return new OkResult();
    }
}
