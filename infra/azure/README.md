# Azure deployment notes

Accessly is Azure-ready. The example Terraform under [`infra/terraform`](../terraform) targets
Azure Container Apps with Azure SQL, Azure Cache for Redis and a storage account. This document
describes how a real deployment would be wired; nothing here is applied automatically.

## Building and publishing images

The API, worker and web images are built from [`infra/docker`](../docker). Publish them to a
registry (for example GitHub Container Registry or Azure Container Registry) and pass the image
references to Terraform via `api_image`, `worker_image` and `web_image`.

## Required configuration

The applications are configured entirely through environment variables (see
[`.env.example`](../../.env.example)). For a hosted environment provide at least:

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings__Default` | Azure SQL connection string |
| `Jwt__SigningKey` | JWT signing key |
| `Redis__Connection`, `Redis__Enabled` | Azure Cache for Redis |
| `RabbitMq__*` | A managed broker, or disable to use in-process handling |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry collector / Application Insights endpoint |

## Secrets

Secrets (SQL password, JWT signing key, registry credentials) are supplied through
**GitHub Actions secrets** or the Container Apps secret store at deploy time — never committed
to the repository. Terraform reads the SQL password from `TF_VAR_sql_admin_password`.

## Observability

The API exposes Prometheus metrics at `/metrics` and is instrumented with OpenTelemetry. To send
traces and metrics to **Azure Application Insights**, run an OpenTelemetry collector configured
with the Application Insights exporter, or set `OTEL_EXPORTER_OTLP_ENDPOINT` to your collector;
no application code changes are required.

## Deployment outline

1. Provision infrastructure with Terraform (`infra/terraform`).
2. Build and push the container images.
3. Update the container apps with the new image tags (CD pipeline or `az containerapp update`).
4. The API applies EF Core migrations on startup; seeding is controlled by `Seed__Enabled`.
