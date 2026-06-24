# Runbook — running the benchmark

Plain, copy-paste steps to run a **cumulative playthrough**: one model walks F1 → F5 on its own growing codebase. For the rationale and judging, see `BENCHMARK-PLAN.md`.

> One **worktree per model** (not per feature). Pick the mode for the playthrough: **Mode A** = **DeveloperAgent** (suffix `-dev`), **Mode B** = **OrchestratorAgent** (suffix `-orch`). Keep model, reasoning effort, and clean room identical across everything you compare.

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

## 1. Start a playthrough for one model

A git *worktree* is a second working folder sharing the same repo history. Make one per model, branched from `baseline`:

```bash
git worktree add -b play/gpt-codex-dev ../play-gpt-codex-dev baseline
```

Open that folder in VS Code, pick the model + mode (DeveloperAgent or OrchestratorAgent), and apply the **clean room** (above).

## 2. Walk the features F1 → F5 on the same codebase

For each feature in order, give the model the identical task prompt from `BENCHMARK-PLAN.md`, swapping only the file:

```
Implement the feature specified in .github/spec/features/F<n>-<name>.md.
Context and conventions: .github/spec/INDEX.md and .github/copilot-instructions.md.
```

Don't reset between features — each one builds on what the model already wrote.

## 3. Gate + journal + commit after each feature

```bash
./scripts/run-gate.sh HEAD     # build + test + show this feature's diff vs the last checkpoint
```

- **Green** → add a `JOURNAL.md` entry for this feature (template in `BENCHMARK-PLAN.md`), then checkpoint: `git add -A && git commit -m "F1"`. Move to the next feature on the same code.
- **Red** → don't abort. Let the model try to fix it first. If it still can't, make a **documented manual fix yourself**, write exactly what you changed (and why) in the journal as an *intervention*, then gate + commit and continue. Needing intervention is data, not failure.

> Keep `JOURNAL.md` in this worktree; it gets committed with each feature, so the journal and the code evolve together (`git log`, `git show <commit>`).

## 4. Judge the playthrough

After F5 (or wherever it broke), judge the whole run on feel (see "Judging" in `BENCHMARK-PLAN.md`): did it stay green the whole way, did quality drift, did F5 clean up its own work? A few lines per playthrough.

## 5. Next model

```bash
git worktree add -b play/<next-model>-dev ../play-<next-model>-dev baseline
```

New worktree from `baseline`, repeat. Remove old worktrees when done judging (`git worktree remove ../play-...`).

## Notes

- **Deterministic data:** test fake data uses a fixed Bogus seed (`tests/.../Support/TestSeed.cs`) and parallelization is disabled, so the data is identical across runs and models.
- **Same bar for everyone:** `Directory.Build.props` and `.editorconfig` centralize the conventions (TFM, nullable, warnings-as-errors) so no model can quietly relax them. New projects a model adds (e.g. in F5) inherit them automatically.
