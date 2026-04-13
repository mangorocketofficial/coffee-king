# Release R1 Sound - Contract

## Objective
Add procedural sound effects and background music to every game interaction, using Unity's AudioClip.Create API to generate synthetic audio entirely in code (no external audio files required).

## Completion Conditions

### CC-1: AudioManager extended with all required SFX clips
The `AudioManager` must create procedural AudioClip instances for every required sound:
- Grinding (continuous loop while held)
- Tamping (short impact)
- Portafilter lock (click/snap) - already exists as `snapClip`
- Extraction (drip/flow loop)
- Milk steaming (hiss loop)
- Water/milk pouring (liquid flow)
- Lid snap (click)
- Serving (slide/ding) - already exists as `serveClip`
- Customer arrival (bell/chime)
- Customer timeout (warning tone)
- Score feedback: Perfect, Good, Bad - already exist
- Stage clear - already exists as `stageCompleteClip`
- Stage fail (sad tone)
- UI button tap - already exists as `uiClip`

### CC-2: Looping audio support
AudioManager must support starting and stopping looping sounds (grinding, extraction, steaming) via a dedicated looping AudioSource separate from the one-shot SFX source.

### CC-3: BGM support
AudioManager must include a BGM AudioSource that plays a procedurally generated looping cafe atmosphere track. BGM starts on stage start and persists across gameplay.

### CC-4: Play calls wired into mechanics
Each mechanic must trigger the appropriate sound at the correct moment:
- GrindingMechanic: start grinding loop on grind begin, stop on finish/cancel
- TampingMechanic: play tamping impact on finish
- PortafilterMechanic: play lock snap on lock (already wired)
- ExtractionMechanic: start extraction loop on brew begin, stop on finish/cancel
- SteamMilkMechanic: start steam loop on wand snap, stop on finish/cancel
- PourMechanic: play pour sound on completion
- IngredientMechanic: play pour sound on completion
- HotWaterCupMechanic: play pour sound on completion
- LidMechanic: play lid snap on completion
- ServingMechanic: play serve sound on completion (already wired)

### CC-5: Play calls wired into game events
- Customer arrival: chime when a new customer begins order
- Customer timeout: warning tone on timeout
- Stage fail: sad tone when stage fails
- Stage clear: fanfare on clear (already wired, now conditional)

### CC-6: No broken functionality
All existing gameplay, scoring, and UI must continue to work exactly as before. No compile errors.

## Files Modified
- `Assets/Scripts/Util/AudioManager.cs` - Extended with new clips, loop support, BGM
- `Assets/Scripts/Core/GameManager.cs` - Wire new sound calls to mechanic handlers

## Out of Scope
- Volume control UI
- Audio settings persistence
- Real audio file integration (placeholder procedural audio only)
