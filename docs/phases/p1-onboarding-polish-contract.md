# P1 Onboarding Polish Contract

- Date: 2026-04-12
- Baseline: `docs/p0-release-gate.md`, `docs/phases/p0-runtime-gate-verification.md`

## Goal

- Restore the Stage 1 tutorial overlay as a real onboarding aid.
- Improve HUD urgency cues so players can read timer pressure more easily.
- Improve result-screen titles so clear, score-fail, and time-up states are visually distinct.

## Completion Criteria

- Stage 1 play shows the tutorial overlay for the current run on:
  - move to grinder
  - stop in the green zone
  - rotate to lock
- Each tutorial hint appears at most once per run.
- Tutorial overlay hides cleanly on state transition, retry, result screen, and title flow.
- HUD timer changes color when the remaining time becomes low and critical.
- Result screen title and highlight colors distinguish:
  - stage clear
  - time up
  - low-score failure
- Unity batch compile succeeds.
