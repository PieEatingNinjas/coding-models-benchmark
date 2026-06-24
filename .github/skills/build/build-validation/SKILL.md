---
name: build-validation
description: The fixed build/validation commands for this .NET repo. Use on every final validation and in CI.
---

# Build Validation

> One source of truth for "is this green?". Use exactly these steps locally and in CI.

## Order
```bash
dotnet restore
dotnet build -warnaserror              # zero warnings
dotnet format --verify-no-changes      # consistent style
dotnet test                            # all tests
dotnet list package --vulnerable       # no vulnerable packages
```

## Acceptance criteria
- Build: 0 errors, 0 warnings.
- Format: no changes needed.
- Tests: all green; coverage meets the goals.
- No vulnerable/deprecated packages (or explicitly justified exceptions).

## Rules
- Do not continue on a red build or failing test — fix first.
- Build after every change during implementation, not only at the end.
