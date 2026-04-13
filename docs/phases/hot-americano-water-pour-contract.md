# Hot Americano Water Pour Contract

- Date: 2026-04-10
- Baseline: `docs/phases/hot-drinks-expansion-v2-verification.md`

## Goal

- Update the hot americano flow so water is no longer added by a single tap.
- Required interaction:
  - tap the hot water dispenser
  - receive hot water
  - drag the received hot water into the americano cup

## Completion Criteria

- `hot_americano` keeps the existing drink sequence, but its ingredient branch becomes a two-step subflow.
- The hot water dispenser tap does not immediately finalize the drink.
- After tapping the dispenser, a pourable container appears near the dispenser.
- The player must drag that container into the cup to complete the hot-water step.
- Iced americano and milk-based drinks keep their current ingredient flow.
- Unity batch compile succeeds after the change.
