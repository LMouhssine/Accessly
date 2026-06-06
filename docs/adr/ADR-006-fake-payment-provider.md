# ADR-006: Fake payment provider

## Status

Accepted

## Context

Bookings carry a price, but Accessly is a demonstration platform that must not process real
payments or depend on real payment credentials. We still want a realistic payment step in the
booking flow and a clear path to integrate a real provider later.

## Decision

Define an `IPaymentProvider` port in the application layer. Implement a
**`FakePaymentProvider`** as the default, which deterministically "authorizes" payments
without any external call. Leave room for a future, optional `StripeTestPaymentProvider` that
would only ever use test-mode keys supplied via configuration and is disabled by default.

## Consequences

- The booking flow exercises a payment step end to end with no external dependency or secret.
- Adding a real test-mode provider later is a matter of implementing the port and enabling it
  through configuration.
- No real money movement is possible; this is explicit and intended.

## Alternatives considered

- **No payment step** — simpler, but omits a meaningful part of the booking flow.
- **Direct Stripe integration** — requires keys and live/test setup that conflicts with the
  goal of a self-contained, secret-free demo.
