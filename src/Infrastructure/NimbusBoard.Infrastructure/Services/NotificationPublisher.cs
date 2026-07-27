using Microsoft.Extensions.Logging;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;
using NimbusBoard.Infrastructure.Persistence;

namespace NimbusBoard.Infrastructure.Services;

public class NotificationPublisher(
    NimbusBoardDbContext db,
    IEmailSender emailSender,
    ILogger<NotificationPublisher> logger) : IAppNotificationService
{
    public virtual async Task PublishAsync(
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

        await OnPublishedAsync(notification, cancellationToken);
    }

    protected virtual Task OnPublishedAsync(Notification notification, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    private static bool ShouldEmail(NotificationType type) =>
        type is NotificationType.Assigned or NotificationType.Mentioned or NotificationType.SprintStarted;
}
