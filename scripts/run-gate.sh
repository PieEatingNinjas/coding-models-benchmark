#!/usr/bin/env bash
# The "gate": the exact same objective check for every model run, so you never
# judge two runs differently by hand. Build + tests must be green for a run to count.
# It also prints how big the change is versus the baseline.
#
# Usage:  ./scripts/run-gate.sh [baseline-ref]   (defaults to the tag "baseline")
set -euo pipefail

BASELINE="${1:-baseline}"

echo "==> Build (warnings are errors)"
dotnet build -warnaserror

echo "==> Test"
dotnet test --no-build

echo "==> Change size vs '${BASELINE}'"
git diff --stat "${BASELINE}" 2>/dev/null || echo "(no ref '${BASELINE}' found — tag the baseline first)"

echo "==> GATE PASSED — build + tests green"
