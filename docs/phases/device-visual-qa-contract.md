# Device Visual QA Contract

- Date: 2026-04-12
- Baseline: `docs/phases/final-sprite-drop-verification.md`, `docs/release-prep-checklist.md`

## Goal

- Attempt final-path visual QA on an Android runtime target.
- If a physical device is not available in this environment, leave the project in a state where the remaining QA can be executed immediately after a device is connected.

## Completion Criteria

- Android device discovery is checked and the adb state is recorded.
- A fresh Android QA APK is produced from the current project content.
- A device visual QA checklist exists for:
  - layer ordering
  - sprite pivots
  - text readability
  - title / stage / result screens
- If no physical device is available, the validation document records the blocker explicitly instead of claiming the QA was completed.
