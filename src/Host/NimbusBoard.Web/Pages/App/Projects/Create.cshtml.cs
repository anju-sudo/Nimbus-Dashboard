using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Projects;

namespace Nimbus_Board.Pages.App.Projects;

public sealed class CreateModel(IMediator mediator) : AppPageModel
{
    [BindProperty]
    public CreateProjectInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("projects", "Create Project");
        Input.WorkspaceId = await mediator.Send(new GetDefaultWorkspaceIdQuery());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await mediator.Send(new CreateProjectCommand(
            Input.Key,
            Input.Name,
            Input.Description,
            Input.WorkspaceId));

        return Redirect($"/app/projects/{result.Key}");
    }

    public sealed class CreateProjectInput
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid WorkspaceId { get; set; }
    }
}
