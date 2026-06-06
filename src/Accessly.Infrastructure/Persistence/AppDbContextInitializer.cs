using Accessly.Domain.Common;
using Accessly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Accessly.Infrastructure.Persistence;

/// <summary>Applies pending migrations and seeds baseline demo data (idempotent).</summary>
public sealed class AppDbContextInitializer(AppDbContext context, ILogger<AppDbContextInitializer> logger)
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await context.Organizations.AnyAsync(cancellationToken))
            {
                var organization = new Organization
                {
                    Name = "Demo Organization",
                    Slug = SlugGenerator.Generate("Demo Organization"),
                };
                context.Organizations.Add(organization);
                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Seeded demo organization {OrganizationId}.", organization.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}

/// <summary>Convenience helpers to run database initialization at host startup.</summary>
public static class AppDbContextInitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, bool seed, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
        await initializer.MigrateAsync(cancellationToken);
        if (seed)
        {
            await initializer.SeedAsync(cancellationToken);
        }
    }
}
