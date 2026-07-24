using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Sprints.Commands;
using NimbusBoard.Application.Sprints.Models;
using NimbusBoard.Application.Sprints.Queries;

namespace Nimbus_Board.Pages.App.Sprints;

public class DetailModel(IMediator mediator) : AppPageModel
{
    public SprintDetailViewModel? Sprint { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Sprint = await mediator.Send(new GetSprintDetailQuery(id));
        if (Sprint is null)
        {
            return NotFound();
        }

        await SetLayoutDataAsync("sprints", Sprint.Name);
        return Page();
    }

    public async Task<IActionResult> OnPostStartAsync(Guid id)
    {
        await mediator.Send(new StartSprintCommand(id));
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        await mediator.Send(new CompleteSprintCommand(id));
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAssignAsync(Guid id, Guid issueId)
    {
        await mediator.Send(new AssignIssueToSprintCommand(issueId, id));
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid id, Guid issueId)
    {
        await mediator.Send(new AssignIssueToSprintCommand(issueId, null));
        return RedirectToPage(new { id });
    }
}
