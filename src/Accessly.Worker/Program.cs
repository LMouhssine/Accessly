using Accessly.Application;
using Accessly.Infrastructure;
using Accessly.Infrastructure.Messaging;
using Accessly.Worker;
using Hangfire;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

builder.Services.AddHostedService<RecurringJobsRegistration>();

// Consume integration events from RabbitMQ when a broker is configured.
if (builder.Configuration.GetValue($"{RabbitMqOptions.SectionName}:Enabled", false))
{
    builder.Services.AddHostedService<RabbitMqConsumer>();
}

var host = builder.Build();
host.Run();
