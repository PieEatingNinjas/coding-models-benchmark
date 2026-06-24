# Workflows

CI/CD for this repo. Maintained by the **DevOpsAgent** (skill: `devops/github-actions`).

## `ci.yml`
Runs on push/PR to `main`: restore → build (`-warnaserror`) → format check → test (+coverage) → vulnerable-package check. This is the gate that also applies locally (skill `build/build-validation`).

## Deploy (to add)
A separate `deploy.yml` on tag/release: `dotnet publish -c Release` → artifact/container → Azure via OIDC (`azure/login`). No long-lived secrets.

## Agentic workflows (optional)
If you want AI agents in the pipeline (e.g. an automatic security-review PR), see GitHub Agentic Workflows (gh-aw): a natural-language `.md` workflow that compiles to a `.lock.yml`. Commit both. Verify the current syntax via the official docs / Microsoft Learn MCP.

## Conventions
- Pin action versions; minimal `permissions:`; secrets via GitHub Secrets/OIDC; no secrets in logs.
