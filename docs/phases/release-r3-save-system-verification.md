# Release R3 Save System Verification

- Date: 2026-04-12
- Contract: `docs/phases/release-r3-save-system-contract.md`

## Checklist

- [x] `SaveManager` class exists at `Assets/Scripts/Core/SaveManager.cs`.
- [x] Uses `JsonUtility` to serialize/deserialize `SaveData` to `Application.persistentDataPath/save.json`.
- [x] `SaveManager` is initialized in `GameManager.InitializeIfNeeded()` before other systems (line 226).
- [x] `SaveManager.Instance` static property is set in the constructor.
- [x] On stage complete with a pass, `RecordStageResult` is called (GameManager.cs line 693-696).
  - [x] Updates `highestClearedStage` if the new stage index is higher.
  - [x] Updates `stageBestScores[stageIndex]` if the new score is higher.
  - [x] Updates `stageBestStars[stageIndex]` if the new stars are higher.
- [x] Tutorial flags use `HasSeenTutorial(key)` and `MarkTutorialSeen(key)` instead of per-run booleans.
  - [x] "grinder" key for MoveToGrinder tutorial.
  - [x] "gauge" key for Tamping tutorial.
  - [x] "lock" key for PortafilterLocking tutorial.
- [x] Tutorial flags persist across sessions (saved to JSON file on `MarkTutorialSeen`).
- [x] `IsStageUnlocked(int stageIndex)` returns true if stageIndex is 0 or previous stage has >= 1 star saved.
- [x] `GetBestScore(int stageIndex)` and `GetBestStars(int stageIndex)` return saved best values.
- [x] Save file is written only on explicit `Save()` call, not every frame.
- [x] `ClearSave()` method exists for debugging/testing.
- [x] `SaveData` includes version field for future migration.
- [x] `EnsureArraySizes()` handles loading saves from older versions with fewer stages.
- [x] Old per-run tutorial booleans (`grinderTutorialShown`, `gaugeTutorialShown`, `lockTutorialShown`) are fully removed.
- [x] Existing game flow is preserved: all mechanics, scoring, stage progression, and UI continue to work unchanged.

## Files Changed

- `Assets/Scripts/Core/SaveManager.cs` (new) - SaveData model and SaveManager class.
- `Assets/Scripts/Core/GameManager.cs` (modified) - Integrated SaveManager initialization, stage result recording, and persistent tutorial flags.

## Notes

- Save data is written to `Application.persistentDataPath/save.json` which is platform-appropriate (e.g., `%APPDATA%/../LocalLow/` on Windows).
- `RecordStageResult` only persists when `stars > 0` (i.e., the player passed), preventing failed attempts from overwriting best scores.
- `MarkTutorialSeen` calls `Save()` immediately so tutorial flags are not lost on unexpected quit.
- Stage unlock via `IsStageUnlocked` is available for future stage-select UI integration.
