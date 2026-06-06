using Accessly.Application.Common.Interfaces;
using Accessly.Domain.Enums;

namespace Accessly.Infrastructure.Payments;

/// <summary>Default payment provider. Authorizes every charge deterministically; no money moves.</summary>
public sealed class FakePaymentProvider : IPaymentProvider
{
    public string Name => "Fake";

    public Task<PaymentResult> ChargeAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default)
    {
        var reference = $"fake_{Guid.NewGuid():N}";
        return Task.FromResult(new PaymentResult(true, PaymentStatus.Succeeded, reference, Name));
    }
}
