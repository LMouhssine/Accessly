using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Domain.Enums;

namespace Accessly.Application.Common;

/// <summary>Authorization helpers enforcing tenant boundaries inside handlers.</summary>
public static class AccessGuard
{
    public static void EnsureAuthenticated(ICurrentUser user)
    {
        if (!user.IsAuthenticated || user.UserId is null)
        {
            throw new UnauthorizedException();
        }
    }

    /// <summary>Admins can manage any organization; organizers only their own.</summary>
    public static void EnsureCanManageOrganization(ICurrentUser user, Guid organizationId)
    {
        EnsureAuthenticated(user);
        if (user.Role == UserRole.Admin)
        {
            return;
        }

        if (user.Role == UserRole.Organizer && user.OrganizationId == organizationId)
        {
            return;
        }

        throw new ForbiddenAccessException();
    }

    /// <summary>Admins, plus organizers and staff of the organization, may operate an event (e.g. check-in).</summary>
    public static void EnsureCanOperateEvent(ICurrentUser user, Guid organizationId)
    {
        EnsureAuthenticated(user);
        if (user.Role == UserRole.Admin)
        {
            return;
        }

        if (user.Role is UserRole.Organizer or UserRole.Staff && user.OrganizationId == organizationId)
        {
            return;
        }

        throw new ForbiddenAccessException();
    }

    public static Guid ResolveOrganizationId(ICurrentUser user, Guid? requested)
    {
        EnsureAuthenticated(user);
        if (user.Role == UserRole.Admin)
        {
            return requested ?? user.OrganizationId
                ?? throw new ForbiddenAccessException("An organization must be specified.");
        }

        var organizationId = user.OrganizationId
            ?? throw new ForbiddenAccessException("The current user is not attached to an organization.");

        if (requested is not null && requested != organizationId)
        {
            throw new ForbiddenAccessException();
        }

        return organizationId;
    }
}
