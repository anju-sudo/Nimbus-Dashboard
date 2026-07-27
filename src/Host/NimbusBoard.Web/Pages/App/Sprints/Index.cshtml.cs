using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Notifications.Queries;
using NimbusBoard.Application.Sprints.Commands;
using NimbusBoard.Application.Sprints.Models;
using NimbusBoard.Application.Sprints.Queries;

namespace Nimbus_Board.Pages.App.Sprints;

public class IndexModel(IMediator mediator) : AppPageModel
{
    public IReadOnlyList<SprintListItemViewModel> Sprints { get; private set; } = [];
    public SprintCreateFormViewModel CreateForm { get; private set; } = new();

    [BindProperty]
    public CreateSprintInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Name) || Input.ProjectId == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "Name and project are required.");
            await LoadAsync();
            return Page();
        }

        var id = await mediator.Send(new CreateSprintCommand(
            Input.ProjectId,
            Input.Name,
            Input.Goal,
            Input.StartDate,
            Input.EndDate));

        return RedirectToPage("Detail", new { id });
    }

    private async Task LoadAsync()
    {
        await SetLayoutDataAsync("sprints", "Sprints");
        Sprints = await mediator.Send(new GetSprintsQuery());
        CreateForm = await mediator.Send(new GetSprintCreateFormQuery());
        if (Input.ProjectId == Guid.Empty && CreateForm.Projects.Count > 0)
        {
            Input.ProjectId = CreateForm.Projects[0].Id;
        }

        if (Input.StartDate == default)
        {
            Input.StartDate = DateTime.UtcNow.Date;
            Input.EndDate = DateTime.UtcNow.Date.AddDays(14);
        }
    }

    public class CreateSprintInput
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Goal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
