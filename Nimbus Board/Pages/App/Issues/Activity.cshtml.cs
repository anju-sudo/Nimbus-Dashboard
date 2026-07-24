using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Collaboration.Handlers;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Issues.Queries;

namespace Nimbus_Board.Pages.App.Issues;

public class ActivityModel(IMediator mediator, INimbusBoardDbContext db) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string key)
    {
        var issue = await mediator.Send(new GetIssueByKeyQuery(key));
        if (issue is null)
        {
            return NotFound();
        }

        var activity = await CollaborationQueryHelper.GetActivityAsync(db, issue.Id, HttpContext.RequestAborted);
        return Partial("App/Shared/_IssueActivity", activity);
    }
}
