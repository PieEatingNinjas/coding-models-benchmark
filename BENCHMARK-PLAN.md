# Benchmark plan — comparing coding models on the TodoApi

This document is the starting point for comparing different coding models (or agents) on realistic work. Each model does a **cumulative playthrough**: starting from the same frozen baseline, it implements features F1 → F5 **one after another on its own growing codebase**, just like real development. You then compare whole playthroughs on feel. This is closer to "which model do I actually want as my .NET dev" than implementing each feature from scratch.

## Roles vs. models (read this first)

An "agent" here is a **role** (a prompt/persona in `.github/agents/`), not a model. A **model** is what fills a role. The same model can play any role; what you vary in the benchmark is which model you drop into the role under test.

| Role | What it does | In this benchmark |
|------|--------------|-------------------|
| **Planner / Orchestrator** (Phase 1 = analysis & specs) | Turns a feature into a spec-level plan | **Fixed.** Run once, frozen as the `F*.md` plans (planner = Claude Opus 4.8). |
| **Implementer** (Developer, and Tester) | Writes the code/tests from the plan | **The variable.** This is the model you compare. |
| Security / DevOps | Review & pipeline | Not exercised in the core comparison. |

Note: the **Planner and the Orchestrator are the same role** — Phase 1 of the OrchestratorAgent *is* the planning. We pre-ran that phase and froze its output, so in the core comparison the orchestrator does not run live.

## Two modes — one playthrough each

A playthrough can be run in either mode (pick one per playthrough; you can do both per model if you want to compare the modes too):

**Mode A — implementation only.** Pick the **DeveloperAgent**. No orchestrator in the loop; the DeveloperAgent writes the code and its own tests. This isolates raw coding ability. Worktree suffix: `-dev`.

**Mode B — full agent system.** Pick the **OrchestratorAgent**. It drives the pipeline and delegates to the worker agents (Developer/Tester/Security) as **context-isolated subagents**. Here you're also measuring "how well does this model run a team". Worktree suffix: `-orch`.

Keep model + reasoning effort + clean room identical across everything you compare.

### Mode B setup (one-time, in the baseline)

Subagent orchestration is wired in `.github/agents/orchestrator.agent.md` via its frontmatter:

```
tools: ['agent', 'edit', 'search', 'web/fetch', 'runCommands', 'runTasks']
agents: ['DeveloperAgent', 'TesterAgent', 'SecurityAgent', 'DevOpsAgent']
```

The `agent` tool lets the orchestrator invoke subagents; `agents` whitelists which ones. The worker agents are left without a `tools` restriction, so they get full default tools and are subagent-invocable by default. This is a **preview** VS Code feature — expect occasional rough edges (if the orchestrator doesn't actually delegate, that's the preview, not your setup). Nested subagents stay off (`chat.subagents.allowInvocationsFromSubagents` = false by default), so there's no runaway nesting.

> Note: in Mode B the orchestrator re-does its own Phase 1 (writes/updates specs) and produces a report, so its diff is larger and the run is slower than Mode A. That's expected — it's part of what Mode B measures.

## Planning vs. implementation (important)

**Planning** a feature and **implementing** it are kept separate:

- **Planning is done once, by a single fixed planner model.** The plans are already prepared in `.github/spec/features/` (F1–F5), at spec level (WHAT + acceptance criteria + test cases, no code). This keeps the plan a constant so you measure implementation purely.
- **Implementation is done by the models under comparison**, each from the same baseline with the same plan.

> Want a different planner model later? Then regenerate **all** plans with it; don't mix planners within one comparison. Fill in `planner_model` in each plan.

## How it works (cumulative playthrough)

One **worktree per model** (not per feature). Inside it, the model walks F1 → F5 on its own evolving code. You only go back to `baseline` when you start a **new model**.

1. **Freeze the baseline.** Commit the current repo (green build + tests) and tag it `baseline`. Every playthrough starts here.
2. **Start a playthrough for a model.** New worktree from `baseline`, e.g. `git worktree add -b play/gpt-codex-dev ../play-gpt-codex-dev baseline`. Pick the model + mode (DeveloperAgent or OrchestratorAgent) and apply the clean room.
3. **Walk the features in order.** For each of F1 → F5, give the **identical** task prompt below (just swap the `F<n>` file). After each feature: run the gate, and if green **commit** it (e.g. `git commit -m "F1"`) so each step is a checkpoint, and add a journal entry (see "Journal" below). Then move to the next feature **on the same codebase** — don't reset.
   - "Existing tests/specs stay green" in each plan now means *everything the model has built so far*. Early choices carry forward; dealing with them is part of the test.
   - **Don't stop on a red feature.** Let the model try to fix it first. If it can't, make a **documented manual fix yourself** (note exactly what you changed and why in the journal), commit, and continue. Needing human intervention is a real signal — "how much hand-holding did this model need" — not a reason to abort the playthrough.
4. **Judge the whole playthrough** on feel (see "Judging"). Did it stay green the whole way? Did quality drift as the codebase grew? How does the end state look, and did F5 (the refactor) clean up its own accumulated work?
5. **Next model:** new worktree from `baseline`, repeat. (Optional: a second playthrough of the same model to gauge run-to-run variance — recall F1 alone produced two different designs.)

> You're not building a product — these worktrees are disposable probes. Keep them around long enough to judge, then move on.

## Task template (give this to each model)

Keep it minimal on purpose. The ground rules, test stack, and build gates already live in `.github/copilot-instructions.md` (which Copilot applies automatically as repository custom instructions), and the per-feature acceptance criteria live in the plan itself. A terse prompt also tests something real: whether the model reads and follows the repo's context, and whether it decomposes the work itself.

```
Implement the feature specified in .github/spec/features/F<n>-<name>.md.
Context and conventions: .github/spec/INDEX.md and .github/copilot-instructions.md.
```

> Give the identical prompt to every model. Don't re-state the ground rules, test stack, or build gates — that just duplicates what's already in the repo and would help weaker models in a way that muddies the comparison.
>
> **No forced planning step.** The feature plan (`F<n>.md`) is already the plan, so requiring the model to re-plan into a `PLAN.md` is redundant on tight features (you saw this on F1) and mostly helps weaker models — exactly the difference you want to keep. Whether a model plans internally is part of what you're measuring. If you ever want to study the planning effect itself, do it as a deliberate A/B on a *bigger* feature (e.g. F5): run it once as-is and once with an added "write a short plan first" line, and compare.

---

## Features

The full, spec-level plans live in **`.github/spec/features/`** (one file per feature, written in advance by the planner model). Below is just an overview; give the matching file as the task to each model.

| Feature | Plan | Short description |
|---------|------|-------------------|
| F1 | `features/F1-priority.md` | Priority (`Low/Medium/High`) + filter endpoint (warm-up) |
| F2 | `features/F2-due-dates.md` | Optional due date, past-date validation (ProblemDetails), overdue overview |
| F3 | `features/F3-tags.md` | Tags per todo + case-insensitive filtering |
| F4 | `features/F4-validation-pagination.md` | Name validation, ProblemDetails, pagination with total |
| F5 | `features/F5-clean-architecture.md` | Refactor into layers, preserve behavior, unit tests on the Application layer (the big test) |

> Start with F1 as a warm-up; F5 is the big test (refactoring within boundaries — often where models differ the most).

---

## Judging (score on feel)

No rigid scorecard. Trust your judgment — you'll often *see* which playthrough is better. The point of the list below is just to keep your eye consistent across models, not to add up numbers.

**Across the whole playthrough**, weigh: did it stay green at every feature? Did code quality hold up or drift as the codebase grew? Did it manage its own earlier decisions (or did an F1 shortcut cause pain in F4)? How clean is the end state, and did F5 successfully refactor its own accumulated work?

**At each feature step**, weigh roughly:

- **Does it work and stay green?** This part isn't a feeling — it's the hard gate (see below). Anything that doesn't build or pass tests is out before judgment starts.
- **Did it do what the plan asked** — right endpoints, status codes, behavior?
- **Are the new tests any good** — do they actually exercise the behavior, with meaningful asserts?
- **Does it fit the codebase** — conventions, DTO/Secret rule, async, one type per file?
- **Did it stay in scope** — no sprawling refactors; the diff is about this feature?
- **Would you be happy to maintain it** — readable, no dead code? (Specs kept in sync is a minor plus, not a dealbreaker.)
- **How smooth was it** — roughly how many iterations to green, diff size, time/tokens.

Then form a holistic impression per model (e.g. a quick "strong / ok / weak", or just rank them against each other). Keep a few sentences of notes on *why* — that's more useful later than a number.

### The hard gate (not a feeling)

After **each feature** in the playthrough, run the same objective check before committing:

```bash
dotnet build -warnaserror
dotnet test
git diff --stat HEAD        # this feature's change (vs the previous checkpoint)
git diff --stat baseline    # the whole playthrough so far
```

Feature red → don't commit it yet; let the model iterate, or mark the playthrough as faltering here. A model that can't get green on F3 tells you something — note where it broke.

### Journal (in the worktree, per feature)

Keep a `JOURNAL.md` in the playthrough's worktree and add an entry **per feature**, committed together with that feature's checkpoint. Co-locating it with the code means your commentary evolves alongside the build — `git log`/`git show` then shows both. (Small caveat: since it's committed, a later feature could in theory read your notes about earlier ones; with the minimal prompt the model isn't pointed at it, so the risk is low. Keep it uncommitted if you want zero risk.)

Capture **how** it got to green, not just that it did — especially any **manual intervention** (that's the signal). Template:

```
# Playthrough journal
Model: <model>     Mode: <dev | orch>     Reasoning: <e.g. Medium>
Dropdown model (if used): <…>     Date: <yyyy-mm-dd>

## F1 — priority
- Result: green 1 pass | green after N tries | red → manual fix
- Intervention: none | <what you changed by hand and why>
- Notes: <approach, smells, surprises>

## F2 — due dates
- Result: …
- Intervention: …
- Notes: …

## F3 — tags
## F4 — validation & pagination
## F5 — clean architecture

## Overall
- Reached: <F5 / faltered at Fx>
- Interventions needed: <count + summary>
- Impression: <strong / ok / weak> — <why>
```

An incomplete playthrough is valid data: record where and why it stalled, and what (if anything) you did to push past it.
