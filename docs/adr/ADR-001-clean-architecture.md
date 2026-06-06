# ADR-001: Clean Architecture

## Status

Accepted

## Context

Accessly spans several concerns — HTTP APIs, real-time messaging, background jobs,
persistence and external providers (payments, email, assistant). We want business rules to
be independent of frameworks and infrastructure so the system stays testable and adaptable as
technology choices evolve.

## Decision

Adopt Clean Architecture with four conceptual layers mapped to projects:

- `Accessly.Domain` — entities, enums and invariants, with no external dependencies.
- `Accessly.Application` — use cases, DTOs, validation and *ports* (interfaces), depending
  only on the domain.
- `Accessly.Infrastructure` — adapters implementing the ports (EF Core, providers, messaging).
- `Accessly.Api` / `Accessly.Worker` — presentation and hosting.

The dependency rule (dependencies point inward) is enforced by architecture tests.

## Consequences

- Business logic is unit-testable without a database or web host.
- Infrastructure can be swapped (e.g. a different message broker) without touching the domain.
- There is some indirection (ports/adapters) and more projects to maintain.

## Alternatives considered

- **Single-project layered API** — faster to start but couples business rules to the
  framework and makes boundaries easy to erode.
- **Vertical slice only** — good for feature cohesion; we still use feature folders inside
  the application layer, but keep explicit layer boundaries for testability.
