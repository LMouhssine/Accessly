using Accessly.Application.Common.Exceptions;
using Accessly.Application.Features.Bookings;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Accessly.Infrastructure.Payments;
using Accessly.UnitTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Accessly.UnitTests.Features;

public class CreateBookingHandlerTests
{
    [Fact]
    public async Task Throws_conflict_when_event_is_full()
    {
        await using var db = TestDb.NewInMemory();
        var organization = new Organization { Name = "Org", Slug = "org" };
        var attendee = new User { Email = "a@accessly.local", DisplayName = "A", Role = UserRole.Attendee, PasswordHash = "x" };
        var existing = new User { Email = "b@accessly.local", DisplayName = "B", Role = UserRole.Attendee, PasswordHash = "x" };
        var @event = NewPublishedEvent(organization, capacity: 1);
        db.AddRange(organization, attendee, existing, @event);
        db.Bookings.Add(new Booking { Event = @event, AttendeeUser = existing, Status = BookingStatus.Confirmed });
        await db.SaveChangesAsync();

        var handler = new CreateBookingHandler(
            db, new FakeCurrentUser(attendee.Id, UserRole.Attendee, null), new FixedClock(), new FakePaymentProvider(), new NoOpAuditLogger());

        var act = () => handler.Handle(new CreateBookingCommand(@event.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Confirms_booking_and_issues_ticket()
    {
        await using var db = TestDb.NewInMemory();
        var organization = new Organization { Name = "Org", Slug = "org" };
        var attendee = new User { Email = "a@accessly.local", DisplayName = "A", Role = UserRole.Attendee, PasswordHash = "x" };
        var @event = NewPublishedEvent(organization, capacity: 5);
        db.AddRange(organization, attendee, @event);
        await db.SaveChangesAsync();

        var handler = new CreateBookingHandler(
            db, new FakeCurrentUser(attendee.Id, UserRole.Attendee, null), new FixedClock(), new FakePaymentProvider(), new NoOpAuditLogger());

        var result = await handler.Handle(new CreateBookingCommand(@event.Id), CancellationToken.None);

        result.Status.Should().Be(BookingStatus.Confirmed);
        result.TicketCode.Should().NotBeNullOrEmpty();
        (await db.Tickets.CountAsync()).Should().Be(1);
        (await db.Payments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Rejects_duplicate_active_booking()
    {
        await using var db = TestDb.NewInMemory();
        var organization = new Organization { Name = "Org", Slug = "org" };
        var attendee = new User { Email = "a@accessly.local", DisplayName = "A", Role = UserRole.Attendee, PasswordHash = "x" };
        var @event = NewPublishedEvent(organization, capacity: 10);
        db.AddRange(organization, attendee, @event);
        db.Bookings.Add(new Booking { Event = @event, AttendeeUser = attendee, Status = BookingStatus.Confirmed });
        await db.SaveChangesAsync();

        var handler = new CreateBookingHandler(
            db, new FakeCurrentUser(attendee.Id, UserRole.Attendee, null), new FixedClock(), new FakePaymentProvider(), new NoOpAuditLogger());

        var act = () => handler.Handle(new CreateBookingCommand(@event.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    private static Event NewPublishedEvent(Organization organization, int capacity) => new()
    {
        Organization = organization,
        Title = "Event",
        Slug = "event",
        Description = "Description",
        VenueName = "Venue",
        Capacity = capacity,
        PriceAmount = 10m,
        Currency = "EUR",
        Status = EventStatus.Published,
        StartAt = DateTimeOffset.UtcNow.AddDays(1),
        EndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
    };
}
