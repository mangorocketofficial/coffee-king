# Phase 2 Completion Contract

- Date: 2026-04-09
- Scope: Phase 2 game loop from `development.md`
- Baseline: `development.md` and `docs/phases/phase1-verification.md`

## Binding Rules

- Follow `agent.md` phase order: contract, implementation, verification, verification document.
- Build on top of the existing Phase 1 graybox mechanics instead of replacing them with editor-authored content.
- Keep all runtime flow programmatic inside the existing single-scene setup.

## Completion Criteria

### Customer Queue

- `CustomerSpawner` generates a stage customer list from stage rules and spawns customers into up to 3 simultaneous waiting slots.
- `CustomerView` shows each active customer with order speech bubble and a visible patience bar.
- Customer patience decreases while waiting and while an order is in progress.
- Customers exit visually after being served or timing out.

### Stage System

- `StageLibrary` defines 5 stages matching the design document:
  - Stage 1: 3 customers, americano only, 1 simultaneous, slow patience
  - Stage 2: 5 customers, americano + latte, 1 simultaneous, normal patience
  - Stage 3: 7 customers, all drinks, 2 simultaneous, normal patience
  - Stage 4: 10 customers, all drinks, 2 simultaneous, fast patience
  - Stage 5: 12 customers, all drinks, 3 simultaneous, fast patience
- `StageManager` runs `STAGE_INTRO -> PLAYING -> STAGE_COMPLETE -> RESULT_SCREEN`.
- Stage intro is visible before play begins.
- Stage completes when all customers have either been served or timed out.

### Scoring And Results

- `ScoreManager` tracks stage score, per-order score, speed bonus, and timeout penalty.
- `StarRating` converts stage score percentage into 0 to 3 stars using the documented thresholds.
- `ResultScreenView` displays stage result, star count, score summary, and supports `Next Stage` and `Retry`.

### Integration

- Existing Phase 1 mechanics remain playable inside the new stage loop.
- Orders are driven by the active customer selected from the queue.
- Timing out the active customer cancels the current in-progress drink and applies the timeout penalty.

## Validation Target

- Unity batch compile succeeds for the updated project.
- The project reaches a stage intro, stage play, and result screen flow without compile errors.
