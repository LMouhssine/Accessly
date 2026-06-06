using Accessly.Api.Hubs;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Features.CheckIns;
using Microsoft.AspNetCore.SignalR;

namespace Accessly.Api.Infrastructure;

/// <summary>Broadcasts check-in updates over SignalR to dashboards watching an event.</summary>
public sealed class CheckInNotifier(IHubContext<CheckInsHub> hub) : ICheckInNotifier
{
    public Task CheckInRecordedAsync(Guid eventId, CheckInSummaryDto summary, CheckInDto checkIn, CancellationToken cancellationToken = default)
        => hub.Clients.Group(CheckInsHub.GroupName(eventId))
            .SendAsync("CheckInRecorded", new { summary, checkIn }, cancellationToken);
}
