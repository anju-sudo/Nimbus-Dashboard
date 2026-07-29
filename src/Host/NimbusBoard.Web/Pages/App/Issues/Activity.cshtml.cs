using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Collaboration.Queries;

namespace Nimbus_Board.Pages.App.Issues;

public sealed class ActivityModel(IMediator mediator) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string key)
    {
        var activity = await mediator.Send(new GetIssueActivityQuery(key));
        if (activity is null)
        {
            return NotFound();
        }

        return Partial("App/Shared/_IssueActivity", activity);
    }
}
