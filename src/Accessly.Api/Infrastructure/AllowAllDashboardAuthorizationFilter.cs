using Hangfire.Dashboard;

namespace Accessly.Api.Infrastructure;

/// <summary>
/// Permissive Hangfire dashboard filter for the demonstration environment. In a real
/// deployment this should be replaced with proper authentication.
/// </summary>
public sealed class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
