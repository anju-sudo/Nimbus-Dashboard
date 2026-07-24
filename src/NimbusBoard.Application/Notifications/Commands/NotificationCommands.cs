using MediatR;

namespace NimbusBoard.Application.Notifications.Commands;

public record MarkNotificationReadCommand(Guid NotificationId, int RecipientMemberId) : IRequest<Unit>;

public record MarkAllNotificationsReadCommand(int RecipientMemberId) : IRequest<Unit>;
