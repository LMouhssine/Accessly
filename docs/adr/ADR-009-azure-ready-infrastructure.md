# ADR-009: Azure-ready infrastructure

## Status

Accepted

## Context

The platform should be deployable to a cloud environment in a reproducible way, without
committing to a live deployment or storing any cloud credentials in the repository.

## Decision

Provide an **Azure-ready Terraform example** under `infra/terraform` describing a resource
group, container/app hosting, SQL, Redis and storage, with variables and outputs. The
configuration is an **example only**: it is validated and planned in CI but **never applied
automatically**. Deployment notes and expected variables live in `infra/azure`. Secrets for
any real deployment are supplied through GitHub Actions secrets, never the repository.

## Consequences

- Infrastructure intent is captured as code and reviewed like any other change.
- No accidental provisioning or cost; `apply` is a deliberate, manual action.
- The same OpenTelemetry instrumentation can target Azure Application Insights when deployed.

## Alternatives considered

- **Manual portal setup** — not reproducible or reviewable.
- **Auto-apply on merge** — risks unintended provisioning and cost; explicitly avoided.
