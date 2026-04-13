# Hot Water Dispenser Rework Contract

- Date: 2026-04-10
- Baseline: `docs/phases/hot-americano-water-pour-verification.md`

## Goal

- Keep the hot water dispenser visible on the game screen at all times.
- Increase the hot water dispenser visual scale to match the grinder footprint.
- Replace the hot americano water interaction with:
  - drag empty hot-water cup to dispenser
  - receive full hot-water cup
  - tap the full hot-water cup to pour into the drink

## Completion Criteria

- The hot water dispenser no longer hides between steps.
- The dispenser is resized to the grinder-scale footprint.
- Hot americano no longer uses `tap dispenser -> immediate full cup -> drag pour`.
- A dedicated hot-water cup interaction handles empty cup drag, fill, and tap-to-pour.
- Milk pitcher and water bottle ingredient flows continue to work without regression.
- Unity batch compile succeeds.
