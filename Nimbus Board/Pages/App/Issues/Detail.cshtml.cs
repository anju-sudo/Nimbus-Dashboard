using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Issues.Commands;
using NimbusBoard.Application.Issues.Models;
using NimbusBoard.Application.Issues.Queries;

namespace Nimbus_Board.Pages.App.Issues;

public class DetailModel(IMediator mediator) : AppPageModel
{
    [BindProperty(SupportsGet = true)]
    public string Key { get; set; } = string.Empty;

    public bool IsPartial { get; private set; }
    public IssueDetailViewModel? Issue { get; private set; }

    public async Task<IActionResult> OnGetAsync(bool partial = false)
    {
        IsPartial = partial;
        if (!partial)
        {
            await SetLayoutDataAsync("issues", Key);
        }

        Issue = await mediator.Send(new GetIssueByKeyQuery(Key));
        return Issue is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(string title, string? description, string type, string priority, int? storyPoints, DateTime? dueDate, string? assigneeName, string? assigneeInitials)
    {
        await mediator.Send(new UpdateIssueCommand(Key, title, description, type, priority, storyPoints, dueDate, assigneeName, assigneeInitials));
        return Redirect($"/app/issues/{Key}");
    }
}
