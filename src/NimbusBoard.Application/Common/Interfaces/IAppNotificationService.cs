using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Common.Interfaces;

public interface IAppNotificationService
{
    Task PublishAsync(
        int recipientMemberId,
        NotificationType type,
        string message,
        string? linkUrl = null,
        Guid? issueId = null,
        string? emailTo = null,
        CancellationToken cancellationToken = default);
}
