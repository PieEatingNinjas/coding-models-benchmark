---
name: github-actions
description: Conventions for GitHub Actions CI/CD workflows for .NET — build, test, security, deploy. Used by DevOpsAgent.
---

# GitHub Actions (.NET)

## Basic CI (`.github/workflows/ci.yml`)

```yaml
name: CI
on:
  push: { branches: [main] }
  pull_request: { branches: [main] }
permissions:
  contents: read
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '10.0.x' }
      - run: dotnet restore
      - run: dotnet build -warnaserror --no-restore
      - run: dotnet format --verify-no-changes
      - run: dotnet test --no-build --collect:"XPlat Code Coverage"
      - run: dotnet list package --vulnerable --include-transitive
```

## Conventions
- **Pin versions** — action tag or commit SHA; no `@master`.
- **Minimal `permissions:`** per workflow/job.
- **Secrets** via `${{ secrets.* }}`; for Azure: **OIDC** (`azure/login` with federated credentials), no long-lived keys.
- **Caching** of NuGet for speed (`actions/cache` or `setup-dotnet` cache).
- **Path/branch filters** so workflows only run where needed.

## Deploy (separate workflow, `deploy.yml`)
- Trigger on tag/release or after green CI.
- `dotnet publish -c Release` → artifact or container image.
- Deploy to Azure App Service / Container Apps via `azure/login` (OIDC) + CLI/Bicep.

## Agentic workflows (optional)
If you want an AI agent in the pipeline (e.g. a security review that automatically opens a PR with fixes), look at GitHub Agentic Workflows (gh-aw): a natural-language `.md` workflow + a compiled `.lock.yml`. Verify current syntax via Microsoft Learn MCP / official docs before use.

## Rules
- Verify action/Azure syntax before use (Microsoft Learn MCP).
- No secrets in logs; no `pull_request_target` without reason.
