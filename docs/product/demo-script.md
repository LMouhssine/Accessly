# Accessly — local demo script

This walkthrough exercises the main attendee and organizer flows end to end. It assumes the
full stack is running locally (see [Run the stack](#run-the-stack)) and that the demo data
seeder has populated the database.

## Run the stack

```bash
cp .env.example .env
make docker-up        # builds and starts SQL Server, Redis, RabbitMQ, API, worker, web, Prometheus, Grafana
```

Wait for the API health check to report healthy:

```bash
curl -s http://localhost:8080/api/health
```

> **Apple Silicon note.** The SQL Server image is `linux/amd64` and runs under emulation on
> Apple Silicon, so the database container takes longer to become healthy on first start.
> This is expected and does not indicate a failure — give it a minute. An arm64-native
> alternative is documented in the README under *Local demo*.

## Service URLs

| Service | URL |
| --- | --- |
| Web dashboard | <http://localhost:4200> |
| API + Swagger | <http://localhost:8080/swagger> |
| Health | <http://localhost:8080/api/health> |
| Metrics | <http://localhost:8080/metrics> |
| Hangfire dashboard | <http://localhost:8080/hangfire> |
| RabbitMQ management | <http://localhost:15672> |
| Prometheus | <http://localhost:9090> |
| Grafana | <http://localhost:3000> |

## Demo accounts

All demo accounts use the password `Password123!`:

| Email | Role |
| --- | --- |
| `admin@accessly.local` | Admin |
| `organizer@accessly.local` | Organizer |
| `staff@accessly.local` | Staff |
| `attendee@accessly.local` | Attendee |

## Recommended demo scenario

### 1. Browse and book as an attendee

1. Open <http://localhost:4200> and visit **Events** to browse the published catalog.
2. Open an event to see its detail page (venue, schedule, speakers, capacity).
3. Sign in as `attendee@accessly.local`, then book the event. A confirmed booking issues a
   ticket with a QR code, visible under **My tickets**.

### 2. Manage events as an organizer

1. Sign out and sign in as `organizer@accessly.local`. You land on the dashboard **Overview**,
   which shows cached aggregate counts (events, drafts, confirmed bookings) and upcoming events.
2. Go to **Events → New event**, fill in the form (optionally use the assistant to draft a
   description and tags), and save it as a draft.
3. Publish the event. It now appears in the public catalog.

### 3. Live check-in as staff

1. Open the organizer dashboard **Live check-in** page for an event.
2. Enter (or scan) the ticket code from the attendee's ticket and submit.
3. The attendance count, fill rate and live feed update in real time over SignalR. Submitting
   the same code again is rejected as already checked in.

### 4. Review activity

1. **Attendees** — see everyone who booked across your events, aggregated per person.
2. **Feedback** — pick an event to review ratings and comments, and generate a summary.
3. **Notifications** — confirmation and reminder messages are recorded here (emails are
   simulated by default).
4. **Audit logs** — every significant action (event published, booking created, check-in
   recorded, assistant generation) is recorded with its actor and timestamp, filterable by
   action and paginated.

## Scripted smoke test (API only)

The following sequence drives the core flow without the UI, using the seeded data:

```bash
BASE=http://localhost:8080

# Log in as the organizer and capture the token
TOKEN=$(curl -s -X POST $BASE/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"organizer@accessly.local","password":"Password123!"}' | jq -r .accessToken)

# Dashboard summary (cached) and audit logs are organizer/admin only
curl -s $BASE/api/dashboard/summary -H "Authorization: Bearer $TOKEN" | jq
curl -s "$BASE/api/audit-logs?pageSize=5" -H "Authorization: Bearer $TOKEN" | jq '.items[].action'

# Public catalog
curl -s "$BASE/api/events?pageSize=3" | jq '.items[].title'
```
