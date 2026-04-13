# Release R6 - UI Polish and Transitions Verification

## Verification Against Contract

### 1. Screen Fade Transitions
- [x] `ScreenFader` utility class created in `Assets/Scripts/UI/ScreenFader.cs`
- [x] ScreenFader manages a full-screen black Image with CanvasGroup alpha animation via coroutines
- [x] ScreenFader created once by UIBuilder, available via `UIContext.ScreenFader`
- [x] Fade out (0.35s) -> switch screen -> fade in (0.35s) on all major transitions:
  - [x] Title -> Stage Select (`HandleTitleStartRequested`)
  - [x] Stage Select -> Stage Intro (`HandleStageSelected`)
  - [x] Stage Select -> Title (`HandleStageSelectBackRequested`)
  - [x] Result -> Stage Select (`HandleNextStageRequested`)
  - [x] Result -> Stage Intro (`HandleRetryRequested`)
  - [x] Stage complete -> Result screen (`CompleteStage`)
  - [x] Pause -> Main Menu (`HandlePauseMainMenuRequested`)
- [x] Fade duration configurable via `DefaultDuration` property (default 0.35s)
- [x] Uses `Time.unscaledDeltaTime` so fades work correctly even when paused

### 2. Score Popup Animation
- [x] `ScorePopup` MonoBehaviour created in `Assets/Scripts/UI/ScorePopup.cs`
- [x] On mechanic completion, a floating "+N" text appears near the feedback area
- [x] Text animates upward (80px) and fades out over 0.8s with ease-out quad curve
- [x] Color matches grade color (green for Perfect/Good, red for Bad)
- [x] Spawned from `HUDView.ShowScorePopup(int score, QualityGrade grade)`
- [x] Self-destroys after animation completes

### 3. Grade Feedback Effects
- [x] Red screen flash on Bad grade (0.15s, red-tinted overlay at 35% opacity)
- [x] Golden flash on Perfect grade (0.2s, golden-tinted overlay)
- [x] Good grade: no flash (score popup only)
- [x] Effects triggered via `ShowGradeFeedback(MechanicScoreResult)` helper in GameManager
- [x] Applied to all 5 scored mechanics: Grinding, Tamping, Lock, Extraction, Steam
- [x] Implemented via `ScreenFader.Flash()` which temporarily changes overlay color

### 4. Customer Expression Changes
- [x] Customer tint changes based on patience level in `SetPatience()`:
  - Normal (>=50% patience): white/neutral
  - Impatient (<50% patience): warm orange tint `(1, 0.90, 0.75)`
  - Critical (<20% patience): red tint `(1, 0.75, 0.70)`
- [x] Happy green tint on serve `(0.80, 1, 0.85)` in `MarkServed()`
- [x] Expression updates every frame via `CustomerSpawner.Tick` -> `SetPatience`
- [x] Expression changes do not interfere with focused or exiting states

### 5. Result Screen Animations
- [x] `ResultAnimator` MonoBehaviour created in `Assets/Scripts/UI/ResultAnimator.cs`
- [x] Attached to result screen canvas at creation time
- [x] Score counter roll-up: counts from 0 to final value over 1.0s with ease-out cubic
- [x] Star reveal: stars appear one at a time with 0.4s delay, plus white color flash on each earned star
- [x] `ResultScreenView.Show()` triggers animation via `resultAnimator.PlayReveal()`
- [x] `ResultScreenView.Hide()` stops animation via `resultAnimator.StopAnimation()`
- [x] Uses `Time.unscaledDeltaTime` and `WaitForSecondsRealtime` for pause-safety

### Architecture Compliance
- [x] All new UI is procedural C# (no editor scene setup)
- [x] ScreenFader uses its own Canvas with sorting order 950 (above all others)
- [x] Existing class constructors updated additively (UIContext, ResultScreenView)
- [x] No DOTween dependency - all animations use manual coroutine lerps
- [x] No particle systems
- [x] No changes to game mechanics or scoring logic
- [x] Existing functionality preserved

## Files Created
- `Assets/Scripts/UI/ScreenFader.cs` - Screen fade transition manager
- `Assets/Scripts/UI/ScorePopup.cs` - Floating score popup animation
- `Assets/Scripts/UI/ResultAnimator.cs` - Result screen reveal animations

## Files Modified
- `Assets/Scripts/UI/UIBuilder.cs` - Added ScreenFader to UIContext and Build()
- `Assets/Scripts/UI/HUDView.cs` - Added ShowScorePopup() method
- `Assets/Scripts/UI/ResultScreenView.cs` - Added ResultAnimator integration, animated Show/Hide
- `Assets/Scripts/Customer/CustomerView.cs` - Added patience-based expression tints, happy serve tint
- `Assets/Scripts/Core/GameManager.cs` - Added fade transitions to all screen changes, ShowGradeFeedback helper for score popups and grade flashes
