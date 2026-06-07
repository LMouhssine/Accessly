using Accessly.Application.Common.Events;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Infrastructure.Ai;
using Accessly.Infrastructure.Auditing;
using Accessly.Infrastructure.Caching;
using Accessly.Infrastructure.Identity;
using Accessly.Infrastructure.Jobs;
using Accessly.Infrastructure.Messaging;
using Accessly.Infrastructure.Notifications;
using Accessly.Infrastructure.Payments;
using Accessly.Infrastructure.Persistence;
using Accessly.Infrastructure.QrCodes;
using Accessly.Infrastructure.Storage;
using Accessly.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        // Distributed cache: Redis when enabled and configured, otherwise an in-process store
        // so the app runs without an external cache.
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        var redis = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new RedisOptions();
        if (redis.Enabled && !string.IsNullOrWhiteSpace(redis.Connection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redis.Connection;
                options.InstanceName = redis.InstanceName;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, DistributedCacheService>();

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

        // Notifications and messaging.
        services.AddSingleton<IEmailSender, FakeEmailSender>();
        services.AddScoped<NotificationJobs>();
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IIntegrationEventHandler<BookingConfirmedIntegrationEvent>, BookingConfirmedNotificationHandler>();
        services.AddScoped<IIntegrationEventHandler<EventCancelledIntegrationEvent>, EventCancelledNotificationHandler>();

        var rabbit = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        if (rabbit.Enabled)
        {
            services.AddSingleton<IEventBus, RabbitMqEventBus>();
        }
        else
        {
            services.AddScoped<IEventBus, InMemoryEventBus>();
        }

        // AI assistant (deterministic Fake by default; OpenAI-compatible only when fully configured).
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        var ai = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();
        if (string.Equals(ai.Provider, "OpenAiCompatible", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(ai.BaseUrl)
            && !string.IsNullOrWhiteSpace(ai.ApiKey))
        {
            services.AddHttpClient<IAiProvider, OpenAiCompatibleProvider>((sp, client) =>
            {
                var aiOptions = sp.GetRequiredService<IOptions<AiOptions>>().Value;
                client.BaseAddress = new Uri(aiOptions.BaseUrl!.TrimEnd('/') + "/");
            });
        }
        else
        {
            services.AddSingleton<IAiProvider, FakeAiProvider>();
        }

        return services;
    }
}
