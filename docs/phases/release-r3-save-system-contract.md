# Release R3 Save System Contract

- Date: 2026-04-12
- Baseline: GameManager.cs, StageManager.cs, ScoreManager.cs, ResultScreenView.cs, TutorialOverlay.cs

## Goal

- Persist player progress across sessions using a JSON save file at `Application.persistentDataPath`.
- Track per-stage best score, best star rating, and highest cleared stage.
- Track tutorial completion flags so hints are not re-shown after first viewing.
- Load save data on app start, save on stage complete.

## Data Model

SaveData contains:
- `highestClearedStage` (int): index of the highest stage completed with at least 1 star.
- `stageBestScores` (int[]): per-stage best score, indexed by stage number minus one.
- `stageBestStars` (int[]): per-stage best star rating, indexed by stage number minus one.
- `tutorialFlags` (string[]): list of mechanic keys whose tutorial hint has already been shown.
- `version` (int): save format version for future migration.

## Completion Criteria

- A `SaveManager` class exists in `Assets/Scripts/Core/SaveManager.cs`.
- SaveManager uses `JsonUtility` to serialize/deserialize a `SaveData` class to/from `Application.persistentDataPath/save.json`.
- SaveManager is initialized in `GameManager.InitializeIfNeeded()` before other systems.
- SaveManager is accessible via `SaveManager.Instance` static property.
- On stage complete, if the result is a pass (at least 1 star), the save updates:
  - `highestClearedStage` if higher than current value.
  - `stageBestScores[stageIndex]` if score is higher than current best.
  - `stageBestStars[stageIndex]` if stars are higher than current best.
- Tutorial flags are written when a tutorial hint is first shown, and checked before showing hints.
- Tutorial flags persist across sessions (not just per-run booleans).
- `SaveManager.IsStageUnlocked(int stageIndex)` returns true if stageIndex is 0 or the previous stage has at least 1 star saved.
- `SaveManager.HasSeenTutorial(string key)` and `SaveManager.MarkTutorialSeen(string key)` manage tutorial flags.
- `SaveManager.GetBestScore(int stageIndex)` and `SaveManager.GetBestStars(int stageIndex)` return saved best values.
- Save file is written to disk only on explicit `Save()` call (not every frame).
- A `ClearSave()` method exists for debugging/testing.
- Unity batch compile succeeds with no errors.
- Existing game flow is not broken; all mechanics, scoring, and UI continue to work.
