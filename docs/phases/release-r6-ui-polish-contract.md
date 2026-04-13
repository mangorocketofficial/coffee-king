# Release R6 - UI Polish and Transitions Contract

## Overview
Add visual polish: screen fade transitions, score popup animations, grade feedback effects, customer expression changes, and animated result screen reveals.

## Completion Conditions

### 1. Screen Fade Transitions
- [ ] `ScreenFader` utility class created in `Assets/Scripts/UI/ScreenFader.cs`
- [ ] ScreenFader manages a full-screen black Image with CanvasGroup alpha animation via coroutines
- [ ] ScreenFader created once by UIBuilder, available via `UIContext.ScreenFader`
- [ ] Fade out (0.35s) -> switch screen -> fade in (0.35s) on all major transitions:
  - Title -> Stage Select
  - Stage Select -> Stage Intro (StartStage)
  - Stage Select -> Title (Back)
  - Result -> Stage Select (Next)
  - Result -> Stage Intro (Retry)
  - Stage complete -> Result screen
- [ ] Fade duration configurable (default 0.35s)

### 2. Score Popup Animation
- [ ] `ScorePopup` MonoBehaviour created in `Assets/Scripts/UI/ScorePopup.cs`
- [ ] On mechanic completion, a floating "+N" text appears near the feedback area
- [ ] Text animates upward (~80px) and fades out over ~0.8s
- [ ] Color matches grade color (green for Perfect/Good, red for Bad)
- [ ] Spawned from HUDView via a public method

### 3. Grade Feedback Effects
- [ ] Screen flash on Bad grade (brief red-tinted overlay, ~0.15s)
- [ ] Golden flash/glow on Perfect grade (~0.2s)
- [ ] Effects triggered from GameManager mechanic completion handlers
- [ ] Implemented via ScreenFader or a dedicated overlay in HUDView

### 4. Customer Expression Changes
- [ ] Customer tint changes based on patience level:
  - Normal (>50% patience): white/neutral
  - Impatient (<50% patience): warm orange tint
  - Critical (<20% patience): red tint
- [ ] Happy tint on serve (brief green flash before exit)
- [ ] Implemented in CustomerView.Tick and MarkServed

### 5. Result Screen Animations
- [ ] Star reveal: stars appear one at a time with 0.4s delay between each
- [ ] Score counter roll-up: score counts from 0 to final value over ~1.0s
- [ ] ResultScreenView.Show triggers animation coroutine
- [ ] Requires MonoBehaviour host for coroutines (added to result screen canvas)

### Non-Goals
- No particle systems (keep it lightweight)
- No DOTween dependency (use manual coroutine lerps)
- No changes to game mechanics or scoring logic
- No changes to scene structure or render pipeline

### Architecture Constraints
- All new UI is procedural C# (no editor scene setup)
- ScreenFader uses its own Canvas with highest sorting order (900+)
- Preserve existing class constructors and public APIs
- New features are additive; existing functionality must not break
