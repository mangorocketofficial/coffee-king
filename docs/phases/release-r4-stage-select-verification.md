# Release R4 - Stage Select Screen Verification

## C1: StageSelectView exists - PASS
- Created `Assets/Scripts/UI/StageSelectView.cs` in namespace `CoffeeKing.UI`.
- Follows existing view patterns: private constructor, static `Create()`, `Show()`/`Hide()`, events.
- Creates its own overlay canvas at `sortingOrder = 600` (between HUD at 500 and result at 700).
- Shows 5 stage nodes in a horizontal row with connector lines between them.
- Each node displays: stage number, star rating (using `StarRating.ToDisplayString`), best score.
- Locked stages shown with `NodeLockedColor` (grayed out) and button set to `interactable = false`.
- Unlocked stages use `NodeUnlockedColor` and are clickable.
- Fires `StageSelected(int stageIndex)` event when an unlocked stage button is clicked.
- Has a "Back" button that fires `BackRequested` event.

## C2: GameState enum updated - PASS
- Added `StageSelect` value to `GameState` enum in `GameManager.cs`, positioned between `TitleScreen` and `WaitingForOrder`.

## C3: UIContext updated - PASS
- `UIContext` constructor now accepts `StageSelectView` parameter.
- `StageSelectView` property added to `UIContext`.
- `UIBuilder.Build()` creates `StageSelectView.Create(canvasObject.transform)` and passes it into `UIContext`.

## C4: GameManager flow wired - PASS
- Title "Start" button: `HandleTitleStartRequested` calls `ShowStageSelect()` instead of directly starting stage 1.
- Stage Select: `HandleStageSelected(int)` calls `stageManager.StartStageByIndex(stageIndex)` to begin the chosen stage.
- Stage Select "Back": `HandleStageSelectBackRequested` returns to title screen via `ShowTitleScreen()`.
- Result "Next" button: `HandleNextStageRequested` now calls `ShowStageSelect()` to return to stage select.
- Result "Retry" button: `HandleRetryRequested` still retries the current stage directly (unchanged).
- Pause "Main Menu": Goes to `ShowTitleScreen()` (unchanged).
- `ShowTitleScreen` hides stage select via `uiContext.StageSelectView.Hide()`.
- `StartStage` hides stage select via `uiContext.StageSelectView.Hide()`.
- `StageManager.StartStageByIndex(int)` added to support starting arbitrary stage indices.
- Result screen "Next" button label changed to "Stage Select" and is always shown.
- OnDestroy properly unsubscribes and disposes `StageSelectView`.

## C5: SaveManager integration - PASS
- `RefreshNodes()` in `StageSelectView` uses `SaveManager.Instance.IsStageUnlocked(i)` for lock state.
- Uses `SaveManager.Instance.GetBestStars(i)` to display star rating per node.
- Uses `SaveManager.Instance.GetBestScore(i)` to display best score per node.

## C6: Flow correctness - PASS
- Flow: Title -> Stage Select -> Stage Intro -> Playing -> Result -> Stage Select.
- Stage 1 is always unlocked (SaveManager.IsStageUnlocked returns true for index 0).
- Stages 2-5 require previous stage cleared with >= 1 star (SaveManager logic).
- Locked stages cannot be selected (button.interactable = false).

## C7: No regressions - PASS
- All existing gameplay mechanics untouched.
- Pause menu flow preserved (resume, restart, main menu all work as before).
- Save system unchanged.
- Result screen retry button unchanged.
- HUD visibility correctly excludes StageSelect state.
- All event subscriptions properly paired with unsubscriptions in OnDestroy.

## Files Modified
- `Assets/Scripts/Core/GameManager.cs` - Added StageSelect state, ShowStageSelect flow, event wiring
- `Assets/Scripts/UI/UIBuilder.cs` - UIContext expanded with StageSelectView, UIBuilder.Build creates it
- `Assets/Scripts/UI/ResultScreenView.cs` - Next button label changed to "Stage Select"
- `Assets/Scripts/Stage/StageManager.cs` - Added StartStageByIndex method

## Files Created
- `Assets/Scripts/UI/StageSelectView.cs` - New stage select screen view
