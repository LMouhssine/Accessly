using Accessly.Application.Common.Interfaces;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Accessly.Infrastructure.Jobs;

/// <summary>Scheduled notification work executed by Hangfire in the worker.</summary>
public sealed class NotificationJobs(IAppDbContext db, IEmailSender email, IClock clock, ILogger<NotificationJobs> logger)
{
    /// <summary>Sends a reminder for events starting in roughly 24 hours (idempotent per attendee/event).</summary>
    public async Task SendDueRemindersAsync()
    {
        var now = clock.UtcNow;
        var windowStart = now.AddHours(23);
        var windowEnd = now.AddHours(25);

        var events = await db.Events
            .Where(e => e.Status == EventStatus.Published && e.StartAt >= windowStart && e.StartAt <= windowEnd)
            .ToListAsync();

        var sent = 0;
        foreach (var @event in events)
        {
            var subject = $"Reminder: {@event.Title} starts soon";
            var bookings = await db.Bookings
                .Include(b => b.AttendeeUser)
                .Where(b => b.EventId == @event.Id && b.Status == BookingStatus.Confirmed)
                .ToListAsync();

            foreach (var booking in bookings)
            {
                var alreadyReminded = await db.Notifications.AnyAsync(n =>
                    n.UserId == booking.AttendeeUserId && n.Type == NotificationType.EventReminder && n.Subject == subject);
                if (alreadyReminded)
                {
                    continue;
                }

                var body = $"Hi {booking.AttendeeUser.DisplayName}, {@event.Title} starts on {@event.StartAt:f}. See you there!";
                db.Notifications.Add(new Notification
                {
                    UserId = booking.AttendeeUserId,
                    OrganizationId = @event.OrganizationId,
                    Type = NotificationType.EventReminder,
                    Channel = NotificationChannel.Email,
                    ToAddress = booking.AttendeeUser.Email,
                    Subject = subject,
                    Body = body,
                    Status = NotificationStatus.Sent,
                    SentAt = now,
                });
                await email.SendAsync(booking.AttendeeUser.Email, subject, body);
                sent++;
            }
        }

        if (sent > 0)
        {
            await db.SaveChangesAsync();
        }

        logger.LogInformation("Sent {Count} event reminder(s).", sent);
    }

    /// <summary>Marks published events whose end time has passed as completed.</summary>
    public async Task CompletePastEventsAsync()
    {
        var now = clock.UtcNow;
        var ended = await db.Events
            .Where(e => e.Status == EventStatus.Published && e.EndAt < now)
            .ToListAsync();

        foreach (var @event in ended)
        {
            @event.Status = EventStatus.Completed;
        }

        if (ended.Count > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Marked {Count} event(s) as completed.", ended.Count);
        }
    }
}
