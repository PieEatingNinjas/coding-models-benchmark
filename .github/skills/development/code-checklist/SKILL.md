---
name: code-checklist
description: Short checklist to run after every class before the build. Use after each implemented type.
---

# Code Checklist (after every class)

> Run this before `dotnet build`. Keep it short — this prevents rework, not endless polish.

- [ ] `nullable` correct: no accidental `!` suppressions; nulls handled.
- [ ] `decimal` for money, `DateTimeOffset` for timestamps.
- [ ] I/O is `async` + `CancellationToken` passed through; no `.Result`/`.Wait()`.
- [ ] Dependencies via constructor injection, not `new` on a service.
- [ ] One public type; file name == type name.
- [ ] No hardcoded secrets/connection strings/URLs.
- [ ] Public APIs have XML docs where useful; no dead code/`TODO` without context.
- [ ] No unsolicited refactor outside the task.
- [ ] `dotnet build -warnaserror` green; `dotnet format` clean.

> Done? Move on to the next class. Don't go back to "make it nicer".
