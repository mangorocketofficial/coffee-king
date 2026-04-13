# Final Sprite Drop Verification

- Date: 2026-04-12
- Contract: `docs/phases/final-sprite-drop-contract.md`
- Baseline docs: `docs/release-prep-checklist.md`, `docs/phases/release-prep-handoff-verification.md`

## Implemented Scope

- Promoted the 43 production-tracked runtime sprite PNGs from `Assets/Resources/Sprites/Graybox` into `Assets/Resources/Sprites/Final`.
- Preserved Unity sprite import behavior by cloning the source `.meta` content and regenerating destination GUIDs for the promoted assets.
- Left non-tracked graybox extras out of the final drop:
  - `water_cup_empty_runtime`
  - `water_cup_full_runtime`
  - `hotmilk_pitcher_cold`
- The workspace `Final` directory now contains `43` PNG files matching the release audit list.
- Runtime already preferred final sprites before graybox fallback, so no gameplay code change was needed beyond the populated asset drop.

## Verification Evidence

- Unity batch compile succeeded against the validation clone after importing the final sprite drop.
- Compile log: `C:\Users\User\Desktop\Games\coffee-king\build\outputs\logs\final-sprite-drop-unity.log`
- Result: `return code 0`
- Release audit succeeded against the same validation clone.
- Audit log: `C:\Users\User\Desktop\Games\coffee-king\build\outputs\logs\final-sprite-drop-audit-unity.log`
- Audit report copy: `C:\Users\User\Desktop\Games\coffee-king\build\outputs\logs\release-prep-audit.txt`
- Audit result:
  - `Final Art Coverage: 43/43`
  - remaining warning count: `1`

## Remaining Audit Warning

- Android custom keystore is still not configured yet.

## Residual Risk

- This slice removes graybox fallback for the tracked 43 sprite names, but it does not guarantee the art is the final commercial art direction forever; it is the initial final drop based on the current runtime-ready painted set.
- A manual in-editor and on-device visual pass is still needed to confirm pivots, layering, and readability with the new `Final` path active.
