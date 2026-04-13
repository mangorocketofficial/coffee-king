# P0 Release Gate

- Date: 2026-04-12
- Baseline: `update_plan.md`, current runtime in `Assets/Scripts`

## Purpose

This document defines the minimum runtime gate for treating the current build as a release candidate instead of an internal alpha.

It does not claim the project is store-ready today.
It defines the P0 line the build must hold before release packaging, art replacement, and store submission work continue.

## Runtime P0

### 1. Stage End Conditions

- A stage clears only when all planned customers have been resolved.
- A stage fails immediately when the stage timer reaches zero.
- When the timer expires, every unresolved order counts as missed for score and summary purposes.

### 2. Failure Conditions

- `Time Up`: automatic failure, regardless of current score.
- `Low Score`: finishing the customer list below the 1-star threshold remains a failure.

### 3. Score Rules

- Mechanic judgments remain the base score source:
  - grinding
  - tamping
  - lock
  - extraction
  - steam for milk drinks
- Timeout or missed-at-closing orders apply the timeout penalty.
- Speed bonus must be real, not stubbed out.
- Stage max score must include the maximum possible speed bonus so result percentages stay honest.

### 4. Result Screen Expectations

- The result screen must clearly distinguish:
  - stage clear
  - failed by time
  - failed by score
- The summary must explain why the stage failed.

## Still Outside P0

These are still release blockers, but this document does not treat them as part of the runtime gate implementation slice:

- final production sprite set
- real audio pass
- tutorial onboarding flow
- Android package identity, keystore, icons, splash, store metadata
- manual device QA across all drinks and stages

## Exit Criteria For This Slice

- Runtime enforces the timer failure path.
- Remaining unresolved customers are settled consistently on timer expiry.
- Speed bonus is enabled and included in max-score math.
- Result screen can report timer failure separately from low-score failure.
- Unity batch compile succeeds after the changes.
