# Contributing to Accessly

Thanks for your interest in Accessly. This document explains how to set up the project,
the conventions we follow, and how to propose changes.

## Development setup

### Prerequisites

- .NET SDK 10+
- Node.js 20+ and npm
- Docker and Docker Compose

### First-time setup

```bash
cp .env.example .env
make setup        # restore backend + install frontend dependencies
make build        # build the .NET solution
make test         # run the backend test suite
```

To run the full local stack (databases, broker, services):

```bash
make docker-up
make logs
```

## Branching model

- `main` is always releasable. Do not commit directly to `main`.
- Create a short-lived branch per change:
  - `feat/<short-name>` — new functionality
  - `fix/<short-name>` — bug fixes
  - `docs/<short-name>` — documentation
  - `ci/<short-name>`, `infra/<short-name>`, `test/<short-name>`, `chore/<short-name>`

## Commit conventions

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <summary>
```

Allowed types: `feat`, `fix`, `test`, `docs`, `ci`, `chore`, `refactor`, `security`, `infra`.

Examples:

```
feat(events): add event publishing workflow
fix(bookings): prevent booking when an event is full
test(api): add integration tests with testcontainers
infra(docker): add local compose stack
```

Keep commits focused. Avoid large, unrelated changes in a single commit.

## Pull requests

1. Branch from an up-to-date `main`.
2. Make your change with tests and documentation where relevant.
3. Run the checks below locally.
4. Open a pull request using the provided template and fill in the checklist.
5. Squash-merge once checks pass.

## Quality checks

| Check | Command |
| --- | --- |
| Backend build | `make build` |
| Backend tests | `make test` |
| C# formatting | `make lint` |
| Frontend build | `cd src/Accessly.Web && npm run build` |
| Frontend tests | `cd src/Accessly.Web && npm test` |

## Coding standards

- Respect the Clean Architecture boundaries; the `Domain` layer has no external
  dependencies and the dependency direction is enforced by architecture tests.
- Validate all external input (FluentValidation in the application layer).
- Never commit secrets. Use `.env` (git-ignored) and provide defaults in `.env.example`.
- Follow the rules in `.editorconfig`; run `make format` to apply fixes.

## Reporting issues

Use the issue templates for bug reports and feature requests. For security concerns, follow
[SECURITY.md](SECURITY.md) instead of opening a public issue.
