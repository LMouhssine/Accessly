using Accessly.Application.Common.Interfaces;

namespace Accessly.Infrastructure.Payments;

/// <summary>
/// Placeholder for a future Stripe test-mode integration. It is never enabled by default and
/// never depends on real keys; selecting it without an implementation fails fast with a clear message.
/// </summary>
public sealed class StripeTestPaymentProvider : IPaymentProvider
{
    public string Name => "StripeTest";

    public Task<PaymentResult> ChargeAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "The Stripe test-mode provider is a placeholder and is not implemented. Use the Fake provider for demonstrations.");
}
