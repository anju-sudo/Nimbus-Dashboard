using MediatR;
using NimbusBoard.Application.Notifications.Models;

namespace NimbusBoard.Application.Notifications.Queries;

public record GetNotificationsQuery(int RecipientMemberId, int Take = 50) : IRequest<IReadOnlyList<NotificationItemViewModel>>;

public record GetUnreadNotificationCountQuery(int RecipientMemberId) : IRequest<int>;
