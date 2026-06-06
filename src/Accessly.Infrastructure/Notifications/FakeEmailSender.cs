using Accessly.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Accessly.Infrastructure.Notifications;

/// <summary>Default email sender. Logs the message instead of delivering it.</summary>
public sealed class FakeEmailSender(ILogger<FakeEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[FakeEmail] To={ToAddress} Subject=\"{Subject}\"", toAddress, subject);
        return Task.CompletedTask;
    }
}
