# P0 Runtime Gate Contract

- Date: 2026-04-12
- Baseline: `docs/p0-release-gate.md`, `update_plan.md`

## Goal

- Turn the current stage timer into a real failure condition.
- Make unresolved orders count as missed when the timer expires.
- Replace the speed-bonus stub with active score rules.
- Keep stage result logic honest by including speed bonus in max-score calculation.
- Report timer failure separately from low-score failure on the result screen summary.

## Completion Criteria

- `StageManager` exposes timer expiry as a real runtime condition.
- `GameManager` stops active play and ends the stage when the timer expires.
- `CustomerSpawner` can settle every unresolved customer as timed out when the stage clock runs out.
- `ScoreRules.GetSpeedBonus(...)` returns non-zero bonuses for fast clears.
- `ScoreRules.GetMaximumRecipeScore(...)` includes the maximum speed bonus.
- `ScoreManager` can record timeout reasons for both normal patience expiry and timer-expiry misses.
- `StageResult` preserves why the stage ended so pass/fail logic can distinguish timer failure from low score.
- Unity batch compile succeeds.
