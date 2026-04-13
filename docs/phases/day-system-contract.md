# Day-based Earnings System - Contract

## Overview
Convert the fixed 5-stage system with star ratings to an infinite day-based system with money earnings.

## Completion Conditions

### 1. Day Progression
- [x] Days progress infinitely: Day 1, Day 2, Day 3...
- [x] No fail state - timed out customers simply earn no money
- [x] Day 1 = tutorial day, Iced Americano only, 3 customers
- [x] Gradual scaling: more customers, more drink variety as days increase
- [x] Day scaling is not capped at 5

### 2. Money/Earnings System
- [x] Existing score system stays exactly as-is (Perfect/Good/Bad grades, mechanic points)
- [x] DrinkRecipe has a price field (int, in won)
- [x] Americano = 4,500 won, Latte = 5,500 won
- [x] Track daily earnings per successful serve
- [x] Track total accumulated earnings across all days
- [x] ScoreManager tracks earnings alongside score

### 3. Flow Change
- [x] Title Screen -> Start -> immediately start current day (skip stage select)
- [x] End of day -> settlement screen with score + earnings -> Next Day button
- [x] On app relaunch: start from (last completed day + 1)
- [x] StageSelectView file kept but not used in flow

### 4. HUD Changes
- [x] Show "Day X" instead of stage info
- [x] Show today's earnings alongside score
- [x] Timer and progress bar remain

### 5. Result/Settlement Screen
- [x] Show day number
- [x] Show score (existing)
- [x] Show drinks served count and earnings breakdown
- [x] Show daily total earnings
- [x] Show accumulated total earnings
- [x] "Next Day" button instead of "Stage Select"

### 6. Save System
- [x] Track current day number (highest completed day)
- [x] Track total accumulated earnings
- [x] Tutorial flags remain as-is
- [x] Settings (bgm, sfx, vibration) remain as-is

### 7. Day Scaling Formula
- Day 1: 3 customers, Iced Americano only, patience 75s, time limit 180s
- Day 2: 4 customers, Iced Americano only, patience 70s
- Day 3: 5 customers, Iced Americano + Iced Latte, patience 65s
- Day 4+: customers = 3 + day, gradually add hot drinks
- Day 5+: add Hot Americano
- Day 7+: add Hot Latte
- Cap customer patience at ~40s minimum
- Cap spawn interval at ~3s minimum

### 8. Currency Display
- Korean won (won) symbol used
- Comma separators for amounts (e.g., "4,500won")

### 9. Preserved Systems (must not break)
- Tutorial system
- Pause system
- Settings
- Sound/audio
- All mechanic behaviors
- Score grading (Perfect/Good/Bad)

## Files Modified
- SaveManager.cs - new day/earnings fields
- DrinkRecipe.cs - price field
- DrinkLibrary.cs - prices on recipes
- StageLibrary.cs - dynamic day generation
- StageData.cs - DisplayName shows "Day X"
- StageManager.cs - remove "has next stage" cap
- ScoreManager.cs - earnings tracking
- GameManager.cs - flow changes (skip stage select, next day)
- HUDView.cs - earnings display
- ResultScreenView.cs - settlement screen with earnings
- ResultAnimator.cs - earnings animation
