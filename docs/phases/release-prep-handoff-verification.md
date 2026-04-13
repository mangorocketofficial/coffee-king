# Release Prep Handoff Verification

- Date: 2026-04-12
- Contract: `docs/phases/release-prep-handoff-contract.md`
- Baseline docs: `docs/release-prep-checklist.md`, `docs/phases/p1-onboarding-polish-verification.md`

## Implemented Scope

- Added a release-prep checklist documenting:
  - Android package identity baseline
  - app bundle direction
  - remaining store-submission requirements
  - final art handoff expectations
- Updated project packaging defaults:
  - company name -> `CoffeeKing`
  - product name -> `Coffee King`
  - Android application id -> `com.coffeeking.game`
- Updated the Android build profile to enable app bundle output.
- Switched `SpriteFactory` to prefer `Resources/Sprites/Final` before graybox fallback by default.
- Updated both sprite README files to reflect the current shipping sprite basenames instead of the older placeholder naming set.
- Added a release audit entrypoint:
  - `CoffeeKing.EditorTools.ReleasePrepAudit`
  - batch wrapper `ReleasePrepBatchEntry.Run`
- The release audit now reports:
  - package identity status
  - app bundle status
  - keystore status
  - final art coverage

## Verification Evidence

- Unity batch compile succeeded against a temporary validation clone because the main working project was already open in the Unity editor and could not be reopened by batchmode.
- Compile log: `C:\Users\User\Desktop\Games\coffee-king\build\outputs\logs\release-prep-unity.log`
- Result: `return code 0`
- Release audit succeeded against the same validation clone.
- Audit log: `C:\Users\User\Desktop\Games\coffee-king\build\outputs\logs\release-prep-audit-unity.log`
- Audit report copy: `C:\Users\User\Desktop\Games\coffee-king\build\outputs\logs\release-prep-audit.txt`

## Remaining Audit Warnings

- Android custom keystore is still not configured.
- Final art coverage is still `0/43`, so runtime will continue falling back to graybox assets.

## Residual Risk

- Package identity is now cleaner, but the final publisher-owned namespace may still need to replace the baseline application id before store submission.
- No launcher icons or splash assets were added in this slice.
- This slice prepares the final art handoff path, but it does not include any actual final production sprite delivery.
