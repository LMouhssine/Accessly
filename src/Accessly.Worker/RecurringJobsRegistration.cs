using Accessly.Infrastructure.Jobs;
using Hangfire;

namespace Accessly.Worker;

/// <summary>Registers the recurring Hangfire jobs when the worker starts.</summary>
public sealed class RecurringJobsRegistration(IRecurringJobManager recurringJobs) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobs.AddOrUpdate<NotificationJobs>(
            "send-due-reminders",
            job => job.SendDueRemindersAsync(),
            Cron.Hourly());

        recurringJobs.AddOrUpdate<NotificationJobs>(
            "complete-past-events",
            job => job.CompletePastEventsAsync(),
            Cron.Hourly());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
