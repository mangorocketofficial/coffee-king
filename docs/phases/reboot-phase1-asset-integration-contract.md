# Reboot Phase 1 Asset Integration Contract

- Date: 2026-04-10
- Baseline: `update_plan.md`
- Slice baseline: `docs/phases/reboot-phase1-verification.md`
- Scope: integrate the real PNG asset set from `Assets/images` into the current reboot R1 playable slice

## Binding Rules

- Follow `agent.md`: contract, implementation, verification, verification document.
- Keep `Assets/images` as the source asset folder.
- Runtime loading must continue to flow through `SpriteFactory`.
- Missing assets must still fall back to generated placeholders so the game remains playable.

## Completion Criteria

### Source Asset Integration

- Real PNG assets from `Assets/images` are available to runtime loading through the `Resources/Sprites/Graybox` path.
- Asset-name mapping aligns the current code-facing names with the real filenames where they differ.

### Runtime Sizing

- Loaded sprites are resized at runtime to fit the intended world-space target size.
- This prevents the imported PNGs from appearing wildly larger or smaller than the placeholder geometry they replace.
- Existing placeholder fallback behavior still works.

### Reboot Slice Coverage

- At minimum, the current reboot R1 slice can display real assets for:
  - machine empty / locked
  - grinder empty / locked
  - portafilter empty / filled / tamped
  - tamper
  - shot glass empty / filling / full
  - cup plastic empty / ice / shot / americano
  - water bottle
  - customer

### Safety

- Assets that do not yet have final code usage may remain unused, but their presence must not break compile or runtime fallback.
- Existing stage flow and gameplay logic must remain unchanged.

## Validation Target

- Unity batch compile succeeds.
- Runtime asset loading continues to function when real PNGs are present.
