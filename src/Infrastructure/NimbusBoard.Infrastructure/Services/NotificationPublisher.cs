using Microsoft.Extensions.Logging;
using NimbusBoard.Application.Common.Abstractions;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;
using NimbusBoard.Infrastructure.Persistence;

namespace NimbusBoard.Infrastructure.Services;

/// <summary>
/// Persists notifications and optionally emails. Host may decorate this with SignalR.
/// </summary>
public sealed class NotificationPublisher(
    NimbusBoardDbContext db,
    IEmailSender emailSender,
    ILogger<NotificationPublisher> logger) : IAppNotificationService
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
        var notification = new Notification
        {
            RecipientMemberId = recipientMemberId,
            Type = type,
            Message = message,
            LinkUrl = linkUrl,
            IssueId = issueId,
            IsRead = false
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(emailTo) && ShouldEmail(type))
        {
            try
            {
                await emailSender.SendAsync(emailTo, $"Nimbus: {type}", message, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send email notification to {Email}", emailTo);
            }
        }
    }

    private static bool ShouldEmail(NotificationType type) =>
        type is NotificationType.Assigned or NotificationType.Mentioned or NotificationType.SprintStarted;
}
