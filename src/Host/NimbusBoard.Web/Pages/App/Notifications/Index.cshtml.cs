using MediatR;
using Microsoft.AspNetCore.Mvc;
using NimbusBoard.Application.Notifications.Commands;
using NimbusBoard.Application.Notifications.Models;
using NimbusBoard.Application.Notifications.Queries;

namespace Nimbus_Board.Pages.App.Notifications;

public class IndexModel(IMediator mediator) : AppPageModel
{
    private const int MemberId = 1;

    public IReadOnlyList<NotificationItemViewModel> Notifications { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("notifications", "Notifications");
        Notifications = await mediator.Send(new GetNotificationsQuery(MemberId));
    }

    public async Task<IActionResult> OnPostMarkReadAsync(Guid id)
    {
        await mediator.Send(new MarkNotificationReadCommand(id, MemberId));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        await mediator.Send(new MarkAllNotificationsReadCommand(MemberId));
        return RedirectToPage();
    }
}
