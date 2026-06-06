using Accessly.Application.Common.Interfaces;
using Accessly.Infrastructure.Auditing;
using Accessly.Infrastructure.Identity;
using Accessly.Infrastructure.Payments;
using Accessly.Infrastructure.Persistence;
using Accessly.Infrastructure.QrCodes;
using Accessly.Infrastructure.Storage;
using Accessly.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accessly.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure();
            }));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<AppDbContextInitializer>();
        services.AddSingleton<IClock, SystemClock>();

        // Identity and authentication.
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Domain services.
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        // Payment provider (Fake by default; Stripe test-mode is a disabled placeholder).
        var paymentProvider = configuration["Payments:Provider"] ?? "Fake";
        if (string.Equals(paymentProvider, "StripeTest", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IPaymentProvider, StripeTestPaymentProvider>();
        }
        else
        {
            services.AddSingleton<IPaymentProvider, FakePaymentProvider>();
        }

        return services;
    }
}
