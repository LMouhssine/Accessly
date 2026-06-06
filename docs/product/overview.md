# Product overview

Accessly is an event access and ticketing platform for organizers who run events such as
conferences, meetups and workshops. It covers the lifecycle from publishing an event to
checking attendees in at the door, with a modern web dashboard.

> Ticketing, payments and email delivery are intentionally fictional/simulated. Accessly is
> a demonstration platform and is not intended to process real payments or personal data.

## Personas

| Persona | Goal |
| --- | --- |
| **Admin** | Operate the platform; manage organizations, users and roles. |
| **Organizer** | Create and run events, manage bookings, monitor live check-in. |
| **Staff** | Validate tickets at the entrance and check attendees in. |
| **Attendee** | Discover events, book a place and access QR code tickets. |

## Roles and permissions

| Capability | Admin | Organizer | Staff | Attendee |
| --- | :---: | :---: | :---: | :---: |
| Manage all organizations & users | ✅ | — | — | — |
| Create / edit / publish events | ✅ | ✅ (own org) | — | — |
| Cancel events | ✅ | ✅ (own org) | — | — |
| View bookings & attendees | ✅ | ✅ (own org) | ✅ (assigned) | — |
| Check in tickets | ✅ | ✅ | ✅ | — |
| Book a place / view own tickets | ✅ | ✅ | ✅ | ✅ |
| Leave feedback | ✅ | ✅ | ✅ | ✅ |
| View audit logs | ✅ | ✅ (own org) | — | — |

## Core flows

1. **Publish an event** — an organizer drafts an event (title, schedule, venue, capacity,
   fictional price, speakers, cover image), then publishes it.
2. **Book a place** — an attendee books a published event; capacity is enforced and a
   confirmed booking produces a ticket with a QR code.
3. **Check in** — staff scan or enter a ticket code at the door; valid tickets are accepted
   once, invalid or already-used tickets are rejected, and the organizer dashboard updates
   live.
4. **Follow up** — after the event, attendees leave feedback and the assistant can summarize
   it for the organizer.

## Lifecycle states

- **Event** — `DRAFT` → `PUBLISHED` → (`CANCELLED` | `COMPLETED`)
- **Booking** — `PENDING` → `CONFIRMED` → (`CANCELLED` | `EXPIRED`)
- **Ticket** — `ACTIVE` → (`USED` | `CANCELLED`)

## Assistant

An optional assistant helps organizers draft event descriptions, suggest tags, write
reminder emails, propose agendas and summarize feedback. It runs through a provider
abstraction with a deterministic offline default, so the platform works fully without any
external service or API key. See
[ADR-007](../adr/ADR-007-ai-provider-abstraction.md).
