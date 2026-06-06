namespace Accessly.Application.Common.Interfaces;

/// <summary>Sends email messages. The default implementation simulates delivery (no real email).</summary>
public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default);
}
