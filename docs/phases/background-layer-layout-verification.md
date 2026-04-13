# Background Layer Layout Verification

- Date: 2026-04-11
- Contract: `docs/phases/background-layer-layout-contract.md`
- Validation project: `C:\Users\User\Desktop\Games\coffee-king-compilecheck`

## Implemented

- Background asset mapping now uses:
  - `wall_background`
  - `counter_edge`
  - `counter_surface`
- `GameSceneBuilder` now renders the background stack as:
  - wall background
  - counter edge
  - counter surface
- Background sprites use exact-size placement so the wall / counter split reads correctly on screen.
- `counter_edge` was processed into a transparent runtime sprite in `Assets/Resources/Sprites/Graybox/counter_edge.png`.
- Customer body, bubble, patience bar, and labels were moved above gameplay props via higher sorting orders.

## Verification Evidence

- Unity batch compile completed successfully against the validation clone.
- Log file: `C:\Users\User\Desktop\Games\coffee-king-compilecheck\unity-background-layout.log`
- Result: `return code 0`

## Residual Risk

- This turn verified compile and asset wiring, but not a full manual playtest in the editor.
- `wall_background` and `counter_surface` are opaque full-frame textures, while `counter_edge` is the only runtime-processed transparent layer.
