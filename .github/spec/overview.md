---
title: Overview
category: domain
priority: 1
tags: [crud, baseline]
source: [min-web-api tutorial]
related_docs: [domain-model.md, api-contracts.md]
summary: Purpose, scope, and non-goals of the TodoApi baseline.
---

# Overview

## Purpose
A minimal, correct todo API that serves as a **fixed, simple baseline** for comparing coding models (see `BENCHMARK-PLAN.md`). The baseline is intentionally small so that features can be built on top of it predictably.

## Scope (baseline)
- CRUD over todo items via a Minimal Web API.
- Persistence via EF Core InMemory (no real database needed).
- Strict separation between entity (`TodoItem`) and public contract (`TodoItemDto`).
- Unit and integration tests (xUnit, Bogus, FluentAssertions).

## Non-goals (baseline)
- No authentication/authorization.
- No real database, migrations, or external integrations.
- No priorities, due dates, tags, pagination — those come as **features** in the benchmark.

## Quality bar
- `dotnet build -warnaserror` and `dotnet test` green.
- Endpoints never leak the `Secret` property.
