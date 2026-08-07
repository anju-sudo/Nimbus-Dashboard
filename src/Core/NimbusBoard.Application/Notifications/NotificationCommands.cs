using MediatR;

namespace NimbusBoard.Application.Notifications;

public record MarkNotificationReadCommand(Guid NotificationId, int RecipientMemberId) : IRequest<Unit>;

public record MarkAllNotificationsReadCommand(int RecipientMemberId) : IRequest<Unit>;
