using Accessly.Application.Common.Interfaces;
using Accessly.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Accessly.Infrastructure.Identity;

/// <summary>Wraps ASP.NET Core's <see cref="PasswordHasher{TUser}"/> behind the application port.</summary>
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private static readonly User Placeholder = new();
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(Placeholder, password);

    public bool Verify(string hash, string password)
        => _hasher.VerifyHashedPassword(Placeholder, hash, password) != PasswordVerificationResult.Failed;
}
