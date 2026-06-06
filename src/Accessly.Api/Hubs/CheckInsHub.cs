using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Accessly.Api.Hubs;

/// <summary>Real-time hub that pushes check-in updates to organizer dashboards.</summary>
[Authorize(Roles = Roles.StaffOrAbove)]
public sealed class CheckInsHub : Hub
{
    public Task JoinEvent(Guid eventId) => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(eventId));

    public Task LeaveEvent(Guid eventId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(eventId));

    public static string GroupName(Guid eventId) => $"event-{eventId}";
}
