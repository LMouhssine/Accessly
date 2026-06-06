using Accessly.Application.Common;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Notifications;

public sealed record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Subject,
    string Body,
    NotificationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt,
    string? ToAddress);

public sealed record GetNotificationsQuery(int Take = 50) : IQuery<IReadOnlyList<NotificationDto>>;

public sealed class GetNotificationsHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    public async Task<IReadOnlyList<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        AccessGuard.EnsureAuthenticated(user);
        var take = Math.Clamp(request.Take, 1, 200);

        var query = db.Notifications.AsNoTracking();
        query = user.Role switch
        {
            UserRole.Admin => query,
            UserRole.Attendee => query.Where(n => n.UserId == user.UserId),
            _ => query.Where(n => n.OrganizationId == user.OrganizationId || n.UserId == user.UserId),
        };

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .Select(n => new NotificationDto(n.Id, n.Type, n.Subject, n.Body, n.Status, n.CreatedAt, n.SentAt, n.ToAddress))
            .ToListAsync(cancellationToken);
    }
}

public sealed record SendTestNotificationCommand(string? ToAddress = null) : ICommand<NotificationDto>;

public sealed class SendTestNotificationHandler(IAppDbContext db, ICurrentUser user, IClock clock, IEmailSender email)
    : IRequestHandler<SendTestNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(SendTestNotificationCommand request, CancellationToken cancellationToken)
    {
        AccessGuard.EnsureAuthenticated(user);
        var toAddress = string.IsNullOrWhiteSpace(request.ToAddress) ? "demo@accessly.local" : request.ToAddress.Trim();

        var notification = new Notification
        {
            UserId = user.UserId,
            OrganizationId = user.OrganizationId,
            Type = NotificationType.Test,
            Channel = NotificationChannel.Email,
            ToAddress = toAddress,
            Subject = "Accessly test notification",
            Body = "This is a test notification from Accessly.",
            Status = NotificationStatus.Sent,
            SentAt = clock.UtcNow,
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);
        await email.SendAsync(toAddress, notification.Subject, notification.Body, cancellationToken);

        return new NotificationDto(notification.Id, notification.Type, notification.Subject, notification.Body, notification.Status, notification.CreatedAt, notification.SentAt, notification.ToAddress);
    }
}
