# Release R7 - Scoring Rule Expansion Contract

## Overview
Expand the scoring system with three new features: wrong drink detection, perfect streak bonus, and no-mistake stage bonus.

## Completion Conditions

### 1. Wrong Drink Detection
- **CC-1.1**: When a drink is served, compare the recipe that was made (`DrinkFlowController.CurrentRecipe`) against the customer's order (`Customer.Order`).
- **CC-1.2**: If the drink type does not match the order, the round score is zeroed and a `ScoreRules.WrongDrinkPenalty` is applied.
- **CC-1.3**: The customer view shows a rejection visual (red tint on body) before the customer exits.
- **CC-1.4**: HUD feedback text displays "Wrong drink!" in the bad highlight color.
- **CC-1.5**: The stage summary event line shows "WRONG DRINK" for that customer.

### 2. Perfect Streak Bonus
- **CC-2.1**: `ScoreManager` tracks all `QualityGrade` values recorded during a single drink round.
- **CC-2.2**: When finalizing a served round, if every mechanic grade is `Perfect`, a flat bonus (`ScoreRules.PerfectDrinkBonus`) is awarded.
- **CC-2.3**: The bonus appears in the round breakdown as "AllPerfect+{bonus}".
- **CC-2.4**: The stage summary event line includes "ALL PERFECT" when the bonus is awarded.

### 3. No Mistake Bonus
- **CC-3.1**: `ScoreManager` tracks a stage-level flag `HasAnyBadGrade` that is set to true whenever a `QualityGrade.Bad` result is recorded.
- **CC-3.2**: At stage completion (not time-expired), if `HasAnyBadGrade` is false, a flat bonus (`ScoreRules.NoMistakeBonus`) is added to the stage score.
- **CC-3.3**: The result screen summary includes a "No Mistake Bonus: +{bonus}" line when awarded.
- **CC-3.4**: `StageMaxScore` accounts for the possible no-mistake bonus.

### 4. Score Constants
- **CC-4.1**: `ScoreRules` defines `WrongDrinkPenalty = 150`.
- **CC-4.2**: `ScoreRules` defines `PerfectDrinkBonus = 75`.
- **CC-4.3**: `ScoreRules` defines `NoMistakeBonus = 200`.

### 5. Integration
- **CC-5.1**: All existing scoring, serving, and customer flows continue to work unchanged for correct drinks.
- **CC-5.2**: The result screen shows the no-mistake bonus as a separate summary line.
- **CC-5.3**: `GetMaximumRecipeScore` is unchanged; `NoMistakeBonus` is added to `StageMaxScore` separately in `BeginStage`.

## Files Modified
- `Assets/Scripts/Scoring/ScoreRules.cs` - new constants
- `Assets/Scripts/Scoring/ScoreManager.cs` - per-drink grade tracking, stage-level bad flag, wrong drink handling, perfect streak and no-mistake bonus logic
- `Assets/Scripts/Core/GameManager.cs` - wrong drink detection at serve time, no-mistake bonus at stage end, rejection visual
- `Assets/Scripts/Customer/CustomerView.cs` - rejection visual method
- `Assets/Scripts/Customer/CustomerSpawner.cs` - MarkRejected method

## Files Not Modified
- DrinkFlowController, DrinkFlowState, DrinkRecipe, DrinkLibrary, ServingMechanic, HUDView, StarRating, StageManager, ResultScreenView - no changes needed.
