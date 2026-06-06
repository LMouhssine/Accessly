# ADR-003: SQL Server with Entity Framework Core

## Status

Accepted

## Context

Accessly needs a relational store for strongly-related data (organizations, events,
bookings, tickets, check-ins) with transactional guarantees and multi-tenant scoping.

## Decision

Use **SQL Server** as the system of record, accessed through **Entity Framework Core**
(code-first) with explicit migrations and a development data seeder. Persistence is exposed
to the application layer through an `IAppDbContext` port. Logical multi-tenancy is implemented
by storing `OrganizationId` on tenant-scoped entities and applying query filters.

## Consequences

- Transactional integrity for booking/ticket/check-in workflows.
- Schema evolves through reviewable migrations; demo data is reproducible via the seeder.
- On Apple Silicon, the SQL Server container runs under emulation (slower but functional);
  this is acceptable for local development and is documented.

## Alternatives considered

- **PostgreSQL** — excellent and cross-platform, but SQL Server is the chosen target for this
  platform and integrates cleanly with the Azure-ready infrastructure.
- **NoSQL** — not a fit for the highly relational, transactional core.
