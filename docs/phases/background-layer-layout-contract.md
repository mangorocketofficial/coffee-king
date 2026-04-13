# Background Layer Layout Contract

- Date: 2026-04-11
- Baseline: `update_plan.md`

## Goal

- Apply the newly added background assets:
  - `wall_background.png`
  - `counter_edge.png`
  - `counter_surface.png`
- Rebuild the world rendering order to match the requested layer structure:
  - Layer 0: wall background
  - Layer 1: counter edge
  - Layer 2: counter surface
  - Layer 3: machine, grinder, cup, props
  - Layer 4: draggable mechanics
  - Layer 5: customers, bubbles, UI

## Completion Criteria

- `SpriteAssetNames` points background loading at the new asset names.
- `GameSceneBuilder` uses the new background assets instead of the old placeholder wall/counter setup.
- `counter_edge` is rendered as a real asset layer, not a flat fallback rectangle.
- Background layer placement visually reads as top wall + bottom counter.
- Customers and bubbles render above gameplay objects.
- New background assets are copied into `Assets/Resources/Sprites/Graybox`.
- Unity batch compile succeeds.
