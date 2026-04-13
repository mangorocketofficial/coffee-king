# Release R5 - Tutorial Completion Verification

## Verification Against Contract

### CC-1: Extraction Tutorial - PASS
- Key `"extraction"` checked via `saveManager.HasSeenTutorial("extraction")`
- Marked seen via `saveManager.MarkTutorialSeen("extraction")`
- Title: "Tap To Extract"
- Body: "Tap the button to start brewing, then tap again in the green zone to stop."
- Triggered from `DrinkFlowState.Extracting` case in `TryShowTutorialHint`

### CC-2: Steam Milk Tutorial - PASS
- Key `"steam"` checked via `saveManager.HasSeenTutorial("steam")`
- Marked seen via `saveManager.MarkTutorialSeen("steam")`
- Title: "Steam The Milk"
- Body: "Drag the steam wand into the pitcher, then move it up or down to control heat. Tap to stop in the green zone."
- Triggered from `DrinkFlowState.SteamMilk` case in `TryShowTutorialHint`

### CC-3: Pour Shot Tutorial - PASS
- Key `"pour_shot"` checked via `saveManager.HasSeenTutorial("pour_shot")`
- Marked seen via `saveManager.MarkTutorialSeen("pour_shot")`
- Title: "Pour The Shot"
- Body: "Drag the shot glass over the cup and release to pour the espresso."
- Triggered from `DrinkFlowState.PourShotToCup` case in `TryShowTutorialHint`

### CC-4: Ingredient Tutorial - PASS
- Key `"ingredient"` checked via `saveManager.HasSeenTutorial("ingredient")`
- Marked seen via `saveManager.MarkTutorialSeen("ingredient")`
- Title: "Add Ingredient"
- Body: "Tap the ingredient source to pour it into the cup."
- Triggered from `DrinkFlowState.PourIngredient` case in `TryShowTutorialHint`

### CC-5: Lid Tutorial - PASS
- Key `"lid"` checked via `saveManager.HasSeenTutorial("lid")`
- Marked seen via `saveManager.MarkTutorialSeen("lid")`
- Title: "Place The Lid"
- Body: "Drag the lid onto the top of the cup to seal the drink."
- Triggered from `DrinkFlowState.Lid` case in `TryShowTutorialHint`

### CC-6: Serving Tutorial - PASS
- Key `"serving"` checked via `saveManager.HasSeenTutorial("serving")`
- Marked seen via `saveManager.MarkTutorialSeen("serving")`
- Title: "Serve The Drink"
- Body: "Drag the finished drink to the serving area near the customer."
- Triggered from `DrinkFlowState.Serving` case in `TryShowTutorialHint`

### CC-7: TryShowTutorialHint Called For All New States - PASS
- `TryShowTutorialHint(state)` call added in `TransitionDrinkState` for:
  - `DrinkFlowState.Extracting` (line after `SetState(GameState.ExtractionStep)`)
  - `DrinkFlowState.SteamMilk` (line after `SetState(GameState.SteamMilkStep)`)
  - `DrinkFlowState.PourShotToCup` (line after `SetState(GameState.PourShotStep)`)
  - `DrinkFlowState.PourIngredient` (line after `SetState(GameState.IngredientStep)`)
  - `DrinkFlowState.Lid` (line after `SetState(GameState.LidStep)`)
  - `DrinkFlowState.Serving` (line after `SetState(GameState.ServingStep)`)
- All follow the same placement pattern as existing MoveToGrinder/Tamping/PortafilterLocking calls

### CC-8: Auto-Dismiss Timing - PASS
- All new tutorials use `ShowTutorialHint` which calls `HideTutorialHintAfterDelay(4f)` - same 4-second auto-dismiss as existing tutorials
- Tutorial overlay does not block gameplay input (it is a non-blocking UI overlay)

### CC-9: Stage-1 Guard Preserved - PASS
- The guard `stageManager.CurrentStage.Number != 1` at the top of `TryShowTutorialHint` applies to all cases, including the six new ones
- No modifications to this guard condition

### CC-10: No Regressions - PASS
- Existing grinding ("grinder"), tamping ("gauge"), and portafilter lock ("lock") cases unchanged
- No modifications to any mechanic files (ExtractionMechanic, SteamMilkMechanic, PourMechanic, IngredientMechanic, LidMechanic, ServingMechanic)
- No modifications to TutorialOverlay.cs or SaveManager.cs
- All changes confined to `GameManager.cs` only: `TransitionDrinkState` (6 new `TryShowTutorialHint` calls) and `TryShowTutorialHint` (6 new switch cases)

## Files Modified
- `Assets/Scripts/Core/GameManager.cs` - Added 6 `TryShowTutorialHint(state)` calls in `TransitionDrinkState` and 6 new case blocks in `TryShowTutorialHint`

## Files Created
- `docs/phases/release-r5-tutorial-completion-contract.md`
- `docs/phases/release-r5-tutorial-completion-verification.md`

## Tutorial Coverage Summary (Complete)
| Mechanic | Key | Status |
|---|---|---|
| Grinding (gauge hold) | `grinder` | Existing |
| Tamping (gauge hold) | `gauge` | Existing |
| Portafilter Lock (rotation) | `lock` | Existing |
| Extraction (tap start/stop) | `extraction` | NEW |
| Steam Milk (temperature gauge) | `steam` | NEW |
| Pour Shot (drag to cup) | `pour_shot` | NEW |
| Ingredient (tap to pour) | `ingredient` | NEW |
| Lid (drag to cup) | `lid` | NEW |
| Serving (drag to customer) | `serving` | NEW |
