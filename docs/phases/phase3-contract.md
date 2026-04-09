# Phase 3 Completion Contract

- Date: 2026-04-09
- Scope: Phase 3 UI and tutorial from `development.md`
- Baseline: `development.md` and `docs/phases/phase2-verification.md`

## Binding Rules

- Follow the repository phase sequence from `agent.md`: contract, implementation, verification, verification document.
- Keep UI and tutorial creation fully programmatic. Do not rely on authoring scene UI in the Unity Editor.
- Preserve the existing Phase 2 stage loop while layering title, HUD, and tutorial behavior on top.

## Completion Criteria

### HUD And Title

- `UIBuilder` creates a Canvas and runtime UI hierarchy in code.
- `HUDView` displays:
  - score counter at the top-left
  - stage progress bar at the top-center
  - timer countdown at the top-right
  - order list / queue summary at the bottom-left
- `TitleScreenView` shows the game title and a start button before Stage 1 begins.

### Tutorial

- `TutorialOverlay` supports Stage 1 first-play guidance.
- At minimum, Stage 1 first play shows these hints once each:
  - drag to machine
  - rotate to lock
  - tap in the green zone
- Tutorial hints do not repeat after they have been shown once during the current run.

### Audio

- `AudioManager` exists as a runtime singleton and exposes at least:
  - snap sound
  - gauge stop sound variants
  - serve sound
  - stage complete jingle
- Phase 3 integration calls these hooks from the main gameplay flow.

### Overall Flow

- Startup flow is `Title -> Stage 1`.
- Stage UI remains visible during gameplay.
- Existing result screen flow still works with the new title/HUD/tutorial layer.

## Validation Target

- Unity batch compile succeeds for the updated project.
- The project enters title flow and stage flow without compile errors.
