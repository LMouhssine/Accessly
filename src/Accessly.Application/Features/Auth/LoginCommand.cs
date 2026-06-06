using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Auth;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

public sealed record AuthenticatedUser(Guid Id, string Email, string DisplayName, UserRole Role, Guid? OrganizationId);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, AuthenticatedUser User);

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}

public sealed class LoginHandler(IAppDbContext db, IPasswordHasher hasher, IJwtTokenService tokens)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !hasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = tokens.CreateToken(user);
        return new LoginResponse(
            token.AccessToken,
            token.ExpiresAt,
            new AuthenticatedUser(user.Id, user.Email, user.DisplayName, user.Role, user.OrganizationId));
    }
}
