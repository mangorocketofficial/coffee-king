# Hot Drinks Expansion Contract

- Date: 2026-04-10
- Baseline: `C:\Users\User\.claude\plans\jolly-finding-lighthouse-revised.md`
- Current project baseline: `docs/phases/reboot-phase1-asset-integration-verification.md`

## Binding Rules

- Follow `agent.md`: contract, implementation, verification, verification document.
- Preserve the reboot slice architecture: `GameManager` remains top-level owner, `DrinkFlowController` owns per-drink sequence only.
- `Lid` must be implemented as a real mechanic, not an automatic coroutine-only step.
- Hot cup assets must use a new naming family and must not reuse legacy `cup_empty` / `cup_americano` / `cup_latte` identifiers.

## Completion Criteria

### Data And Flow

- `DrinkRecipe` supports:
  - canonical id
  - display name
  - `CupType`
  - `IngredientType`
  - step list
- `DrinkFlowController` uses the recipe step list dynamically instead of a hardcoded Americano sequence.
- `DrinkFlowState` includes `SteamMilk` and `Lid`.

### Drinks

- The game supports 4 drinks:
  - `iced_americano`
  - `iced_cafe_latte`
  - `hot_americano`
  - `hot_cafe_latte`

### Mechanics

- `SteamMilkMechanic` is reconnected to the active flow.
- `IngredientMechanic` is generalized to use a supplied source renderer.
- `LidMechanic` exists and supports:
  - lid spawn
  - drag
  - snap on cup
  - completion event

### Scene / Visual Layer

- `GrayboxSceneContext` and `GameSceneBuilder` expose:
  - `HotWaterDispenserRenderer`
  - `LidRenderer`
- `RunCupSetup()` supports:
  - plastic cup with ice
  - hot cup without ice

### Recipes / Stage / Scoring

- `DrinkLibrary` defines all 4 drinks using canonical ids.
- `StageLibrary` uses the canonical ids and stage rollout from the revised plan.
- `ScoreRules.GetMaximumRecipeScore()` is recipe-step aware and includes steam score for latte recipes.

## Validation Target

- Unity batch compile succeeds.
- The codebase reaches hot/iced branching, steam branch, and lid step without compile errors.
