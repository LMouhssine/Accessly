# Security Policy

## Supported versions

Accessly is under active development. Security fixes are applied to the latest `main` and
the most recent tagged release.

| Version | Supported |
| --- | --- |
| `main` (latest) | ✅ |
| Latest tagged release | ✅ |
| Older tags | ❌ |

## Reporting a vulnerability

Please **do not** open a public issue for security vulnerabilities.

Instead, report privately through GitHub's
[private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability)
on this repository (Security → Report a vulnerability), or contact the maintainer,
Mouhssine Lakhili.

When reporting, please include:

- a description of the issue and its impact;
- steps to reproduce or a proof of concept;
- affected components and versions.

We aim to acknowledge reports promptly and to keep you informed about remediation progress.
Please allow a reasonable period for a fix before any public disclosure.

## Scope

This is a demonstration platform. Payments and email delivery are simulated by default and
must not be connected to real financial or personal-data systems. Reports about the
following are in scope:

- authentication and authorization flaws (JWT, RBAC);
- injection, broken access control, or data exposure;
- insecure configuration defaults shipped in the repository;
- dependency vulnerabilities with a practical impact.

## Security practices in this repository

- No secrets are committed; configuration is provided via environment variables.
- Inputs are validated server-side.
- Authentication uses JWT with role-based authorization.
- Rate limiting, controlled CORS and security headers are applied at the API edge.
- Dependency and filesystem scanning run in CI.

See [docs/security/threat-model.md](docs/security/threat-model.md) for the threat model.
