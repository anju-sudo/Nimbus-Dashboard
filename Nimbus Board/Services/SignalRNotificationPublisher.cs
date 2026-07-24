using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Infrastructure.Persistence;
using NimbusBoard.Infrastructure.Services;
using Nimbus_Board.Hubs;

namespace Nimbus_Board.Services;

public class SignalRNotificationPublisher(
    NimbusBoardDbContext db,
    IEmailSender emailSender,
    IHubContext<NotificationHub> hub,
    ILogger<NotificationPublisher> logger)
    : NotificationPublisher(db, emailSender, logger)
{
    protected override async Task OnPublishedAsync(Notification notification, CancellationToken cancellationToken)
    {
        await hub.Clients.Group($"member:{notification.RecipientMemberId}")
            .SendAsync("notificationReceived", new
            {
                id = notification.Id,
                type = notification.Type.ToString(),
                message = notification.Message,
                linkUrl = notification.LinkUrl,
                createdAt = notification.CreatedAt
            }, cancellationToken);
    }
}
