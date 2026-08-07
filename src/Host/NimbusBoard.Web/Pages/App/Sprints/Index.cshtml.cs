using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Sprints;

namespace Nimbus_Board.Pages.App.Sprints;

public class IndexModel(IMediator mediator) : AppPageModel
{
    public IReadOnlyList<SprintListItemViewModel> Sprints { get; private set; } = [];
    public SprintCreateFormViewModel CreateForm { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    [BindProperty]
    public CreateSprintInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (CreateForm.Projects.Count == 0 && Input.ProjectId == Guid.Empty)
        {
            await LoadAsync();
        }

        if (string.IsNullOrWhiteSpace(Input.Name))
        {
            ErrorMessage = "Sprint name is required.";
            await LoadAsync();
            return Page();
        }

        if (Input.ProjectId == Guid.Empty)
        {
            ErrorMessage = "Select a project for this sprint.";
            await LoadAsync();
            return Page();
        }

        if (Input.EndDate.Date < Input.StartDate.Date)
        {
            ErrorMessage = "End date must be on or after the start date.";
            await LoadAsync();
            return Page();
        }

        try
        {
            var id = await mediator.Send(new CreateSprintCommand(
                Input.ProjectId,
                Input.Name,
                Input.Goal,
                Input.StartDate == default ? DateTime.UtcNow.Date : Input.StartDate,
                Input.EndDate == default ? DateTime.UtcNow.Date.AddDays(14) : Input.EndDate));

            return Redirect($"/app/sprints/{id}");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            await LoadAsync();
            return Page();
        }
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
        }

        if (Input.EndDate == default)
        {
            Input.EndDate = Input.StartDate.AddDays(14);
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
