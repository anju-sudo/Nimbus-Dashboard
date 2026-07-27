using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Projects.Commands;

namespace Nimbus_Board.Pages.App.Projects;

public class CreateModel(IMediator mediator, INimbusBoardDbContext db) : AppPageModel
{
    [BindProperty]
    public CreateProjectInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("projects", "Create Project");
        var workspace = await db.Workspaces.FirstOrDefaultAsync();
        Input.WorkspaceId = workspace?.Id ?? Guid.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.WorkspaceId == Guid.Empty)
        {
            var workspace = await db.Workspaces.FirstOrDefaultAsync();
            if (workspace is null)
            {
                var ws = new NimbusBoard.Domain.Entities.Workspace { Name = "Acme", Slug = "acme" };
                db.Workspaces.Add(ws);
                await db.SaveChangesAsync();
                Input.WorkspaceId = ws.Id;
            }
            else
            {
                Input.WorkspaceId = workspace.Id;
            }
        }

        var projectId = await mediator.Send(new CreateProjectCommand(Input.Key, Input.Name, Input.Description, Input.WorkspaceId));
        var project = await db.Projects.FirstAsync(p => p.Id == projectId);
        return Redirect($"/app/projects/{project.Key}");
    }

    public class CreateProjectInput
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid WorkspaceId { get; set; }
    }
}
