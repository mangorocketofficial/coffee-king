# Phase 1 Completion Contract

- Date: 2026-04-09
- Scope: Phase 1 graybox mechanics baseline from `development.md`
- Objective: make the first playable graybox interaction loop available entirely from code without relying on Unity scene editing

## Binding Rules

- Follow the phase order defined in `agent.md`: contract first, implementation second, verification third, verification document last.
- Treat `development.md` as the current source of truth because no prior phase verification document exists yet.
- Keep the scene bootstrapped from code so the current `SampleScene.unity` can run without manual setup.

## Completion Criteria

### Day 1 Foundation

- `SceneBootstrapper` guarantees runtime bootstrapping, landscape orientation, 60 FPS target, and a usable orthographic camera.
- `GameManager` owns a simple game state machine and assembles the graybox play scene at runtime.
- `SpriteFactory` generates cached colored rectangle sprites at runtime for graybox visuals.
- `GameSceneBuilder` creates the wall, counter, machine body, machine slot, ingredient tray, and on-screen instruction/status text in code.
- `GestureDetector` distinguishes press/drag/tap and emits rotation deltas for multitouch rotation.
- `PortafilterMechanic` supports:
  - dragging the portafilter from its spawn point
  - snapping when close to the machine slot
  - rotating around the snap point until the lock threshold is reached
  - color feedback for idle, snapped, and locked states

### Explicitly Out Of Scope For This Start

- Extraction mechanic
- Steam milk mechanic
- Recipe flow
- Customer spawning
- Stage progression

## Validation Target

- The new scripts must compile in the local Unity project.
- Entering play mode in the current project must produce a playable graybox scene where the portafilter can be dragged, snapped, and locked with mouse or touch.
