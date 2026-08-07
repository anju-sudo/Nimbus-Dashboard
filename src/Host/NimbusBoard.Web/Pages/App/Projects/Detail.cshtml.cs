using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Projects;

namespace Nimbus_Board.Pages.App.Projects;

public class DetailModel(IMediator mediator) : AppPageModel
{
    [BindProperty(SupportsGet = true)]
    public string Key { get; set; } = string.Empty;

    public ProjectDetailViewModel? Project { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Avoid clashing with static routes like /app/projects/new
        if (string.IsNullOrWhiteSpace(Key)
            || Key.Equals("new", StringComparison.OrdinalIgnoreCase)
            || Key.Equals("create", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect("/app/projects/new");
        }

        await SetLayoutDataAsync("projects", Key);
        Project = await mediator.Send(new GetProjectByKeyQuery(Key));
        if (Project is null)
        {
            return Redirect("/app/projects");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddMemberAsync(Guid projectId, string displayName, string initials, string role)
    {
        await mediator.Send(new AddProjectMemberCommand(projectId, displayName, initials, role));
        return RedirectToPage(new { key = Key });
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        Project = await mediator.Send(new GetProjectByKeyQuery(Key));
        if (Project is null)
        {
            return RedirectToPage("./Index");
        }

        await mediator.Send(new DeleteProjectCommand(Project.Id));
        return RedirectToPage("./Index");
    }
}