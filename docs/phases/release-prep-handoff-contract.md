# Release Prep Handoff Contract

- Date: 2026-04-12
- Baseline: `docs/release-prep-checklist.md`, `docs/phases/p1-onboarding-polish-verification.md`

## Goal

- Clean up the Android release baseline so the project no longer ships with template identity values.
- Prepare the runtime to pick up final sprite assets without another code change.
- Add an editor audit so missing release-packaging and final-art items are visible in one place.

## Completion Criteria

- `ProjectSettings.asset` no longer uses template company/product values.
- Android application id has a concrete project baseline value.
- Android build profile is configured to produce an app bundle.
- `SpriteFactory` prefers final art before graybox fallbacks.
- `Assets/Resources/Sprites/Final/README.txt` and `Graybox/README.txt` document the current shipping sprite names.
- An editor audit can report:
  - package identity status
  - keystore status
  - app bundle status
  - missing final sprites
- Unity batch compile succeeds.
