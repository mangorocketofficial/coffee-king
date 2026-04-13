# Release R2 - Pause and App Lifecycle Contract

## Overview
Add a pause system with pause button on HUD, pause overlay with Resume/Restart/Main Menu options, and automatic pause on app lifecycle events (background/focus loss).

## Completion Conditions

### Pause Button on HUD
- [ ] Pause button visible in top-right area of HUD (near timer)
- [ ] Button uses existing UIBuilder.CreateButton pattern
- [ ] Button only active during gameplay states (not title screen, not result screen, not already paused)
- [ ] Tapping pause button triggers pause

### Pause Overlay
- [ ] Full-screen semi-transparent backdrop (matches existing overlay style)
- [ ] "PAUSED" title text
- [ ] Three buttons stacked vertically: Resume, Restart, Main Menu
- [ ] Resume: unpauses game, hides overlay
- [ ] Restart: restarts current stage (reuses existing HandleRetryRequested logic)
- [ ] Main Menu: returns to title screen (reuses existing ShowTitleScreen logic)
- [ ] Overlay uses same Canvas/sorting approach as ResultScreenView (sortingOrder above HUD)

### Time Freeze
- [ ] `Time.timeScale = 0` when paused
- [ ] `Time.timeScale = 1` when resumed
- [ ] Customer patience timers freeze (they use Time.deltaTime, which becomes 0)
- [ ] Stage timer freezes (StageManager.Tick receives Time.deltaTime)
- [ ] Coroutines using WaitForSeconds pause (they respect timeScale)

### GameState Integration
- [ ] Add `Paused` value to GameState enum
- [ ] Store previous state before pausing to restore on resume
- [ ] Prevent game logic progression while paused (Update early-returns)

### App Lifecycle
- [ ] `OnApplicationPause(true)` triggers auto-pause if in gameplay state
- [ ] `OnApplicationFocus(false)` triggers auto-pause if in gameplay state
- [ ] Returning from background sees paused game (no state corruption)
- [ ] Does not auto-pause from title screen or result screen

### Integration Rules
- [ ] No existing functionality broken
- [ ] Follows existing architecture patterns (event-driven, factory Create methods)
- [ ] All UI procedurally created in C# code
- [ ] Clean cleanup in OnDestroy
