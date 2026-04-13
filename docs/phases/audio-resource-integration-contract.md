# Audio Resource Integration Contract

- Date: 2026-04-12
- Baseline: `docs/phases/release-r1-sound-verification.md`, current runtime audio flow in `Assets/Scripts`

## Goal

- Replace the current code-only procedural sound output with real generated WAV assets stored in the project.
- Keep all existing game event hookups intact.
- Preserve procedural synthesis only as a fallback path if an audio asset is missing.

## Completion Criteria

- `Assets/Resources/Audio` contains the runtime sound set used by `AudioManager`.
- `AudioManager` loads audio clips from `Resources/Audio` first.
- If a clip is missing from `Resources`, `AudioManager` falls back to the existing procedural generator for that clip.
- Existing runtime audio hooks in `GameManager` continue to work without control-flow changes.
- Unity batch compile succeeds after the asset drop and loader change.
