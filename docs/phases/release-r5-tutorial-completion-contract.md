# Release R5 - Tutorial Completion Contract

## Objective

Complete tutorial hint coverage for all drink-making mechanics. Currently only Grinding (grinder), Tamping (gauge), and Portafilter Lock (lock) have first-encounter tutorial hints. The remaining six mechanics need coverage: Extraction, Steam Milk, Pour Shot, Ingredient, Lid, and Serving.

## Completion Conditions

### CC-1: Extraction Tutorial
- **Key:** `"extraction"`
- **Trigger:** `DrinkFlowState.Extracting` in `TryShowTutorialHint`
- **Title:** "Tap To Extract"
- **Body:** Explains tap-to-start / tap-to-stop timing
- **Shown only once** via `SaveManager.HasSeenTutorial("extraction")` / `MarkTutorialSeen("extraction")`

### CC-2: Steam Milk Tutorial
- **Key:** `"steam"`
- **Trigger:** `DrinkFlowState.SteamMilk` in `TryShowTutorialHint`
- **Title:** "Steam The Milk"
- **Body:** Explains drag wand to pitcher, move up/down to control heat, tap to stop
- **Shown only once** via SaveManager

### CC-3: Pour Shot Tutorial
- **Key:** `"pour_shot"`
- **Trigger:** `DrinkFlowState.PourShotToCup` in `TryShowTutorialHint`
- **Title:** "Pour The Shot"
- **Body:** Explains drag shot glass to cup
- **Shown only once** via SaveManager

### CC-4: Ingredient Tutorial
- **Key:** `"ingredient"`
- **Trigger:** `DrinkFlowState.PourIngredient` in `TryShowTutorialHint`
- **Title:** "Add Ingredient"
- **Body:** Explains tap the ingredient source to pour
- **Shown only once** via SaveManager

### CC-5: Lid Tutorial
- **Key:** `"lid"`
- **Trigger:** `DrinkFlowState.Lid` in `TryShowTutorialHint`
- **Title:** "Place The Lid"
- **Body:** Explains drag lid onto cup
- **Shown only once** via SaveManager

### CC-6: Serving Tutorial
- **Key:** `"serving"`
- **Trigger:** `DrinkFlowState.Serving` in `TryShowTutorialHint`
- **Title:** "Serve The Drink"
- **Body:** Explains drag finished drink to customer area
- **Shown only once** via SaveManager

### CC-7: TryShowTutorialHint Called For All New States
- `TransitionDrinkState` must call `TryShowTutorialHint(state)` for Extracting, SteamMilk, PourShotToCup, PourIngredient, Lid, and Serving states (same pattern as existing MoveToGrinder/Tamping/PortafilterLocking)

### CC-8: Auto-Dismiss Timing
- All tutorial hints auto-dismiss after the same delay as existing tutorials (currently 4 seconds)
- Tutorial overlay does not block gameplay input

### CC-9: Stage-1 Guard Preserved
- Tutorial hints only appear on Stage 1, matching existing behavior (`stageManager.CurrentStage.Number != 1` guard)

### CC-10: No Regressions
- Existing grinding, tamping, and portafilter lock tutorials continue to work unchanged
- No modifications to mechanic files, TutorialOverlay, or SaveManager
- All changes confined to GameManager.TryShowTutorialHint and TransitionDrinkState

## Architecture

All changes are in `GameManager.cs`:
1. Add `TryShowTutorialHint(state)` calls in `TransitionDrinkState` for the six new states
2. Add six new `case` blocks in `TryShowTutorialHint` switch statement
3. Each case follows the identical pattern: check HasSeenTutorial, mark seen, call ShowTutorialHint with title/body/offsets/arrow
