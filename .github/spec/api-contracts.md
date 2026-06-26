---
title: API Contracts
category: api
priority: 2
tags: [crud, dto, due-date, problemdetails]
source: [src/TodoApi/Program.cs]
related_docs: [domain-model.md]
summary: Endpoints under /todoitems with methods, bodies, and status codes.
---

# API Contracts

Base route: `/todoitems`. All request/response bodies use `TodoItemDto` (JSON).
`Priority` is serialized as a JSON enum number (`0 = Low`, `1 = Medium`, `2 = High`).
`DueDate` is an optional `DateTimeOffset`.

| Method | Route | Body | Success | Error |
|--------|-------|------|---------|-------|
| GET | `/todoitems` | — | `200` list of DTOs | — |
| GET | `/todoitems/complete` | — | `200` completed DTOs only | — |
| GET | `/todoitems/overdue` | — | `200` overdue DTOs only | — |
| GET | `/todoitems/by-priority/{priority}` | — | `200` filtered DTOs | `400` invalid priority |
| GET | `/todoitems/{id:int}` | — | `200` DTO | `404` |
| POST | `/todoitems` | DTO (without Id) | `201` + `Location` + DTO | `400` past `DueDate` (ProblemDetails) |
| PUT | `/todoitems/{id:int}` | DTO | `204` | `404`, `400` past `DueDate` (ProblemDetails) |
| DELETE | `/todoitems/{id:int}` | — | `204` | `404` |

## Rules
- POST ignores a supplied `Id`; persistence assigns the `Id`.
- PUT overwrites `Name`, `IsComplete`, `Priority`, and `DueDate`.
- POST/PUT default `Priority` to `Medium` when the property is omitted from JSON.
- POST/PUT reject a supplied `DueDate` in the past with `400` ProblemDetails.
- `GET /todoitems/by-priority/{priority}` matches exact enum values (`Low`/`Medium`/`High`, case-insensitive).
- `GET /todoitems/overdue` returns only todos where `IsComplete = false`, `DueDate` is present, and `DueDate < now`.
- Responses never contain `Secret`.
- `404` for an unknown `{id}` on GET/PUT/DELETE.

> New endpoints from benchmark features are documented here before implementation.
