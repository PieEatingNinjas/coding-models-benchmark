# Spec Index — TodoApi

> **Navigation hub.** EVERY agent starts here and filters from here by category/tags.
> Update this file whenever you add or remove a spec.

## Reading order (priority 1 → 5)

| # | Spec | Category | Prio | Summary |
|---|------|----------|------|---------|
| 1 | `overview.md` | domain | 1 | Purpose, scope, non-goals of the baseline |
| 2 | `domain-model.md` | data-model | 1 | TodoItem, TodoItemDto, mapping, Secret rule |
| 3 | `api-contracts.md` | api | 2 | Endpoints under /todoitems, status codes |

## By category

- **domain:** overview
- **data-model:** domain-model
- **api:** api-contracts

## Tags index

| Tag | Specs |
|-----|-------|
| `dto` | domain-model, api-contracts |
| `secret` | domain-model |
| `crud` | api-contracts |
| `efcore` | domain-model |

## Feature plans (benchmark)

Pre-written, spec-level plans per feature in `features/` — input for the model comparison (see `BENCHMARK-PLAN.md` and `features/README.md`). One plan per feature, written by a fixed planner model.

| # | Plan | Prio | Summary |
|---|------|------|---------|
| F1 | `features/F1-priority.md` | 1 | Priority + filter |
| F2 | `features/F2-due-dates.md` | 2 | Due dates + overdue + validation |
| F3 | `features/F3-tags.md` | 3 | Tags + filtering |
| F4 | `features/F4-validation-pagination.md` | 4 | Validation, ProblemDetails, pagination |
| F5 | `features/F5-clean-architecture.md` | 5 | Refactor to Clean Architecture |

## Open bugs / attention points

| # | Description | Spec | Status |
|---|-------------|------|--------|
| _(empty)_ | | | |

> When implementing a feature, the implementer updates the relevant baseline specs
> (`overview`, `domain-model`, `api-contracts`) and adds new endpoints above.
