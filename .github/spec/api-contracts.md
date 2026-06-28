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

| Method | Route | Body | Success | Error |
|--------|-------|------|---------|-------|
| GET | `/todoitems` | — | `200` list of DTOs | — |
| GET | `/todoitems/complete` | — | `200` completed DTOs only | — |
| GET | `/todoitems/overdue` | — | `200` overdue DTOs | — |
| GET | `/todoitems/by-priority/{priority}` | — | `200` filtered DTOs | `400` invalid priority |
| GET | `/todoitems/{id:int}` | — | `200` DTO | `404` |
| POST | `/todoitems` | DTO (without Id) | `201` + `Location` + DTO | `400` past due date |
| PUT | `/todoitems/{id:int}` | DTO | `204` | `400` past due date, `404` not found |
| DELETE | `/todoitems/{id:int}` | — | `204` | `404` |

## Rules
- POST ignores a supplied `Id`; persistence assigns the `Id`.
- PUT overwrites `Name`, `IsComplete`, `Priority`, and `DueDate`.
- Responses never contain `Secret`.
- `404` for an unknown `{id}` on GET/PUT/DELETE.
- `Priority` is serialized as a string (`Low`, `Medium`, `High`); an unknown priority filter returns `400`.
- POST/PUT return `400` (ProblemDetails) when the `DueDate` is in the past.
- "Overdue" filter only includes incomplete items with a `DueDate` that is in the past.

> New endpoints from benchmark features are documented here before implementation.
