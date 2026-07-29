using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Collaboration.Commands;
using NimbusBoard.Application.Issues.Queries;

namespace Nimbus_Board.Pages.App.Issues;

[IgnoreAntiforgeryToken]
public sealed class DeleteAttachmentModel(IMediator mediator) : PageModel
{
    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var issueKey = await mediator.Send(new DeleteAttachmentCommand(id));
        if (issueKey is null)
        {
            return NotFound();
        }

        var issue = await mediator.Send(new GetIssueByKeyQuery(issueKey));
        Response.Headers.Append("HX-Trigger", "refreshActivity");
        return Partial("App/Shared/_AttachmentList", issue);
    }
}
