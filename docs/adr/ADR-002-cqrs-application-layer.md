# ADR-002: CQRS application layer

## Status

Accepted

## Context

Use cases differ between reads and writes: reads are shaped for the UI, writes enforce
invariants and produce side effects (tickets, notifications, audit entries). We want a
consistent way to express use cases and to attach cross-cutting concerns such as validation
and logging.

## Decision

Model the application layer with **CQRS**: each use case is a `Command` or `Query` with a
dedicated handler. Handlers are invoked through a small in-process dispatcher, with
**pipeline behaviors** wrapping every handler for validation (FluentValidation) and logging.

We implement a **lightweight in-house dispatcher** (`ICommand`/`IQuery`,
`IRequestHandler<,>`, behaviors) registered in DI, rather than taking a dependency on a
third-party mediator library. This keeps the public repository free of libraries whose
recent versions require a commercial license, while preserving the familiar request/handler
ergonomics.

## Consequences

- Each use case is small, isolated and independently testable.
- Cross-cutting concerns are applied uniformly without repetition in handlers.
- We own a small amount of mediator plumbing instead of relying on an external package.

## Alternatives considered

- **MediatR** — the established mediator library; its current major versions moved to a
  commercial license, which we prefer to avoid in a public repository. Our dispatcher mirrors
  its core abstractions so the pattern remains idiomatic.
- **Services without CQRS** — fewer types, but mixes reads/writes and makes pipeline
  behaviors harder to apply consistently.
