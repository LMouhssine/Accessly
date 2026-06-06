# ADR-004: SignalR for real-time check-in

## Status

Accepted

## Context

During an event, organizers need a live view of attendance: how many people are registered,
how many have checked in, the fill rate and the latest check-ins. Polling the API would be
wasteful and laggy.

## Decision

Use **ASP.NET Core SignalR** with a hub at `/hubs/checkins`. When a check-in is recorded, the
server broadcasts updated counts and the latest entries to all dashboards subscribed to that
event's group. For multi-instance deployments, a **Redis backplane** can be enabled via
configuration without code changes.

## Consequences

- Dashboards update instantly with minimal traffic.
- The frontend uses the SignalR client and degrades gracefully if the socket drops
  (reconnection plus an initial summary fetch).
- Real-time state must be derived from persisted data so a reconnecting client can resync.

## Alternatives considered

- **Polling** — simplest, but higher latency and load.
- **Raw WebSockets / SSE** — more manual work; SignalR provides grouping, reconnection and a
  typed client out of the box.
