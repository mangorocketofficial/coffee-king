# Device Visual QA Verification

- Date: 2026-04-12
- Contract: `docs/phases/device-visual-qa-contract.md`
- Baseline docs: `docs/phases/final-sprite-drop-verification.md`, `docs/device-visual-qa-checklist.md`

## Implemented Scope

- Checked adb target discovery in the current environment.
- Added a device visual QA checklist covering:
  - title / stage / result screens
  - layering
  - pivots and snaps
  - text readability
  - final-art-path validation
- Added batch Android QA build tooling:
  - `CoffeeKing.EditorTools.AndroidQaBuildTools`
  - batch wrapper `AndroidQaBatchEntry.Build`
- Built a fresh Android QA APK from the validation clone using the current final sprite drop.
- Copied the resulting QA APK into the workspace at:
  - `build/qa/CoffeeKing-qa.apk`

## Verification Evidence

- Android QA build log: `C:\Users\User\Desktop\Games\coffee-king\build\outputs\logs\device-qa-build-unity.log`
- Result: `return code 0`
- Fresh QA APK: `C:\Users\User\Desktop\Games\coffee-king\build\qa\CoffeeKing-qa.apk`
- ADB discovery result at verification time:
  - `List of devices attached`
  - no connected physical device
- Install attempt result:
  - `adb.exe: no devices/emulators found`

## Blocker

- Real-device visual QA could not be completed in this environment because no Android device was connected and available to install to.

## Residual Risk

- The runtime artifact is ready for visual QA, but layer / pivot / readability confirmation on actual device hardware is still outstanding.
- Once a device is connected, the remaining work is operational rather than implementation-heavy:
  - install the QA APK
  - launch the app
  - capture the required screens
  - record any visual defects against the checklist
