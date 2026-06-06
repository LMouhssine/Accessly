using Accessly.Application.Common.Events;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Infrastructure.Messaging;

/// <summary>On a confirmed booking, records and "sends" a confirmation notification.</summary>
public sealed class BookingConfirmedNotificationHandler(IAppDbContext db, IEmailSender email, IClock clock)
    : IIntegrationEventHandler<BookingConfirmedIntegrationEvent>
{
    public async Task HandleAsync(BookingConfirmedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var booking = await db.Bookings
            .Include(b => b.Event)
            .Include(b => b.AttendeeUser)
            .FirstOrDefaultAsync(b => b.Id == integrationEvent.BookingId, cancellationToken);
        if (booking is null)
        {
            return;
        }

        var subject = $"Your booking for {booking.Event.Title} is confirmed";
        var body = $"Hi {booking.AttendeeUser.DisplayName}, your booking for {booking.Event.Title} on " +
                   $"{booking.Event.StartAt:f} is confirmed. Your ticket is available in your account.";

        db.Notifications.Add(new Notification
        {
            UserId = booking.AttendeeUserId,
            OrganizationId = booking.Event.OrganizationId,
            Type = NotificationType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            ToAddress = booking.AttendeeUser.Email,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Sent,
            SentAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
        await email.SendAsync(booking.AttendeeUser.Email, subject, body, cancellationToken);
    }
}

/// <summary>When an event is cancelled, notifies all confirmed attendees.</summary>
public sealed class EventCancelledNotificationHandler(IAppDbContext db, IEmailSender email, IClock clock)
    : IIntegrationEventHandler<EventCancelledIntegrationEvent>
{
    public async Task HandleAsync(EventCancelledIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var @event = await db.Events.FirstOrDefaultAsync(e => e.Id == integrationEvent.EventId, cancellationToken);
        if (@event is null)
        {
            return;
        }

        var bookings = await db.Bookings
            .Include(b => b.AttendeeUser)
            .Where(b => b.EventId == integrationEvent.EventId && b.Status == BookingStatus.Confirmed)
            .ToListAsync(cancellationToken);

        foreach (var booking in bookings)
        {
            var subject = $"{@event.Title} has been cancelled";
            var body = $"We're sorry, but {@event.Title} scheduled for {@event.StartAt:f} has been cancelled.";

            db.Notifications.Add(new Notification
            {
                UserId = booking.AttendeeUserId,
                OrganizationId = @event.OrganizationId,
                Type = NotificationType.EventCancelled,
                Channel = NotificationChannel.Email,
                ToAddress = booking.AttendeeUser.Email,
                Subject = subject,
                Body = body,
                Status = NotificationStatus.Sent,
                SentAt = clock.UtcNow,
            });
            await email.SendAsync(booking.AttendeeUser.Email, subject, body, cancellationToken);
        }

        if (bookings.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
