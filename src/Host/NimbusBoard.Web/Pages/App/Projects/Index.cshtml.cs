using MediatR;
using NimbusBoard.Application.Projects.Queries;

namespace Nimbus_Board.Pages.App.Projects;

public class IndexModel(IMediator mediator) : AppPageModel
{
    public IReadOnlyList<NimbusBoard.Application.Projects.Models.ProjectListItemViewModel> Projects { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("projects", "Projects");
        Projects = await mediator.Send(new GetProjectsQuery());
    }
}
