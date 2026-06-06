using Accessly.Application.Common.Messaging;

namespace Accessly.Application.Common.Events;

public sealed record BookingConfirmedIntegrationEvent(Guid BookingId, Guid EventId, Guid AttendeeUserId) : IIntegrationEvent;

public sealed record EventCancelledIntegrationEvent(Guid EventId) : IIntegrationEvent;
