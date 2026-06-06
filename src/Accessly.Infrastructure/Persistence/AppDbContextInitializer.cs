using Accessly.Application.Common.Interfaces;
using Accessly.Domain.Common;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Accessly.Infrastructure.Persistence;

/// <summary>Applies pending migrations and seeds fictional demo data (idempotent).</summary>
public sealed class AppDbContextInitializer(
    AppDbContext context,
    IPasswordHasher passwordHasher,
    IClock clock,
    ILogger<AppDbContextInitializer> logger)
{
    /// <summary>Shared password for all demo accounts (documented in the README).</summary>
    public const string DemoPassword = "Password123!";

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (await context.Organizations.AnyAsync(cancellationToken))
            {
                return;
            }

            var now = clock.UtcNow;
            var organization = new Organization { Name = "Demo Organization", Slug = "demo-organization" };
            context.Organizations.Add(organization);

            var admin = NewUser("admin@accessly.local", "Demo Admin", UserRole.Admin, null);
            var organizer = NewUser("organizer@accessly.local", "Demo Organizer", UserRole.Organizer, organization);
            var staff = NewUser("staff@accessly.local", "Demo Staff", UserRole.Staff, organization);
            var attendee = NewUser("attendee@accessly.local", "Demo Attendee", UserRole.Attendee, null);
            context.Users.AddRange(admin, organizer, staff, attendee);

            var conference = new Event
            {
                Organization = organization,
                Title = "Tech Conference 2026",
                Slug = "tech-conference-2026",
                Description = "A full-day conference covering modern software engineering, cloud and developer experience.",
                Category = "Conference",
                StartAt = now.AddDays(30),
                EndAt = now.AddDays(30).AddHours(8),
                VenueName = "Grand Hall",
                VenueAddress = "1 Market Street",
                Capacity = 200,
                PriceAmount = 49.00m,
                Currency = "EUR",
                Status = EventStatus.Published,
                Speakers =
                {
                    new Speaker { Name = "Alex Rivera", Title = "Principal Engineer", Bio = "Speaks on distributed systems." },
                    new Speaker { Name = "Jordan Lee", Title = "Developer Advocate", Bio = "Focuses on developer experience." },
                },
            };

            var meetup = new Event
            {
                Organization = organization,
                Title = "Cloud Native Meetup",
                Slug = "cloud-native-meetup",
                Description = "An evening meetup about containers, orchestration and observability.",
                Category = "Meetup",
                StartAt = now.AddDays(14),
                EndAt = now.AddDays(14).AddHours(3),
                VenueName = "Innovation Lab",
                VenueAddress = "22 Riverside Avenue",
                Capacity = 60,
                PriceAmount = 0m,
                Currency = "EUR",
                Status = EventStatus.Published,
                Speakers = { new Speaker { Name = "Sam Okafor", Title = "SRE Lead" } },
            };

            var workshop = new Event
            {
                Organization = organization,
                Title = "Internal Planning Workshop",
                Slug = "internal-planning-workshop",
                Description = "A draft workshop used to plan the next quarter.",
                Category = "Workshop",
                StartAt = now.AddDays(45),
                EndAt = now.AddDays(45).AddHours(4),
                VenueName = "Meeting Room A",
                Capacity = 25,
                PriceAmount = 0m,
                Currency = "EUR",
                Status = EventStatus.Draft,
            };

            context.Events.AddRange(conference, meetup, workshop);

            var booking = new Booking
            {
                Event = conference,
                AttendeeUser = attendee,
                Status = BookingStatus.Confirmed,
                Payment = new Payment
                {
                    Provider = "Fake",
                    Status = PaymentStatus.Succeeded,
                    Amount = conference.PriceAmount,
                    Currency = conference.Currency,
                    Reference = "fake_seed",
                },
                Ticket = new Ticket
                {
                    EventId = conference.Id,
                    Code = CodeGenerator.Generate(),
                    Status = TicketStatus.Active,
                    IssuedAt = now,
                },
            };
            booking.Ticket!.QrPayload = booking.Ticket.Code;
            context.Bookings.Add(booking);

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded demo organization, users and events.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private User NewUser(string email, string displayName, UserRole role, Organization? organization) => new()
    {
        Email = email,
        DisplayName = displayName,
        Role = role,
        Organization = organization,
        PasswordHash = passwordHasher.Hash(DemoPassword),
    };
}

/// <summary>Convenience helpers to run database initialization at host startup.</summary>
public static class AppDbContextInitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, bool seed, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
        await initializer.MigrateAsync(cancellationToken);
        if (seed)
        {
            await initializer.SeedAsync(cancellationToken);
        }
    }
}
