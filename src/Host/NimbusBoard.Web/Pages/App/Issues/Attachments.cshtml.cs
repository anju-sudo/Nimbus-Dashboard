using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Collaboration.Commands;
using NimbusBoard.Application.Issues.Queries;

namespace Nimbus_Board.Pages.App.Issues;

[IgnoreAntiforgeryToken]
public class AttachmentsModel(IMediator mediator) : PageModel
{
    public async Task<IActionResult> OnPostAsync(string key, IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest();
        }

        await using var stream = file.OpenReadStream();
        await mediator.Send(new UploadAttachmentCommand(key, stream, file.FileName));

        var issue = await mediator.Send(new GetIssueByKeyQuery(key));
        if (issue is null)
        {
            return NotFound();
        }

        Response.Headers.Append("HX-Trigger", "refreshActivity");
        return Partial("App/Shared/_AttachmentList", issue);
    }
}
