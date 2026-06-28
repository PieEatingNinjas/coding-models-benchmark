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

## CRUD Endpoints

| Method | Route | Body | Success | Error |
|--------|-------|------|---------|-------|
| GET | `/todoitems` | — | `200` list of DTOs | — |
| GET | `/todoitems/complete` | — | `200` completed DTOs only | — |
| GET | `/todoitems/{id:int}` | — | `200` DTO | `404` |
| GET | `/todoitems/by-priority/{priority}` | — | `200` list of DTOs with matching priority | `400` for invalid priority |
| POST | `/todoitems` | DTO (without Id) | `201` + `Location` + DTO | — |
| PUT | `/todoitems/{id:int}` | DTO | `204` | `404` |
| DELETE | `/todoitems/{id:int}` | — | `204` | `404` |

## Rules
- POST ignores a supplied `Id`; persistence assigns the `Id`.
- POST/PUT: `Priority` is optional in the request body; if omitted, defaults to `Medium`.
- PUT overwrites `Name`, `IsComplete`, and `Priority`.
- Responses always include `Priority` and never contain `Secret`.
- `404` for an unknown `{id}` on GET/PUT/DELETE.
- `400` for an invalid priority value on `/by-priority/{priority}`. Valid values are case-insensitive: `Low`, `Medium`, `High`.

## Priority Serialization
Priority is serialized as a text string (`Low`, `Medium`, `High`) in all DTOs (both request and response).

> New endpoints from benchmark features are documented here before implementation.
