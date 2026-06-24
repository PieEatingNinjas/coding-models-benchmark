---
title: "Feature F4 — Validation, ProblemDetails & pagination"
category: feature-plan
priority: 4
tags: [validation, problemdetails, pagination]
status: ready
planner_model: "Claude Opus 4.8"
related_docs: [../domain-model.md, ../api-contracts.md]
summary: A more robust API with input validation, standardized error responses, and pagination on the list.
---

# F4 — Validation, ProblemDetails & pagination

> Spec-level plan. Describes WHAT and the approach, not the code.
> Constraint: existing tests stay green unless the pagination shape affects them — adjust those minimally and justify.

## Goal
The API validates input, returns consistent error responses, and paginates the list.

## Scope — what changes
- **Validation:** `Name` is required and has a maximum length (200 characters). Violations → client error.
- **Error format:** validation errors come back as ProblemDetails (standardized), enabled at the application level.
- **Pagination:** the list route accepts `page` and `pageSize` with defaults and an upper bound.
- **Total count:** the response makes the total number of items known. The implementer chooses one mechanism — a response header OR a wrapping response (envelope) — and documents this in `api-contracts.md`.

## Behavior / rules
- `Name` missing or empty → `400` (ProblemDetails).
- `Name` longer than 200 characters → `400` (ProblemDetails).
- Pagination defaults: `page=1`, `pageSize=20`. Upper bound `pageSize=100` (a larger value is clamped or rejected — choose and document).
- Invalid pagination parameters (e.g. `page=0` or negative) are handled in a defined, documented way (clamp or `400`).
- The total reflects all items, not just the current page.

## Endpoints (to document in api-contracts.md)
- `GET /todoitems?page=&pageSize=` → paginated list + total mechanism.
- POST/PUT: `400` (ProblemDetails) on invalid `Name`.

## Acceptance criteria
- Build + tests green (existing tests adjusted only where the pagination shape requires it, with justification).
- Validation works on POST and PUT with ProblemDetails.
- Pagination respects defaults and upper bound; total correct.
- Specs and INDEX updated; chosen total mechanism and clamp/reject choice documented.

## Tests to add (description)
- **Integration:** POST without `Name` → `400` ProblemDetails.
- **Integration:** POST with `Name` > 200 characters → `400`.
- **Integration:** with 25 items and `pageSize=20` → page 1 has 20, page 2 has 5; total = 25.
- **Integration:** `pageSize` above the upper bound is clamped/rejected as documented.
- **Integration:** defaults apply without parameters.

## Out of scope
Sorting, combining filtering with pagination metadata such as links/HATEOAS, rate limiting.
