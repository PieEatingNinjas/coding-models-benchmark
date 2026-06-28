# Playthrough journal

Fill in the header, then add notes per feature **after its gate is green** and commit
this file together with that feature's checkpoint. Capture *how* it got to green
(iterations, manual interventions), not just that it did. See "Journal" in `BENCHMARK-PLAN.md`.

| Field | Value |
|-------|-------|
| Model | Gemini 3.1 Pro |
| Mode | dev |
| Reasoning effort | Medium |
| Dropdown model (if agent `model:` left blank) | |
| Date | |
| Baseline | |

---

## F1 — priority
- **Result:** _(green 1 pass / green after N tries / red → manual fix)_
- **Intervention:** _(none / what you changed by hand and why)_
- **Notes:** 64.5 credits

## F2 — due dates
- **Result:**
- **Intervention:**
- **Notes:** 48.4 credits

## F3 — tags
- **Result:**
- **Intervention:**
- **Notes:** 69.8 credits

## F4 — validation & pagination
- **Result:**
- **Intervention:**
- **Notes:** 83.4 credits

## F5 — clean architecture
- **Result:**
- **Intervention:**
- **Notes:** 103 credits

---

## Overall
- **Reached:** _(F5 completed / faltered at Fx)_
- **Interventions needed:**
- **Quality drift:**
- **Impression:** cleanest layering so far. No EF leak in API (AddApplication+AddInfrastructure),
namespaces match layers, DTO in Application, separate Application.Tests project with Moq.
Minuses: pagination still loads-all-then-pages in memory (but repo interface IS paging-aware, so fixable
without redesign); validation duplicated in endpoints (vs Codex's service-side results); leftover UnitTest1.cs.
Impression: strong — ties Codex on structure, slightly behind on thin-endpoints + scalable querying.
