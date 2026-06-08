using Accessly.Application.Features.Dashboard;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Accessly.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace Accessly.UnitTests.Features;

public class GetDashboardSummaryHandlerTests
{
    [Fact]
    public async Task Organizer_summary_is_scoped_to_their_organization()
    {
        await using var db = TestDb.NewInMemory();
        var org = new Organization { Name = "Org", Slug = "org" };
        var otherOrg = new Organization { Name = "Other", Slug = "other" };
        db.AddRange(org, otherOrg);
        db.Events.Add(NewEvent(org, EventStatus.Published, daysFromNow: 2));
        db.Events.Add(NewEvent(org, EventStatus.Draft, daysFromNow: 5));
        db.Events.Add(NewEvent(otherOrg, EventStatus.Published, daysFromNow: 3));
        await db.SaveChangesAsync();

        var handler = new GetDashboardSummaryHandler(
            db, new FakeCurrentUser(Guid.NewGuid(), UserRole.Organizer, org.Id), new FixedClock(), new PassThroughCache());

        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        result.TotalEvents.Should().Be(2);
        result.PublishedEvents.Should().Be(1);
        result.DraftEvents.Should().Be(1);
        result.UpcomingEvents.Should().Be(1);
        result.Upcoming.Should().ContainSingle();
    }

    [Fact]
    public async Task Admin_summary_spans_all_organizations()
    {
        await using var db = TestDb.NewInMemory();
        var org = new Organization { Name = "Org", Slug = "org" };
        var otherOrg = new Organization { Name = "Other", Slug = "other" };
        db.AddRange(org, otherOrg);
        db.Events.Add(NewEvent(org, EventStatus.Published, daysFromNow: 2));
        db.Events.Add(NewEvent(otherOrg, EventStatus.Published, daysFromNow: 3));
        await db.SaveChangesAsync();

        var handler = new GetDashboardSummaryHandler(
            db, new FakeCurrentUser(Guid.NewGuid(), UserRole.Admin, null), new FixedClock(), new PassThroughCache());

        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        result.TotalEvents.Should().Be(2);
        result.PublishedEvents.Should().Be(2);
    }

    private static Event NewEvent(Organization organization, EventStatus status, int daysFromNow)
    {
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(daysFromNow);
        return new Event
        {
            Organization = organization,
            Title = $"Event {Guid.NewGuid():N}",
            Slug = Guid.NewGuid().ToString("N"),
            Description = "Description",
            VenueName = "Venue",
            Capacity = 50,
            PriceAmount = 0m,
            Currency = "EUR",
            Status = status,
            StartAt = start,
            EndAt = start.AddHours(2),
        };
    }
}
