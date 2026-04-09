# Reboot Phase 1 Contract

- Date: 2026-04-09
- Baseline: `update_plan.md`
- Legacy baseline reference: `docs/phases/phase4-verification.md`
- Scope: V2 Reboot Phase R1 / Americano-only vertical slice

## Binding Rules

- Follow `agent.md` order: contract, implementation, verification, verification document.
- Treat this as the first implementation slice of the reboot baseline, not as an extension of the old latte flow.
- Preserve the existing stage loop, HUD, title, and asset loading infrastructure where reuse is practical.

## Completion Criteria

### Flow Layer

- `DrinkFlowState` exists for the reboot Americano sequence.
- `DrinkFlowController` owns the per-drink sequence and transitions.
- `GameManager` remains the top-level runtime owner.

### Mechanics

- `GrindingMechanic` is implemented:
  - drag or snap portafilter to grinder
  - hold to fill the grinding gauge
  - score on release
- `TampingMechanic` is implemented:
  - hold on tamper
  - pressure gauge rises while held
  - score on release
- Existing portafilter lock and extraction mechanics are reused in the new order.

### Americano Vertical Slice

- Reboot order flow reaches end-to-end playable `Iced Americano`:
  - move to grinder
  - grind
  - tamp
  - lock
  - extract
  - auto cup + ice setup
  - pour shot into cup
  - tap water bottle
  - serve
- No latte branch is active in this slice.
- No lid step is required in this slice.

### Stage Integration

- Stage loop still functions with queue, patience, and result flow.
- Available drink set is restricted to reboot `Iced Americano` only for this slice.
- Stage 1 tutorial text is not misleading for the reboot flow.

### Scoring

- Primary judged mechanics for this slice:
  - grinding
  - tamping
  - portafilter lock
  - extraction timing
- Stage scoring supports this four-mechanic Americano slice.
- Advanced bonus and wrong-drink rules remain out of scope.

## Validation Target

- Unity batch compile succeeds.
- The game reaches a playable Iced Americano-only sequence without compile errors.
