namespace Accessly.Application.Common;

/// <summary>Canonical action names used in the audit log.</summary>
public static class AuditActions
{
    public const string OrganizationCreated = "organization.created";
    public const string EventCreated = "event.created";
    public const string EventUpdated = "event.updated";
    public const string EventPublished = "event.published";
    public const string EventUnpublished = "event.unpublished";
    public const string EventCancelled = "event.cancelled";
    public const string BookingCreated = "booking.created";
    public const string BookingCancelled = "booking.cancelled";
    public const string CheckInRecorded = "checkin.recorded";
    public const string RoleChanged = "role.changed";
    public const string AiGenerated = "ai.generated";
}
