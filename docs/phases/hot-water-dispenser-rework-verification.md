# Hot Water Dispenser Rework Verification

- Date: 2026-04-10
- Contract: `docs/phases/hot-water-dispenser-rework-contract.md`
- Validation project: `C:\Users\User\Desktop\Games\coffee-king-compilecheck`

## Implemented

- The hot water dispenser is now created as a persistent scene prop and is no longer hidden by ingredient-step cleanup.
- The hot water dispenser size now matches the grinder footprint.
- Added `HotWaterCupMechanic` for the hot americano water phase:
  - empty cup spawns near the dispenser
  - player drags it onto the dispenser
  - the cup becomes full
  - player taps the full cup to pour into the drink
- The old hot-water drag-pour branch was removed from `GameManager`.

## Verification Evidence

- Unity batch compile completed successfully against the validation clone.
- Log file: `C:\Users\User\Desktop\Games\coffee-king-compilecheck\unity-hot-water-rework.log`
- Result: `return code 0`

## Residual Risk

- This turn verified compile and flow wiring, but not a full manual playtest in the editor.
- The new `water_cup_full` visual depends on Unity reimporting the latest processed PNG in the editor session.
