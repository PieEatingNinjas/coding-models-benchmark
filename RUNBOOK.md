# Runbook — running the benchmark

Plain, copy-paste steps to compare models on one feature. For the rationale and scoring, see `BENCHMARK-PLAN.md`.

> This runbook is **Mode A** (compare implementation): the plan is fixed, no orchestrator runs, and only the implementer model varies. See "Two ways to run it" in `BENCHMARK-PLAN.md`.

## Clean room — keep ambient VS Code customizations out

VS Code surfaces agents and skills from three scopes:

- **Workspace** — this repo's `.github/agents/` and `.github/skills/`. **This is the benchmark.**
- **User** — your personal `~/.copilot/skills/` and user-level agents. Ambient; travels across all workspaces.
- **Plugins** — installed Copilot plugins under `~/.copilot/installed-plugins/`. One plugin can add many agents *and* skills (e.g. the MSBuild/.NET diagnostics set).

Only the Workspace scope belongs to this benchmark. User and Plugin items are ambient, differ per machine, and would pollute the comparison if Copilot auto-loads them. Before each run, reduce the active set to **only this workspace + the deliberately wired Microsoft Learn MCP**:

1. **Extensions view → "Agent Plugins - Installed"** (or filter `@agentPlugins`) → for each plugin, use the context menu to **disable for this workspace**. The MSBuild/.NET set (`msbuild`, `binlog-*`, `analyzing-dotnet-performance`, `msbuild-code-review`, …) comes from the **`dotnet/skills`** plugin — disable it here for the benchmark workspace. (Check/update plugins from this same view; nothing updates without your confirmation.)
2. **Chat: Configure Skills** → turn **off** every User- and Plugin-scoped skill; keep only the Workspace skills (`.github/skills/`).
3. **Agents** panel → confirm only the 5 Workspace agents are active.
4. Keep **Microsoft Learn MCP** on — it's wired in `.vscode/mcp.json` and part of the baseline, so it's a controlled input that is equal for every model (not pollution). If you want the purest code-only run, you may disable it too — just do so for *all* models.

Do this identically for every model run, and re-check after installing or updating any extension.

## 0. One-time: verify and freeze the baseline

Run on a .NET 10 machine, in the repo root:

```bash
dotnet restore
dotnet build -warnaserror
dotnet test
```

All green? Freeze this exact state so every model starts from the same point:

```bash
git add -A && git commit -m "baseline"
git tag baseline
```

> If build/test is red, fix it first. The whole comparison rests on a green baseline.

## 1. Give each model its own clean copy

A git *worktree* is a second working folder that shares the same repo history, so each model works in isolation without touching the others:

```bash
git worktree add ../run-modelA baseline
git worktree add ../run-modelB baseline
```

Each folder is a fresh checkout of the baseline.

## 2. Hand the model the task

In its worktree, give the model these files plus the task template from `BENCHMARK-PLAN.md` — identical for every model:

- `.github/copilot-instructions.md` — the ground rules
- `.github/spec/INDEX.md` — where it finds context
- `.github/spec/features/F1-priority.md` — the feature to implement (pick the one you're testing)

## 3. Run the gate

The "gate" is just a fixed check you run the same way for every model, so you never judge two runs differently by hand. It builds, runs the tests, and shows how big the change is compared to the baseline:

```bash
./scripts/run-gate.sh baseline
```

- Build or tests red → the run **fails the gate** (doesn't count yet; let the model iterate).
- Green → the run **counts**; note the change size it prints.

## 4. Score

Open `BENCHMARK-PLAN.md`, score the run with the rubric (0–5 per criterion), and note the practical metrics (iterations to green, diff size, time/tokens).

## 5. Reset for the next feature/model

```bash
git worktree remove ../run-modelA
```

Repeat from step 1 for the next model or feature.

## Notes

- **Deterministic data:** test fake data uses a fixed Bogus seed (`tests/.../Support/TestSeed.cs`) and parallelization is disabled, so the data is identical across runs and models.
- **Same bar for everyone:** `Directory.Build.props` and `.editorconfig` centralize the conventions (TFM, nullable, warnings-as-errors) so no model can quietly relax them. New projects a model adds (e.g. in F5) inherit them automatically.
