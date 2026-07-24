using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Collaboration.Commands;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Issues.Queries;

namespace Nimbus_Board.Pages.App.Issues;

[IgnoreAntiforgeryToken]
public class DeleteAttachmentModel(IMediator mediator, INimbusBoardDbContext db) : PageModel
{
    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var attachment = await db.Attachments.Include(a => a.Issue).FirstOrDefaultAsync(a => a.Id == id);
        if (attachment is null)
        {
            return NotFound();
        }

        var issueKey = attachment.Issue.Key;
        await mediator.Send(new DeleteAttachmentCommand(id));

        var issue = await mediator.Send(new GetIssueByKeyQuery(issueKey));
        Response.Headers.Append("HX-Trigger", "refreshActivity");
        return Partial("App/Shared/_AttachmentList", issue);
    }
}
