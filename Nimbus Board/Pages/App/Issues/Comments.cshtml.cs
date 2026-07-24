using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Collaboration.Commands;
using NimbusBoard.Application.Collaboration.Handlers;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Issues.Queries;

namespace Nimbus_Board.Pages.App.Issues;

[IgnoreAntiforgeryToken]
public class CommentsModel(IMediator mediator, INimbusBoardDbContext db) : PageModel
{
    public async Task<IActionResult> OnPostAsync(string key, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest();
        }

        await mediator.Send(new AddCommentCommand(key, body));
        var issue = await mediator.Send(new GetIssueByKeyQuery(key));
        if (issue is null)
        {
            return NotFound();
        }

        var comments = await CollaborationQueryHelper.GetCommentsAsync(db, issue.Id, HttpContext.RequestAborted);
        Response.Headers.Append("HX-Trigger", "refreshActivity");
        return Partial("App/Shared/_CommentThread", comments);
    }
}
