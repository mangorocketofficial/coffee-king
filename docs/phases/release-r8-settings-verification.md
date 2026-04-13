# Release R8 - Settings Screen Verification

## Contract Checklist

### 1. SettingsView.cs
- [x] New file `Assets/Scripts/UI/SettingsView.cs` created following existing view patterns (PauseOverlayView, TitleScreenView).
- [x] Own high-sortingOrder Canvas (sortingOrder 900) so it renders above everything including pause (800).
- [x] BGM volume slider (0-100) with label showing current value.
- [x] SFX volume slider (0-100) with label showing current value.
- [x] Vibration toggle (on/off) with visual state indication (color change + ON/OFF text).
- [x] "Close" button to dismiss the overlay.
- [x] `CloseRequested` event for parent wiring.
- [x] Static `Create(Transform parent)` factory method matching existing patterns.
- [x] `Show(int bgmVolume, int sfxVolume, bool vibrationEnabled)` / `Hide()` methods.
- [x] Applies slider/toggle changes immediately via callbacks (BgmVolumeChanged, SfxVolumeChanged, VibrationChanged events).

### 2. AudioManager Changes
- [x] `SetBgmVolume(float normalized01)` method adjusting `bgmSource.volume` (scaled by 0.12f base).
- [x] `SetSfxVolume(float normalized01)` method adjusting `sfxSource.volume` (scaled by 0.36f) and `loopSource.volume` (scaled by 0.24f).
- [x] Volume values maintain relative ratios between sfx/loop sources.

### 3. SaveData / SaveManager Changes
- [x] `bgmVolume` (int, default 50) added to SaveData.
- [x] `sfxVolume` (int, default 50) added to SaveData.
- [x] `vibrationEnabled` (bool, default true) added to SaveData.
- [x] Public accessors (`BgmVolume`, `SfxVolume`, `VibrationEnabled`) on SaveManager.
- [x] Setter methods (`SetBgmVolume`, `SetSfxVolume`, `SetVibrationEnabled`) with clamping and auto-save.

### 4. UIBuilder / UIContext Integration
- [x] `SettingsView` added to `UIContext` constructor and property.
- [x] `UIBuilder.Build()` creates the SettingsView.

### 5. TitleScreenView Changes
- [x] "Settings" button added below Start button.
- [x] `SettingsRequested` event raised on click.

### 6. PauseOverlayView Changes
- [x] "Settings" button added between Restart and Main Menu buttons.
- [x] `SettingsRequested` event raised on click.
- [x] Panel height expanded to accommodate 4 buttons.
- [x] Button cleanup in Dispose().

### 7. GameManager Wiring
- [x] Settings button on Title screen opens SettingsView via `HandleTitleSettingsRequested`.
- [x] Settings button on Pause overlay opens SettingsView via `HandlePauseSettingsRequested`.
- [x] Close button on SettingsView hides it via `HandleSettingsClosed`.
- [x] Slider changes call AudioManager volume methods and persist via SaveManager.
- [x] Vibration toggle persists via SaveManager; triggers `Handheld.Vibrate()` on mobile when enabled.
- [x] On startup (`InitializeIfNeeded`), `ApplySavedSettings()` loads and applies saved volumes to AudioManager.
- [x] Subscribe to all new events in `InitializeIfNeeded`.
- [x] Unsubscribe from all new events in `OnDestroy`.
- [x] `settingsOpenedFromPause` field tracks context for future use.
- [x] Settings overlay hidden in `ShowTitleScreen()` and `ShowStageSelect()` transitions.

### 8. No Regressions
- [x] Existing pause flow (Resume, Restart, Main Menu) unaffected.
- [x] Title screen flow (Start -> Stage Select) unaffected.
- [x] Existing audio playback works correctly - new volume methods scale from base volumes.
- [x] Save file backward-compatible - new fields have defaults (bgmVolume=50, sfxVolume=50, vibrationEnabled=true).
- [x] All existing event subscriptions preserved in both Initialize and OnDestroy.

## Files Modified
- `Assets/Scripts/UI/SettingsView.cs` (NEW)
- `Assets/Scripts/Util/AudioManager.cs` (added SetBgmVolume, SetSfxVolume)
- `Assets/Scripts/Core/SaveManager.cs` (added settings fields to SaveData, accessors/setters to SaveManager)
- `Assets/Scripts/UI/UIBuilder.cs` (added SettingsView to UIContext)
- `Assets/Scripts/UI/TitleScreenView.cs` (added Settings button)
- `Assets/Scripts/UI/PauseOverlayView.cs` (added Settings button)
- `Assets/Scripts/Core/GameManager.cs` (wiring, ApplySavedSettings, settings handlers)
