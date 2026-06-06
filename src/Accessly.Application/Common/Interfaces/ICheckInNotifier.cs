using Accessly.Application.Features.CheckIns;

namespace Accessly.Application.Common.Interfaces;

/// <summary>Broadcasts check-in updates to connected dashboards (implemented with SignalR).</summary>
public interface ICheckInNotifier
{
    Task CheckInRecordedAsync(Guid eventId, CheckInSummaryDto summary, CheckInDto checkIn, CancellationToken cancellationToken = default);
}
