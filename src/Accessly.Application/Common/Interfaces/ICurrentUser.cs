using Accessly.Domain.Enums;

namespace Accessly.Application.Common.Interfaces;

/// <summary>Information about the authenticated caller for the current request.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? OrganizationId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
