# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2026-06-07

### Added

- Audit-log read API: `GET /api/audit-logs` with simple filtering (action, entity type,
  actor, date range), newest-first ordering, pagination and RBAC scoping (organizers see
  their own organization, admins see all).
- Dashboard pages: a filterable, paginated audit-log viewer, an attendees roll-up, a
  per-event feedback view with an optional generated summary, and a profile/workspace
  settings page, each with loading, empty and error states.
- Distributed cache (`ICacheService`): Redis when configured, in-process otherwise, backing
  a new cached `GET /api/dashboard/summary` aggregate consumed by the overview page.
- Per-IP rate limiting with a stricter policy on the login endpoint.
- Local demo script (`docs/product/demo-script.md`) and README screenshots.
- Frontend unit tests now run in CI.

### Fixed

- Committed `LocalFileStorage` source that an over-broad `storage/` gitignore rule hid on
  case-insensitive filesystems, which had broken the backend build and container images on CI.
- Pointed the Makefile at the real `Accessly.slnx` solution (was `Accessly.sln`).
- Corrected the Trivy action tag and supplied the `GITHUB_TOKEN` gitleaks now requires.
- Stripped a UTF-8 BOM from the generated migrations so `make lint` passes.

## [0.1.0] - 2026-06-06

### Added

- Repository scaffold, documentation, ADRs, threat model and community health files.
- .NET 10 backend with Clean Architecture and an in-house CQRS dispatcher.
- Organizations, JWT auth with role-based authorization, events (CRUD, publish/cancel,
  speakers, cover images), bookings, QR code tickets and a fake payment provider.
- Audit logging across significant actions.
- Real-time check-in over SignalR, an event bus (in-process or RabbitMQ) and Hangfire jobs.
- Fake email notifications persisted to the database.
- Event assistant with a deterministic offline provider and attendee feedback.
- Angular + Angular Material frontend: public catalog and booking flow plus an organizer dashboard.
- Dockerized local stack (SQL Server, Redis, RabbitMQ, API, worker, web, Prometheus, Grafana).
- CI/CD workflows, Azure-ready Terraform, and OpenTelemetry metrics exposed to Prometheus.
- Unit, architecture and integration test suites.

[Unreleased]: https://github.com/LMouhssine/Accessly/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/LMouhssine/Accessly/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/LMouhssine/Accessly/releases/tag/v0.1.0
