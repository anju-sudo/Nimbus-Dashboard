using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Collaboration;

namespace Nimbus_Board.Pages.App.Issues;

[IgnoreAntiforgeryToken]
public sealed class CommentsModel(IMediator mediator) : PageModel
{
    public async Task<IActionResult> OnPostAsync(string key, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest();
        }

        await mediator.Send(new AddCommentCommand(key, body));
        var comments = await mediator.Send(new GetIssueCommentsQuery(key));
        if (comments is null)
        {
            return NotFound();
        }

        Response.Headers.Append("HX-Trigger", "refreshActivity");
        return Partial("App/Shared/_CommentThread", comments);
    }
}
