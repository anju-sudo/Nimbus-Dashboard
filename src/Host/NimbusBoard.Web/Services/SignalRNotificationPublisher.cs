using Microsoft.AspNetCore.SignalR;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Enums;
using NimbusBoard.Infrastructure.Notifications;
using Nimbus_Board.Hubs;

namespace Nimbus_Board.Services;

/// <summary>
/// Host decorator over Infrastructure <see cref="NotificationPublisher"/> that pushes SignalR after persist.
/// </summary>
public sealed class SignalRNotificationPublisher(
    NotificationPublisher inner,
    IHubContext<NotificationHub> hub) : IAppNotificationService
{
    public async Task PublishAsync(
        int recipientMemberId,
        NotificationType type,
        string message,
        string? linkUrl = null,
        Guid? issueId = null,
        string? emailTo = null,
        CancellationToken cancellationToken = default)
    {
        await inner.PublishAsync(recipientMemberId, type, message, linkUrl, issueId, emailTo, cancellationToken);

        await hub.Clients.Group($"member:{recipientMemberId}")
            .SendAsync("notificationReceived", new
            {
                type = type.ToString(),
                message,
                linkUrl,
                createdAt = DateTime.UtcNow
            }, cancellationToken);
    }
}
