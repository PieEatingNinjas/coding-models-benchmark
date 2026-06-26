---
title: API Contracts
category: api
priority: 2
tags: [crud, dto, due-date, problemdetails, tags, validation, pagination]
source: [src/TodoApi/Program.cs]
related_docs: [domain-model.md]
summary: Endpoints under /todoitems with methods, bodies, and status codes.
---

# API Contracts

Base route: `/todoitems`. All request/response bodies use `TodoItemDto` (JSON).
`Priority` is serialized as a JSON enum number (`0 = Low`, `1 = Medium`, `2 = High`).
`DueDate` is an optional `DateTimeOffset`.
`Tags` is an optional list of strings; omitted/`null` is treated as an empty list.

| Method | Route | Body | Success | Error |
|--------|-------|------|---------|-------|
| GET | `/todoitems?tag=<tag>&page=<n>&pageSize=<n>` | — | `200` list of DTOs (filtered/paginated) + `X-Total-Count` header | `400` invalid `page`/`pageSize` (ProblemDetails) |
| GET | `/todoitems/complete` | — | `200` completed DTOs only | — |
| GET | `/todoitems/overdue` | — | `200` overdue DTOs only | — |
| GET | `/todoitems/by-priority/{priority}` | — | `200` filtered DTOs | `400` invalid priority |
| GET | `/todoitems/{id:int}` | — | `200` DTO | `404` |
| POST | `/todoitems` | DTO (without Id) | `201` + `Location` + DTO | `400` invalid `Name` or past `DueDate` (ProblemDetails) |
| PUT | `/todoitems/{id:int}` | DTO | `204` | `404`, `400` invalid `Name` or past `DueDate` (ProblemDetails) |
| DELETE | `/todoitems/{id:int}` | — | `204` | `404` |

## Rules
- POST ignores a supplied `Id`; persistence assigns the `Id`.
- PUT overwrites `Name`, `IsComplete`, `Priority`, `DueDate`, and `Tags`.
- POST/PUT default `Priority` to `Medium` when the property is omitted from JSON.
- POST/PUT require `Name`; empty/missing names return `400` ProblemDetails.
- POST/PUT reject `Name` values longer than 200 characters with `400` ProblemDetails.
- POST/PUT reject a supplied `DueDate` in the past with `400` ProblemDetails.
- `GET /todoitems/by-priority/{priority}` matches exact enum values (`Low`/`Medium`/`High`, case-insensitive).
- `GET /todoitems/overdue` returns only todos where `IsComplete = false`, `DueDate` is present, and `DueDate < now`.
- `GET /todoitems` defaults to `page=1` and `pageSize=20`.
- `GET /todoitems` clamps `pageSize` above `100` to `100`.
- `GET /todoitems` returns `400` ProblemDetails when `page <= 0` or `pageSize <= 0`.
- `GET /todoitems` with `tag` returns only todos containing that tag (case-insensitive). Without `tag`, all todos are considered.
- `GET /todoitems` includes `X-Total-Count` with the total number of filtered items before pagination.
- POST/PUT normalize tags (trim + lowercase), ignore blank entries, and deduplicate tags case-insensitively per todo.
- Responses never contain `Secret`.
- `404` for an unknown `{id}` on GET/PUT/DELETE.

> New endpoints from benchmark features are documented here before implementation.
