using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Issues.Commands;
using NimbusBoard.Application.Issues.Models;

namespace Nimbus_Board.Pages.App.Issues;

public class CreateModel(IMediator mediator, INimbusBoardDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public CreateIssueFormModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid projectId, Guid? boardColumnId = null)
    {
        Input.ProjectId = projectId;
        Input.BoardColumnId = boardColumnId;
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        Input.ProjectKey = project?.Key ?? string.Empty;

        var members = await db.ProjectMembers
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.DisplayName)
            .ToListAsync();
        Input.AssignableMembers = members.Select(MemberAvatarHelper.ToViewModel).ToList();
        Input.AssigneeMemberId = 1;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var key = await mediator.Send(new CreateIssueCommand(
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

        if (Input.BoardColumnId.HasValue)
        {
            var board = await db.Boards
                .Include(b => b.Columns)
                .FirstOrDefaultAsync(b => b.Columns.Any(c => c.Id == Input.BoardColumnId));
            if (board is not null)
            {
                return Redirect($"/app/boards/{board.Id}");
            }
        }

        return Redirect($"/app/issues/{key}");
    }
}
