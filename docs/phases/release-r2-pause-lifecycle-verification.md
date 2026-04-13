# Release R2 - Pause and App Lifecycle Verification

## Files Modified
- `Assets/Scripts/Core/GameManager.cs` - Added Paused state, pause/resume logic, lifecycle hooks, event wiring, cleanup
- `Assets/Scripts/UI/HUDView.cs` - Added pause button, PauseRequested event, Dispose method
- `Assets/Scripts/UI/UIBuilder.cs` - Updated UIContext to include PauseOverlayView, Build creates it

## Files Created
- `Assets/Scripts/UI/PauseOverlayView.cs` - Pause overlay with Resume, Restart, Main Menu buttons

## Contract Verification

### Pause Button on HUD
- [x] Pause button visible in top-right area of HUD (positioned at anchor (1,1) with offset -110,-80 to -24,-20)
- [x] Button uses existing UIBuilder.CreateButton pattern
- [x] Button only triggers during gameplay states (PauseGame checks IsGameplayState)
- [x] Tapping pause button fires PauseRequested event -> HandlePauseRequested -> PauseGame

### Pause Overlay
- [x] Full-screen semi-transparent backdrop (Color 0.11, 0.08, 0.06, alpha 0.78)
- [x] "PAUSED" title text (64pt, bold)
- [x] Three buttons stacked vertically: Resume (green), Restart (orange), Main Menu (brown)
- [x] Resume: calls ResumeGame - hides overlay, restores timeScale=1, restores previous state
- [x] Restart: hides overlay, restores timeScale=1, calls HandleRetryRequested (existing retry logic)
- [x] Main Menu: hides overlay, restores timeScale=1, calls ShowTitleScreen (existing title logic)
- [x] Overlay has its own Canvas with sortingOrder=800 (above HUD at 500, above ResultScreen at 700)

### Time Freeze
- [x] Time.timeScale = 0 when paused (PauseGame method)
- [x] Time.timeScale = 1 when resumed (ResumeGame, HandlePauseRestartRequested, HandlePauseMainMenuRequested)
- [x] Customer patience timers freeze: Customer.TickPatience receives Time.deltaTime which is 0 when timeScale=0
- [x] Stage timer freezes: StageManager.Tick receives Time.deltaTime which is 0 when timeScale=0
- [x] Coroutines using WaitForSeconds pause: WaitForSeconds respects timeScale by default
- [x] Update() returns early when State == Paused, preventing all game logic progression

### GameState Integration
- [x] Added Paused value to GameState enum
- [x] stateBeforePause field stores previous state before pausing
- [x] Update returns early when Paused state, preventing game logic progression
- [x] ResumeGame restores stateBeforePause

### App Lifecycle
- [x] OnApplicationPause(true) triggers PauseGame if in gameplay state
- [x] OnApplicationFocus(false) triggers PauseGame if in gameplay state
- [x] Returning from background sees paused game with overlay shown
- [x] Does not auto-pause from title screen or result screen (IsGameplayState check)

### Integration Rules
- [x] No existing functionality broken (all changes are additive)
- [x] Follows existing architecture (event-driven, factory Create, private constructor)
- [x] All UI procedurally created in C# code
- [x] Clean cleanup in OnDestroy (event unsubscription, Dispose calls)
- [x] ShowTitleScreen and StartStage both hide pause overlay for safety

## Verification Status: PASS
All contract conditions met.
