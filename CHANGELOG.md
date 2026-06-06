# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/LMouhssine/Accessly/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/LMouhssine/Accessly/releases/tag/v0.1.0
