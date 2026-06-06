# Observability

Accessly is instrumented so its behaviour can be understood in development and in any hosted
environment, using open standards (OpenTelemetry, Prometheus) and structured logging.

## Logging

- Structured logs via Serilog, written to the console as JSON in non-development
  environments and human-readable in development.
- Every request is tagged with a **correlation id**. If the incoming request carries an
  `X-Correlation-ID` header it is reused; otherwise one is generated and returned on the
  response, so a single request can be traced across the API and Worker.
- Log levels are configurable through environment variables.

## Metrics

- The API exposes a Prometheus-compatible endpoint at `/metrics` (OpenTelemetry Prometheus
  exporter).
- Built-in ASP.NET Core and runtime metrics (request rate, duration, error rate, GC) are
  collected, alongside selected application metrics (for example check-ins processed).
- Prometheus scrape configuration and a Grafana dashboard are provided under
  [infra/observability](../../infra/observability).

## Tracing

- OpenTelemetry tracing instruments incoming HTTP requests, EF Core commands and outgoing
  HTTP/messaging calls.
- Traces can be exported to an OTLP-compatible collector when `OTEL_EXPORTER_OTLP_ENDPOINT`
  is configured. Tracing is safe to leave unconfigured locally.

## Health checks

- `GET /api/health` returns overall health; readiness includes dependency checks
  (database, Redis, broker) so orchestrators can gate traffic appropriately.

## Hosted environments

For Azure, the same OpenTelemetry instrumentation can be exported to Application Insights by
providing a connection string; no code changes are required. See
[infra/azure](../../infra/azure) for deployment notes.

## Local visualization

`make docker-up` starts Prometheus and Grafana alongside the application. Grafana is
provisioned with the Prometheus data source and an Accessly dashboard so request and
runtime metrics are visible out of the box.
