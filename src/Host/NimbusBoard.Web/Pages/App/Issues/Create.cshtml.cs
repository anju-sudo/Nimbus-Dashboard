using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Issues;

namespace Nimbus_Board.Pages.App.Issues;

public sealed class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public CreateIssueFormModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid projectId, Guid? boardColumnId = null)
    {
        var form = await mediator.Send(new GetIssueCreateFormQuery(projectId, boardColumnId));
        if (form is null)
        {
            return NotFound();
        }

        Input = form;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await mediator.Send(new CreateIssueCommand(
            Input.ProjectId,
            Input.Title,
            Input.Description,
            Input.Type,
            Input.Priority,
            Input.BoardColumnId,
            null,
            Input.StoryPoints,
            Input.DueDate,
            Input.AssigneeMemberId));

        if (result.BoardId.HasValue)
        {
            return Redirect($"/app/boards/{result.BoardId}");
        }

        return Redirect($"/app/issues/{result.Key}");
    }
}
