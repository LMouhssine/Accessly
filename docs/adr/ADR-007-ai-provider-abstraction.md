# ADR-007: AI provider abstraction

## Status

Accepted

## Context

An assistant that drafts event descriptions, suggests tags, writes reminder emails, proposes
agendas and summarizes feedback adds product value. However, the platform must run fully
without any external AI service or API key, and demos must be reproducible.

## Decision

Define an `IAiProvider` port and an `AiEventAssistantService` in the application layer. Ship a
**deterministic `FakeAiProvider`** as the default, producing stable, sensible output for each
capability. Provide an optional `OpenAiCompatibleProvider` that is **disabled by default** and
only activates when explicitly configured via environment variables.

## Consequences

- The platform boots and demos work offline, with no secrets required.
- Tests can assert on deterministic assistant output.
- Switching to a real provider is configuration-only; no code changes in callers.

## Alternatives considered

- **Hard dependency on an external API** — breaks the offline/no-secrets requirement and
  makes demos non-deterministic.
- **No assistant** — removes a differentiating feature that the abstraction lets us offer
  safely.
