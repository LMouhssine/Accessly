# Architecture Decision Records

This directory records the significant architectural decisions made for Accessly. Each ADR
captures the context, the decision and its consequences. ADRs are immutable once accepted; if
a decision changes, a new ADR supersedes the old one.

| ADR | Title | Status |
| --- | --- | --- |
| [001](ADR-001-clean-architecture.md) | Clean Architecture | Accepted |
| [002](ADR-002-cqrs-application-layer.md) | CQRS application layer | Accepted |
| [003](ADR-003-sql-server-entity-framework.md) | SQL Server with Entity Framework Core | Accepted |
| [004](ADR-004-signalr-realtime-checkin.md) | SignalR for real-time check-in | Accepted |
| [005](ADR-005-hangfire-background-jobs.md) | Hangfire for background jobs | Accepted |
| [006](ADR-006-fake-payment-provider.md) | Fake payment provider | Accepted |
| [007](ADR-007-ai-provider-abstraction.md) | AI provider abstraction | Accepted |
| [008](ADR-008-github-actions-ci-cd.md) | GitHub Actions for CI/CD | Accepted |
| [009](ADR-009-azure-ready-infrastructure.md) | Azure-ready infrastructure | Accepted |

## Format

Each ADR follows a short template: **Status**, **Context**, **Decision**,
**Consequences** and **Alternatives considered**.
