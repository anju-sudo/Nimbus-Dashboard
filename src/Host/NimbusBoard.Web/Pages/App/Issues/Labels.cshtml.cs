using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Collaboration.Commands;
using NimbusBoard.Application.Issues.Queries;

namespace Nimbus_Board.Pages.App.Issues;

[IgnoreAntiforgeryToken]
public class LabelsModel(IMediator mediator) : PageModel
{
    public async Task<IActionResult> OnPostAsync(string key, Guid labelId)
    {
        await mediator.Send(new ToggleIssueLabelCommand(key, labelId));
        var issue = await mediator.Send(new GetIssueByKeyQuery(key));
        if (issue is null)
        {
            return NotFound();
        }

        Response.Headers.Append("HX-Trigger", "refreshActivity");
        return Partial("App/Shared/_LabelPicker", issue);
    }
}
