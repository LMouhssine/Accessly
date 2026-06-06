using Accessly.Domain.Common;
using Accessly.Domain.Enums;

namespace Accessly.Domain.Entities;

/// <summary>A simulated payment attached to a booking. No real money movement occurs.</summary>
public class Payment : AuditableEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public string Provider { get; set; } = "Fake";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";

    public string? Reference { get; set; }
}
