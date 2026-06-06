using Accessly.Domain.Entities;

namespace Accessly.Application.Common.Interfaces;

public sealed record TokenResult(string AccessToken, DateTimeOffset ExpiresAt);

/// <summary>Issues signed JWT access tokens for authenticated users.</summary>
public interface IJwtTokenService
{
    TokenResult CreateToken(User user);
}
