---
title: API Contracts
category: api
priority: 2
tags: [crud, dto, tags]
source: [src/TodoApi/Program.cs]
related_docs: [domain-model.md]
summary: Endpoints under /todoitems with methods, bodies, and status codes.
---

# API Contracts

Base route: `/todoitems`. All request/response bodies use `TodoItemDto` (JSON).

| Method | Route | Body | Success | Error |
|--------|-------|------|---------|-------|
| GET | `/todoitems` | — | `200` list of DTOs | — |
| GET | `/todoitems?tag=<x>` | — | `200` list of DTOs filtered by tag (full list if omitted) | — |
| GET | `/todoitems/complete` | — | `200` completed DTOs only | — |
| GET | `/todoitems/overdue` | — | `200` overdue DTOs only | — |
| GET | `/todoitems/by-priority/{priority}` | — | `200` filtered DTOs | `400` |
| GET | `/todoitems/{id:int}` | — | `200` DTO | `404` |
| POST | `/todoitems` | DTO (without Id) | `201` + `Location` + DTO | `400` ProblemDetails if due date is in the past |
| PUT | `/todoitems/{id:int}` | DTO | `204` | `404`, `400` ProblemDetails if due date is in the past |
| DELETE | `/todoitems/{id:int}` | — | `204` | `404` |

## Rules
- POST ignores a supplied `Id`; persistence assigns the `Id`.
- PUT overwrites `Name`, `IsComplete`, `Priority`, `DueDate`, and `Tags`.
- POST/PUT bodies may include `Tags`; omitted tags are treated as an empty collection.
- Priority is serialized as a JSON string (`"Low"`, `"Medium"`, `"High"`).
- A missing priority on POST/PUT defaults to `Medium`.
- `DueDate` is optional and serialized as an RFC3339 timestamp when present.
- POST/PUT reject a supplied due date that is earlier than the current request time with `400` ProblemDetails.
- `GET /todoitems?tag=<x>` filters case-insensitively; an empty or missing `tag` parameter returns the full list.
- Tags are normalized to lowercase, trimmed, and deduplicated on input and output.
- `GET /todoitems/overdue` returns todos that are incomplete, have a due date, and are past due.
- `GET /todoitems/by-priority/{priority}` returns todos with the exact requested priority and `400` for an unknown value.
- Responses never contain `Secret`.
- `404` for an unknown `{id}` on GET/PUT/DELETE.

> New endpoints from benchmark features are documented here before implementation.
