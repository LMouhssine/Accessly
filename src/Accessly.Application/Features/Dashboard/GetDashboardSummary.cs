using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.Events;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Dashboard;

/// <summary>Aggregate counts and a short upcoming list for the organizer dashboard.</summary>
public sealed record DashboardSummaryDto(
    int TotalEvents,
    int PublishedEvents,
    int DraftEvents,
    int UpcomingEvents,
    int ConfirmedBookings,
    IReadOnlyList<EventSummaryDto> Upcoming);

public sealed record GetDashboardSummaryQuery : IQuery<DashboardSummaryDto>;

/// <summary>
/// Computes the dashboard summary, cached briefly per organization. The summary tolerates a
/// few seconds of staleness, which makes it a good fit for a short-lived distributed cache.
/// </summary>
public sealed class GetDashboardSummaryHandler(IAppDbContext db, ICurrentUser user, IClock clock, ICacheService cache)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // Admins see every organization; organizers and staff see their own.
        var organizationId = user.Role == UserRole.Admin ? null : user.OrganizationId;
        var cacheKey = $"dashboard:summary:{organizationId?.ToString() ?? "all"}";

        return cache.GetOrSetAsync(cacheKey, ct => Compute(organizationId, ct), CacheTtl, cancellationToken);
    }

    private async Task<DashboardSummaryDto> Compute(Guid? organizationId, CancellationToken cancellationToken)
    {
        var events = db.Events.AsNoTracking();
        if (organizationId.HasValue)
        {
            events = events.Where(e => e.OrganizationId == organizationId.Value);
        }

        var now = clock.UtcNow;

        var total = await events.CountAsync(cancellationToken);
        var published = await events.CountAsync(e => e.Status == EventStatus.Published, cancellationToken);
        var draft = await events.CountAsync(e => e.Status == EventStatus.Draft, cancellationToken);
        var upcomingCount = await events.CountAsync(
            e => e.Status == EventStatus.Published && e.StartAt >= now,
            cancellationToken);
        var confirmedBookings = await events
            .SelectMany(e => e.Bookings)
            .CountAsync(b => b.Status == BookingStatus.Confirmed, cancellationToken);

        var upcoming = await events
            .Where(e => e.Status == EventStatus.Published && e.StartAt >= now)
            .OrderBy(e => e.StartAt)
            .Take(5)
            .Select(e => new EventSummaryDto(
                e.Id,
                e.Title,
                e.Slug,
                e.Category,
                e.StartAt,
                e.EndAt,
                e.VenueName,
                e.Capacity,
                e.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                e.PriceAmount,
                e.Currency,
                e.Status,
                e.CoverImageUrl,
                e.OrganizationId))
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto(total, published, draft, upcomingCount, confirmedBookings, upcoming);
    }
}
