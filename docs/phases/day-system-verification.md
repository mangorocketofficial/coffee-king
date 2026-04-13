# Day-based Earnings System - Verification

## Contract Checklist

### 1. Day Progression
- [x] Days progress infinitely: `StageLibrary.CreateDay(dayNumber, recipes)` generates config for any day number
- [x] No fail state: `StageResult.Passed` always returns `true`, `CompleteStage` always calls `PlayStageComplete`
- [x] Day 1 = tutorial day: `GetAllowedRecipes(1)` returns only `iced_americano`, `GetCustomerCount(1)` returns 3
- [x] Gradual scaling: customer count = 2 + day, drink variety expands at days 3, 5, 7
- [x] Not capped at 5: `StageLibrary.CreateDay` works for any day number

### 2. Money/Earnings System
- [x] Score system unchanged: `ScoreRules`, `QualityGrade`, scoring logic in `ScoreManager` all preserved
- [x] DrinkRecipe has `PriceWon` field (int)
- [x] Iced/Hot Americano = 4,500 won, Iced/Hot Latte = 5,500 won (in DrinkLibrary)
- [x] `ScoreManager.DailyEarnings` tracks per-day earnings, incremented in `FinalizeServedRound`
- [x] `SaveManager.TotalEarnings` tracks accumulated total, updated in `RecordDayResult`
- [x] `ScoreManager.FormatWon()` formats with comma separators and won character

### 3. Flow Change
- [x] Title -> Start -> `stageManager.StartDay(saveManager.NextDay)` (skips stage select)
- [x] End of day -> settlement screen -> "Next Day" button -> `stageManager.StartNextDay()`
- [x] On relaunch: `saveManager.NextDay` = `highestCompletedDay + 1`
- [x] StageSelectView.cs file kept, `ShowStageSelect` method still exists but not called in main flow

### 4. HUD Changes
- [x] Shows "Day X" via `StageData.DisplayName` which returns `$"Day {Number}"`
- [x] Shows today's earnings via `HUDView.SetEarnings()` with formatted won amount
- [x] Timer and progress bar remain (SetTimer, SetProgress unchanged)

### 5. Result/Settlement Screen
- [x] Shows day number in title: "Day X Complete"
- [x] Shows score (existing score card)
- [x] Shows earnings breakdown via earnings card with daily and total
- [x] Shows drinks served count in summary text
- [x] Shows daily total earnings (animated roll-up)
- [x] Shows accumulated total earnings (animated roll-up)
- [x] "Next Day" button (nextLabelText.text = "Next Day")

### 6. Save System
- [x] `SaveData.highestCompletedDay` tracks current day
- [x] `SaveData.totalEarnings` tracks accumulated earnings (long)
- [x] `SaveManager.RecordDayResult(dayNumber, dailyEarnings)` persists both
- [x] Tutorial flags preserved (tutorialFlags list unchanged)
- [x] Settings preserved (bgmVolume, sfxVolume, vibrationEnabled unchanged)

### 7. Day Scaling Formula
- [x] Day 1: 3 customers (2+1), patience 75s (80-5), spawn 8s (8.5-0.5), time 150s (3*50)
- [x] Day 2: 4 customers, patience 70s, spawn 7.5s
- [x] Day 3: 5 customers, + Iced Latte, patience 65s
- [x] Day 5+: + Hot Americano
- [x] Day 7+: + Hot Latte
- [x] Patience caps at 40s, spawn interval caps at 3s

### 8. Currency Display
- [x] Korean won: `\uc6d0` (won character) used in `FormatWon`
- [x] Comma separators: `{amount:N0}` format specifier

### 9. Preserved Systems
- [x] Tutorial: `TryShowTutorialHint` checks `stageManager.CurrentStage.Number != 1` (Day 1 = Number 1)
- [x] Pause: `PauseGame`/`ResumeGame` logic unchanged
- [x] Settings: `ShowSettings`, volume/vibration handlers unchanged
- [x] Sound: All audio calls preserved
- [x] Mechanics: All mechanic handlers unchanged

## Files Modified
| File | Change |
|------|--------|
| `Assets/Scripts/Orders/DrinkRecipe.cs` | Added `PriceWon` property and constructor param |
| `Assets/Scripts/Orders/DrinkLibrary.cs` | Added price values to all 4 recipes |
| `Assets/Scripts/Stage/StageData.cs` | Changed DisplayName from "Stage X" to "Day X" |
| `Assets/Scripts/Stage/StageLibrary.cs` | Replaced fixed 5-stage array with dynamic `CreateDay(dayNumber)` |
| `Assets/Scripts/Stage/StageManager.cs` | Replaced fixed stage list with recipe-based day system, added earnings to StageResult |
| `Assets/Scripts/Core/SaveManager.cs` | Replaced stage arrays with `highestCompletedDay` and `totalEarnings` |
| `Assets/Scripts/Scoring/ScoreManager.cs` | Added `DailyEarnings` tracking and `FormatWon` utility |
| `Assets/Scripts/Core/GameManager.cs` | Flow: title->day (skip stage select), next day button, earnings in HUD/result |
| `Assets/Scripts/UI/HUDView.cs` | Added earnings text display |
| `Assets/Scripts/UI/ResultScreenView.cs` | Replaced stars with earnings card, "Next Day" button |
| `Assets/Scripts/UI/ResultAnimator.cs` | Replaced star animation with earnings roll-up animation |
| `Assets/Scripts/UI/StageSelectView.cs` | Updated RefreshNodes for compatibility (no longer references old SaveManager APIs) |
