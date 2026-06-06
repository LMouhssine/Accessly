# ADR-008: GitHub Actions for CI/CD

## Status

Accepted

## Context

The project lives on GitHub and needs automated builds, tests and quality gates on every
change, plus repeatable releases.

## Decision

Use **GitHub Actions** for CI/CD with focused workflows:

- `ci.yml` — backend restore/build/test, architecture tests, frontend lint/test/build and
  container builds, on pull requests and pushes to `main`.
- `security.yml` — filesystem and secret scanning, dependency review and a dependency audit.
- `release.yml` — build, test, package and publish a GitHub release on `v*` tags.
- `terraform.yml` — `fmt`, `validate` and `plan` on infrastructure changes; never `apply`.
- `docs.yml` — Markdown linting and link checking.

## Consequences

- Every change is validated automatically; status badges reflect real workflow results.
- Releases are reproducible from tags.
- Workflows run close to the code with no extra CI service to operate.

## Alternatives considered

- **Azure DevOps Pipelines / other CI** — viable, but GitHub Actions is native to the
  repository host and keeps configuration alongside the code.
