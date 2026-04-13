# Release R4 - Stage Select Screen Contract

## Overview
Add a stage select screen between the Title Screen and Stage Intro, allowing players to choose which stage to play and see their progress.

## Completion Conditions

### C1: StageSelectView exists
- New file `Assets/Scripts/UI/StageSelectView.cs` in namespace `CoffeeKing.UI`.
- Follows existing view patterns (private constructor, static `Create`, `Show`/`Hide`, events).
- Creates its own overlay canvas at sortingOrder 600.
- Shows 5 stage nodes in a horizontal row.
- Each node displays: stage number, star rating (0-3 or locked icon), best score.
- Locked stages shown grayed out and not interactable.
- Unlocked stages are clickable buttons.
- Fires `StageSelected(int stageIndex)` event when a stage is clicked.
- Has a "Back" button that fires `BackRequested` event to return to title.

### C2: GameState enum updated
- New `StageSelect` value added to `GameState` enum in `GameManager.cs`.

### C3: UIContext updated
- `UIContext` constructor and properties include `StageSelectView`.
- `UIBuilder.Build` creates `StageSelectView` and passes it into `UIContext`.

### C4: GameManager flow wired
- Title "Start" button goes to Stage Select (not directly to stage 1).
- Stage Select fires `StageSelected` -> GameManager starts the chosen stage.
- Stage Select "Back" button returns to Title Screen.
- Result screen "Next" button goes to Stage Select (not directly to next stage).
- Result screen "Retry" button still retries the current stage directly.
- Pause menu "Main Menu" goes to Title Screen (unchanged).
- `ShowTitleScreen` hides stage select.
- `StageManager.StartStage(int)` method added or existing method used to start arbitrary stage index.

### C5: SaveManager integration
- Stage select uses `SaveManager.IsStageUnlocked(i)` for lock state.
- Stage select uses `SaveManager.GetBestStars(i)` for star display.
- Stage select uses `SaveManager.GetBestScore(i)` for score display.

### C6: Flow correctness
- Flow is: Title -> Stage Select -> Stage Intro -> Playing -> Result -> Stage Select.
- Stage 1 is always unlocked.
- Stages 2-5 require previous stage cleared with >= 1 star.
- Locked stages cannot be selected.

### C7: No regressions
- All existing gameplay, pause, save, and result screen functionality preserved.
- No compile errors.
