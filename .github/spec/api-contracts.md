---
title: API Contracts
category: api
priority: 2
tags: [crud, dto]
source: [src/TodoApi/Program.cs]
related_docs: [domain-model.md]
summary: Endpoints under /todoitems with methods, bodies, and status codes.
---

# API Contracts

Base route: `/todoitems`. All request/response bodies use `TodoItemDto` (JSON).
`Priority` is serialized as a JSON enum number (`0 = Low`, `1 = Medium`, `2 = High`).

| Method | Route | Body | Success | Error |
|--------|-------|------|---------|-------|
| GET | `/todoitems` | — | `200` list of DTOs | — |
| GET | `/todoitems/complete` | — | `200` completed DTOs only | — |
| GET | `/todoitems/by-priority/{priority}` | — | `200` filtered DTOs | `400` invalid priority |
| GET | `/todoitems/{id:int}` | — | `200` DTO | `404` |
| POST | `/todoitems` | DTO (without Id) | `201` + `Location` + DTO | — |
| PUT | `/todoitems/{id:int}` | DTO | `204` | `404` |
| DELETE | `/todoitems/{id:int}` | — | `204` | `404` |

## Rules
- POST ignores a supplied `Id`; persistence assigns the `Id`.
- PUT overwrites `Name`, `IsComplete`, and `Priority`.
- POST/PUT default `Priority` to `Medium` when the property is omitted from JSON.
- `GET /todoitems/by-priority/{priority}` matches exact enum values (`Low`/`Medium`/`High`, case-insensitive).
- Responses never contain `Secret`.
- `404` for an unknown `{id}` on GET/PUT/DELETE.

> New endpoints from benchmark features are documented here before implementation.
