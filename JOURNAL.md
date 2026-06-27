# Playthrough journal

Fill in the header, then add notes per feature **after its gate is green** and commit
this file together with that feature's checkpoint. Capture *how* it got to green
(iterations, manual interventions), not just that it did. See "Journal" in `BENCHMARK-PLAN.md`.

| Field | Value |
|-------|-------|
| Model | MAI Code 1 Flash |
| Mode | dev |
| Reasoning effort | Medium |
| Dropdown model (if agent `model:` left blank) | |
| Date | |
| Baseline | |

---

## F1 — priority
- **Result:** green 1 pass
- **Intervention:** none
- **Notes:** 14.8 credits

## F2 — due dates
- **Result:** green 1 pass
- **Intervention:** non
- **Notes:** 21.8 credits
  
  Date check in both POST and PUT (small DRY violation, but consistent with KISS - don't overengineer)

## F3 — tags
- **Result:**
- **Intervention:**
- **Notes:** 20.3 credits

  Different approach to the tags. Not a fan of collapsing the list into a single (string) column: future DB-side filtering will be a pain (it currently loads all rows and filters in memory). Do like putting the tag handling (SetTags, normalizing) on the domain model.

## F4 — validation & pagination
- **Result:**
- **Intervention:**
- **Notes:** 13.8 credits

   Paging in memory (load all, then slice) as a result of the tags filtering. Curiuous if F5 will fix this.

## F5 — clean architecture
- **Result:**
- **Intervention:**
- **Notes:** 41.1 credits

---

## Overall
- **Reached:** _(F5 completed / faltered at Fx)_
- **Interventions needed:**
- **Quality drift:**
- **Impression:** _(strong / ok / weak — why)_
