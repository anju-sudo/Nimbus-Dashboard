using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Notifications.Commands;
using NimbusBoard.Application.Notifications.Models;
using NimbusBoard.Application.Notifications.Queries;

namespace NimbusBoard.Application.Notifications.Handlers;

public class GetNotificationsQueryHandler(INimbusBoardDbContext db)
    : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationItemViewModel>>
{
    public async Task<IReadOnlyList<NotificationItemViewModel>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Notifications
            .Where(n => n.RecipientMemberId == request.RecipientMemberId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return items.Select(n => new NotificationItemViewModel
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Message = n.Message,
            LinkUrl = n.LinkUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            TimeAgo = TimeAgoHelper.Format(n.CreatedAt)
        }).ToList();
    }
}

public class GetUnreadNotificationCountQueryHandler(INimbusBoardDbContext db)
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        return await db.Notifications.CountAsync(
            n => n.RecipientMemberId == request.RecipientMemberId && !n.IsRead,
            cancellationToken);
    }
}

public class MarkNotificationReadCommandHandler(INimbusBoardDbContext db)
    : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications.FirstOrDefaultAsync(
            n => n.Id == request.NotificationId && n.RecipientMemberId == request.RecipientMemberId,
            cancellationToken);

        if (notification is null)
        {
            return Unit.Value;
        }

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class MarkAllNotificationsReadCommandHandler(INimbusBoardDbContext db)
    : IRequestHandler<MarkAllNotificationsReadCommand, Unit>
{
    public async Task<Unit> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await db.Notifications
            .Where(n => n.RecipientMemberId == request.RecipientMemberId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var item in unread)
        {
            item.IsRead = true;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
