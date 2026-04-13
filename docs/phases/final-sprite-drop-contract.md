# Final Sprite Drop Contract

- Date: 2026-04-12
- Baseline: `docs/release-prep-checklist.md`, `docs/phases/release-prep-handoff-verification.md`

## Goal

- Remove graybox fallback from the 43 production-tracked sprite names by filling `Assets/Resources/Sprites/Final`.
- Use the current runtime-ready painted PNG set as the initial final art drop.
- Preserve Unity import behavior for the promoted sprites.

## Completion Criteria

- `Assets/Resources/Sprites/Final` contains the 43 tracked shipping sprite PNGs.
- The promoted final sprites preserve sprite-import metadata instead of relying on fresh default imports.
- `ReleasePrepAudit` reports final art coverage as `43/43`.
- Unity batch compile succeeds against the validation clone after the final sprite drop.
