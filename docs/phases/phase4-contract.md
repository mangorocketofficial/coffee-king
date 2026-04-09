# Phase 4 Completion Contract

- Date: 2026-04-09
- Scope: Phase 4 asset swap layer from `development.md`
- Baseline: `development.md` and `docs/phases/phase3-verification.md`

## Binding Rules

- Follow the repository phase sequence from `agent.md`: contract, implementation, verification, verification document.
- Preserve current gameplay logic. Phase 4 is a visual asset swap layer, not a mechanic rewrite.
- Keep the game fully functional without shipped art assets by providing runtime fallbacks.

## Completion Criteria

### Sprite Loading Layer

- `SpriteFactory` supports named asset loading from:
  - `Resources/Sprites/Graybox/`
  - `Resources/Sprites/Final/`
- A single toggle controls whether the game prefers graybox assets or final assets.
- If a requested sprite is missing, a runtime placeholder is generated so gameplay still runs.

### Named Asset Usage

- Core gameplay sprite renderers use stable asset names rather than only ad-hoc generated rectangles.
- At minimum, Phase 4-ready names cover:
  - `bg_wall`
  - `bg_counter`
  - `machine`
  - `machine_slot`
  - `portafilter`
  - `cup_empty`
  - `cup_americano`
  - `cup_latte`
  - `cup_vanilla`
  - `steam_wand`
  - `pitcher`
  - `tray_beans`
  - `tray_milk`
  - `tray_syrup`
  - `customer_01`
  - `speech_bubble`

### Swap Readiness

- Visual builders and mechanics request sprites through `SpriteFactory` so drop-in art replacement does not require gameplay code changes.
- Resource folder structure exists in the project for both graybox and final sprite sets.

## Validation Target

- Unity batch compile succeeds for the updated project.
- The project still runs without external art assets because runtime placeholder generation remains available.
