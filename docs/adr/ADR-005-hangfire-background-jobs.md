# ADR-005: Hangfire for background jobs

## Status

Accepted

## Context

Several operations should not run inline with an HTTP request: sending confirmation
messages, scheduling 24-hour reminders, notifying attendees of changes or cancellations and
post-event follow-ups. These need scheduling, retries and visibility.

## Decision

Use **Hangfire** for background processing, hosted in `Accessly.Worker`, with **SQL Server**
storage so jobs are durable across restarts. Use fire-and-forget jobs for immediate work,
scheduled jobs for reminders and recurring jobs for periodic tasks. The Hangfire dashboard is
available for local inspection.

## Consequences

- Reliable, observable background work with built-in retries.
- Jobs survive restarts because state is persisted in SQL Server.
- Hangfire is dual-licensed (LGPL / commercial). We use it unmodified under the LGPL, which is
  appropriate for this usage.

## Alternatives considered

- **`IHostedService` / `BackgroundService`** — fine for simple loops but lacks scheduling,
  retries, persistence and a dashboard.
- **Quartz.NET** — capable scheduler, but Hangfire's dashboard and storage model fit this
  project's needs with less setup.
