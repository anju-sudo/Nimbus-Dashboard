using MediatR;
using NimbusBoard.Application.Notifications;

namespace NimbusBoard.Application.Notifications;

public record GetNotificationsQuery(int RecipientMemberId, int Take = 50) : IRequest<IReadOnlyList<NotificationItemViewModel>>;

public record GetUnreadNotificationCountQuery(int RecipientMemberId) : IRequest<int>;
