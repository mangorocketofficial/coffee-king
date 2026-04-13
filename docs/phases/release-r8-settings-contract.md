# Release R8 - Settings Screen Contract

## Overview
Add a Settings screen accessible from the Title screen and Pause overlay, with BGM volume, SFX volume, and vibration toggle controls. Settings persist via SaveManager.

## Completion Conditions

### 1. SettingsView.cs
- [ ] New file `Assets/Scripts/UI/SettingsView.cs` created following existing view patterns (PauseOverlayView, TitleScreenView).
- [ ] Own high-sortingOrder Canvas (sortingOrder >= 900) so it renders above everything.
- [ ] BGM volume slider (0-100) with label showing current value.
- [ ] SFX volume slider (0-100) with label showing current value.
- [ ] Vibration toggle (on/off) with visual state indication.
- [ ] "Close" button to dismiss the overlay.
- [ ] `CloseRequested` event for parent wiring.
- [ ] Static `Create(Transform parent)` factory method matching existing patterns.
- [ ] `Show()` / `Hide()` methods.
- [ ] Applies slider/toggle changes immediately via callbacks (events for volume changes and vibration toggle).

### 2. AudioManager Changes
- [ ] `SetBgmVolume(float normalized01)` method adjusting `bgmSource.volume`.
- [ ] `SetSfxVolume(float normalized01)` method adjusting `sfxSource.volume` and `loopSource.volume`.
- [ ] Volume values stored as base volumes so relative ratios are maintained.

### 3. SaveData / SaveManager Changes
- [ ] `bgmVolume` (int 0-100, default 50) added to SaveData.
- [ ] `sfxVolume` (int 0-100, default 50) added to SaveData.
- [ ] `vibrationEnabled` (bool, default true) added to SaveData.
- [ ] Public accessors and setters on SaveManager that auto-save.

### 4. UIBuilder / UIContext Integration
- [ ] `SettingsView` added to `UIContext` constructor and property.
- [ ] `UIBuilder.Build()` creates the SettingsView.

### 5. TitleScreenView Changes
- [ ] "Settings" button added to the title screen.
- [ ] `SettingsRequested` event raised on click.

### 6. PauseOverlayView Changes
- [ ] "Settings" button added to the pause overlay.
- [ ] `SettingsRequested` event raised on click.

### 7. GameManager Wiring
- [ ] Settings button on Title screen opens SettingsView.
- [ ] Settings button on Pause overlay opens SettingsView.
- [ ] Close button on SettingsView returns to previous screen.
- [ ] Slider changes call AudioManager volume methods and persist via SaveManager.
- [ ] Vibration toggle persists via SaveManager.
- [ ] On startup (InitializeIfNeeded), saved settings are loaded and applied to AudioManager.
- [ ] Subscribe/unsubscribe to all new events in Initialize/OnDestroy.

### 8. No Regressions
- [ ] Existing pause, title, stage select, gameplay flow unaffected.
- [ ] Existing audio playback works correctly with new volume system.
- [ ] Save file backward-compatible (new fields have defaults).
