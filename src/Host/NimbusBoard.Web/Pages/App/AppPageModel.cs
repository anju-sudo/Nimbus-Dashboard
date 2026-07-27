using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using NimbusBoard.Application.Notifications.Queries;

namespace Nimbus_Board.Pages.App;

public abstract class AppPageModel : PageModel
{
    protected const int CurrentMemberId = 1;

    protected void SetLayoutData(
        string activeNav,
        string title,
        int unread = 0,
        string userName = "Anjumol Babu",
        string userInitials = "AB",
        string workspace = "Acme")
    {
        ViewData["ActiveNav"] = activeNav;
        ViewData["Title"] = title;
        ViewData["UnreadNotifications"] = unread;
        ViewData["UserName"] = userName;
        ViewData["UserInitials"] = userInitials;
        ViewData["WorkspaceName"] = workspace;
        ViewData["MemberId"] = CurrentMemberId;
    }

    protected async Task SetLayoutDataAsync(
        string activeNav,
        string title,
        string userName = "Anjumol Babu",
        string userInitials = "AB",
        string workspace = "Acme",
        CancellationToken cancellationToken = default)
    {
        var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
        var unread = await mediator.Send(new GetUnreadNotificationCountQuery(CurrentMemberId), cancellationToken);
        SetLayoutData(activeNav, title, unread, userName, userInitials, workspace);
    }
}
