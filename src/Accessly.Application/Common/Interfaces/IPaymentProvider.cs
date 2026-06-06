using Accessly.Domain.Enums;

namespace Accessly.Application.Common.Interfaces;

public sealed record PaymentResult(bool Succeeded, PaymentStatus Status, string Reference, string Provider);

/// <summary>Abstraction over a payment provider. Payments are simulated by default.</summary>
public interface IPaymentProvider
{
    string Name { get; }

    Task<PaymentResult> ChargeAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default);
}
