using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Projects;

namespace Nimbus_Board.Pages.App.Projects;

public class IndexModel(IMediator mediator) : AppPageModel
{
    public IReadOnlyList<ProjectListItemViewModel> Projects { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("projects", "Projects");
        Projects = await mediator.Send(new GetProjectsQuery());
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid projectId)
    {
        await mediator.Send(new DeleteProjectCommand(projectId));
        return RedirectToPage();
    }
}
