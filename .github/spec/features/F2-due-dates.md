---
title: "Feature F2 — Due dates & overdue"
category: feature-plan
priority: 2
tags: [due-date, validation, overdue, problemdetails]
status: ready
planner_model: "Claude Opus 4.8"
related_docs: [../domain-model.md, ../api-contracts.md]
summary: Todos get an optional due date, with validation and an overdue overview.
---

# F2 — Due dates & overdue

> Spec-level plan. Describes WHAT and the approach, not the code.
> Constraint: existing tests stay green; new behavior gets new tests.

## Goal
A todo can have an optional due date. Users can fetch what is overdue.

## Scope — what changes
- **Domain model:** add an optional due date (a timezone-aware timestamp type). Optional = may be absent.
- **Contract (DTO):** the due date becomes part of the contract, mapped, optional.
- **Write paths:** POST and PUT accept a due date; validation on it (see rules).
- **New endpoint:** an overview of overdue todos.

## Behavior / rules
- A due date is optional; absence is valid and means "no deadline".
- On POST and PUT, a supplied due date may **not be in the past** → client error (`400`).
- Error responses use a standardized problem format (ProblemDetails) instead of a bare string.
- "Overdue" = a todo that is **not complete** and has a due date that is **before now**. Todos without a due date are never overdue. Completed todos are never overdue.
- "Now" refers to the current time at the moment of the request.

## Endpoints (to document in api-contracts.md)
- `GET /todoitems/overdue` → `200` with only the overdue todos.
- POST/PUT: `400` (ProblemDetails) when the due date is in the past.

## Acceptance criteria
- Build + tests green.
- Due date is optional in the DTO and mapped correctly.
- Past-date validation works on both POST and PUT, with ProblemDetails.
- Overdue filter respects the three rules (not complete, expired, due date present).
- Specs and INDEX updated.

## Tests to add (description)
- **Integration:** POST with a due date in the past → `400` with ProblemDetails shape.
- **Integration:** PUT with a due date in the past → `400`.
- **Integration:** a todo without a due date does not appear in `overdue`.
- **Integration:** a completed todo with an expired due date does not appear in `overdue`.
- **Integration:** an incomplete todo with an expired due date does appear in `overdue`.
- Mind test determinism: choose dates relative to "now" (e.g. well in the past/future), not hardcoded absolute dates.

## Out of scope
Reminders/notifications, per-user timezone conversion, recurring deadlines.
