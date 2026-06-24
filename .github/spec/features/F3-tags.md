---
title: "Feature F3 — Tags & filtering"
category: feature-plan
priority: 3
tags: [tags, filter, collection, persistence]
status: ready
planner_model: "Claude Opus 4.8"
related_docs: [../domain-model.md, ../api-contracts.md]
summary: Todos can be labeled with tags and filtered by tag.
---

# F3 — Tags & filtering

> Spec-level plan. Describes WHAT and the approach, not the code.
> Constraint: existing tests stay green; new behavior gets new tests.

## Goal
A todo can have zero or more textual tags. Users can filter the list by a tag.

## Scope — what changes
- **Domain model:** add a collection of tags (text values) to the entity. Empty = no tags.
- **Contract (DTO):** tags become part of the contract, mapped. A todo without tags returns an empty collection, not `null`.
- **Write paths:** POST and PUT set/overwrite the tags.
- **Filtering:** the existing list route gets an optional query parameter to filter by tag.
- **Persistence choice:** the implementer chooses how a collection of text is stored with the InMemory provider (e.g. as a navigated collection or a converted value) and documents this briefly in `domain-model.md`.

## Behavior / rules
- Filtering is **case-insensitive** (`Work` matches `work`).
- Without a filter parameter the full list is returned (behavior unchanged from baseline).
- With a filter, only todos that contain the requested tag are returned.
- Tags are a set per todo; duplicate tags on one todo are treated as one (the implementer chooses the normalization and documents it).

## Endpoints (to document in api-contracts.md)
- `GET /todoitems?tag=<x>` → filtered list; without `tag` → everything.
- POST/PUT: body optionally contains tags.

## Acceptance criteria
- Build + tests green.
- Tags are in every DTO response (empty instead of null).
- Filter works case-insensitively; empty filter = full list.
- Persistence choice documented in `domain-model.md`.
- Specs and INDEX updated.

## Tests to add (description)
- **Unit (mapping):** tags are preserved; absent tags → empty collection.
- **Integration:** POST with tags `["work","urgent"]` → findable via `?tag=work` and `?tag=WORK`.
- **Integration:** a todo without the requested tag does not appear in the filtered result.
- **Integration:** no `tag` parameter → all todos.
- Use Bogus for todos with random tag sets where useful.

## Out of scope
Tag management as a separate resource, renaming/deleting tags globally, tag suggestions.
