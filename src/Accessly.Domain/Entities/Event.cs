using Accessly.Domain.Common;
using Accessly.Domain.Enums;

namespace Accessly.Domain.Entities;

/// <summary>An event that attendees can book and check in to.</summary>
public class Event : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }

    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }

    public string VenueName { get; set; } = string.Empty;
    public string? VenueAddress { get; set; }

    public int Capacity { get; set; }

    /// <summary>Fictional ticket price; payments are simulated.</summary>
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "EUR";

    public EventStatus Status { get; set; } = EventStatus.Draft;
    public string? CoverImageUrl { get; set; }

    public ICollection<Speaker> Speakers { get; set; } = new List<Speaker>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
