using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Projects.Commands;
using NimbusBoard.Application.Projects.Models;
using NimbusBoard.Application.Projects.Queries;

namespace Nimbus_Board.Pages.App.Projects;

public class DetailModel(IMediator mediator) : AppPageModel
{
    [BindProperty(SupportsGet = true)]
    public string Key { get; set; } = string.Empty;

    public ProjectDetailViewModel? Project { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await SetLayoutDataAsync("projects", Key);
        Project = await mediator.Send(new GetProjectByKeyQuery(Key));
        return Project is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAddMemberAsync(Guid projectId, string displayName, string initials, string role)
    {
        await mediator.Send(new AddProjectMemberCommand(projectId, displayName, initials, role));
        return RedirectToPage(new { key = Key });
    }
}
