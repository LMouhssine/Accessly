using Accessly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Common.Interfaces;

/// <summary>Persistence abstraction exposed to the application layer.</summary>
public interface IAppDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }
    DbSet<Event> Events { get; }
    DbSet<Speaker> Speakers { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<CheckIn> CheckIns { get; }
    DbSet<Feedback> Feedbacks { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Payment> Payments { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
