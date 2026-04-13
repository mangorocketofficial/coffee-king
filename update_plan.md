# Coffee King — V2 Reboot Plan

> This document is a reboot baseline, not a minor update.
> It replaces the previous forward plan for future implementation direction,
> while preserving the existing Phase 1 to Phase 4 work as completed legacy baseline.

---

## Reboot Declaration

### What This Means

- The current repository already contains a playable graybox build based on the original plan.
- That work is not discarded. It remains the completed implementation history for:
  - Phase 1 graybox mechanics
  - Phase 2 stage loop
  - Phase 3 UI and tutorial
  - Phase 4 asset swap layer
- This document defines the next baseline for future work.
- The reboot changes the core drink fantasy, interaction loop, and asset language enough that it should be treated as a new production direction.

### Naming Rule

- Old baseline: original `development.md` phases and completed verification docs
- New baseline: `V2 Reboot`

### Repository Rule

- Existing `docs/phases/phase1-4-*.md` documents remain valid historical records.
- New implementation work should explicitly reference this reboot plan when future phase contracts are created.

---

## Why Reboot

The original build proved the project structure, stage loop, and code-driven Unity workflow.
However, the new direction is stronger because:

- the barista fantasy is clearer
- the actions map better to real cafe process
- the drink build is more visually rewarding
- the asset pipeline now has a more distinctive visual identity

The tradeoff is scope. This is now a deeper simulation-like loop, so the implementation must be staged more carefully.

---

## Current Legacy Baseline

These systems already exist in the repository and are considered stable starting material:

- code-only scene bootstrap
- stage intro, play, result flow
- queue and patience loop
- title screen, HUD, tutorial layer
- asset swap-ready `SpriteFactory`
- extraction, steam, syrup, serving flow

These systems should be reused where possible.
Do not rewrite them all at once unless a slice proves reuse is impossible.

---

## New Product Direction

### Core Fantasy

The player is no longer making generic cafe drinks in a simplified sequence.
The player is building iced coffee drinks through a more believable counter workflow:

- prepare portafilter
- grind
- tamp
- lock
- extract
- build iced drink
- seal drink
- serve

### New Drink Set

- Iced Americano
- Iced Latte

This replaces the current primary drink set of:

- Americano
- Caffe Latte
- Vanilla Latte

### New Presentation Style

- AI-generated 2.5D flat illustration
- front-facing view
- transparent plastic iced cups with visible layers

---

## Reboot Rules

### Rule 1: Preserve Existing Work As Legacy Baseline

- Do not delete old phase verification docs.
- Do not pretend the previous implementation never existed.
- Reuse architecture where it still helps:
  - `SceneBootstrapper`
  - `StageManager`
  - `CustomerSpawner`
  - `HUDView`
  - `TutorialOverlay`
  - `SpriteFactory`
  - `GaugeView`

### Rule 2: Do Not Rebuild Everything At Once

- The reboot must be implemented as vertical slices.
- Each slice must produce a working game state before the next slice begins.

### Rule 3: Americano First

- The first reboot milestone is a complete playable Iced Americano only.
- No latte branch before Americano is stable.

### Rule 4: Complexity Comes After Stability

- Lid placement
- latte milk branch
- wrong drink penalty
- bonus chains

These should be added after the Americano slice works end-to-end.

---

## Reboot Loop

### Full Intended Sequence

#### Iced Americano

```text
CUSTOMER ORDER
  -> pick up empty portafilter
  -> move to grinder
  -> grind
  -> return to workstation
  -> tamp
  -> move to machine
  -> lock portafilter
  -> press extraction
  -> stop extraction in target zone
  -> auto spawn cup + ice
  -> pour shot into cup
  -> tap water bottle
  -> place lid
  -> serve to customer
  -> score
```

#### Iced Latte

Same loop until shot pour, then:

```text
pour shot into cup
-> tap milk pitcher
-> place lid
-> serve
```

---

## Interaction Model

### Scored Mechanics

These are the primary skill checks.

#### 1. Grinding

- Input: hold on grinder
- Gauge: fills while held
- Perfect: 70 to 78
- Good: 60 to 85
- Bad: outside
- Output swap:
  - `portafilter_empty` -> `portafilter_filled`

#### 2. Tamping

- Input: hold on tamper
- Gauge: pressure increases while held
- Perfect: 68 to 76
- Good: 55 to 85
- Bad: outside
- Extra failure zone:
  - over 90 = over-compressed
- Output swap:
  - `portafilter_filled` -> `portafilter_tamped`

#### 3. Portafilter Lock

- Input: drag to machine, snap, rotate
- Score by completion speed
- Perfect: within 2 seconds
- Good: within 4 seconds
- Bad: after 4 seconds

#### 4. Extraction Timing

- Input: tap to start, tap to stop
- Perfect: 65 to 72
- Good: 55 to 80
- Bad: outside
- Output swap:
  - `shot_glass_empty`
  - `shot_glass_filling`
  - `shot_glass_full`

### Simple Actions

These move the drink flow forward but should not add heavy cognitive load.

- pick up portafilter
- move to grinder
- press extraction button
- pour shot into cup
- pour water or milk
- place lid
- serve to customer

### Automated Actions

These exist for flow clarity and rhythm.

- customer entrance
- portafilter return from grinder
- cup spawn
- ice fill
- shot glass spawn

---

## Reboot Asset Direction

### Core Asset Language

- front-facing composition
- flat 2.5D illustration
- warm brown, cream, teal accents
- readable silhouettes on mobile

### New Required Assets

#### Machine

- `machine_empty.png`
- `machine_locked.png`

#### Portafilter

- `portafilter_empty.png`
- `portafilter_filled.png`
- `portafilter_tamped.png`

#### Grinder

- `grinder.png`
- `grinder_with_portafilter.png`

#### Tools

- `tamper.png`

#### Extraction

- `shot_glass_empty.png`
- `shot_glass_filling.png`
- `shot_glass_full.png`

#### Cups

- `cup_plastic_empty.png`
- `cup_plastic_ice.png`
- `cup_plastic_shot.png`
- `cup_plastic_americano.png`
- `cup_plastic_latte.png`
- `cup_americano_lidded.png`
- `cup_latte_lidded.png`
- `dome_lid.png`

#### Ingredients

- `water_bottle.png`
- `milk_pitcher.png`

#### Characters

- `customer_01.png`

---

## Architecture Direction

### Keep

- `GameManager` as top-level runtime owner
- `StageManager` for stage state
- `CustomerSpawner` for queue logic
- `SpriteFactory` for swap-ready loading
- `GaugeView` for all timing and pressure gauges

### Add

#### New Mechanics

- `GrindingMechanic.cs`
- `TampingMechanic.cs`
- `PourMechanic.cs`
- `IngredientMechanic.cs`
- `LidMechanic.cs`

#### New Flow Layer

- `DrinkFlowController.cs`
- `DrinkFlowState.cs`

### Ownership Rule

`GameManager` stays the global owner.
`DrinkFlowController` should manage only the per-drink sequence.
It should not replace `GameManager` as the top-level application controller.

This avoids splitting authority across two competing master state machines.

---

## Vertical Slice Plan

This is the most important change from the previous version.
Implementation must follow this exact rollout logic.

### Slice 1: Iced Americano Core

Goal: one fully playable drink from order to serve.

Build only:

1. `GrindingMechanic`
2. `TampingMechanic`
3. `DrinkFlowController` minimal version
4. Iced Americano only
5. Existing queue and stage loop integration

Required playable sequence:

```text
pick up
-> move to grinder
-> grind
-> tamp
-> lock
-> extract
-> cup + ice auto setup
-> pour shot
-> pour water
-> serve
```

Do not add:

- latte branch
- lid
- wrong drink penalty
- advanced bonuses

until this slice is stable.

### Slice 2: Iced Latte Branch

Goal: add only the milk variation.

Build:

- `IngredientMechanic` milk path
- recipe branch from water to milk
- visual distinction between Americano and Latte fill states

### Slice 3: Lid Finish

Goal: add end-cap finishing interaction.

Build:

- `LidMechanic`
- lidded final cup sprites

### Slice 4: Rule Expansion

Goal: enrich scoring and failure states only after the flow feels good.

Build:

- wrong drink detection
- perfect streak bonus
- no mistake bonus
- tuned speed bonus

---

## Revised Stage Rollout

### Stage Structure For Reboot

| Stage | Customers | Drinks | Simultaneous | Focus |
|-------|-----------|--------|--------------|-------|
| 1 | 3 | Iced Americano | 1 | Tutorial for reboot mechanics |
| 2 | 5 | Iced Americano | 1 | Speed and rhythm |
| 3 | 7 | + Iced Latte | 2 | Branching liquid choice |
| 4 | 10 | All reboot drinks | 2 | Multitasking |
| 5 | 12 | All reboot drinks | 3 | Pressure |

### Important Tuning Note

Current draft numbers from the earlier update were too aggressive for the longer reboot loop.

Specifically:

- extraction alone consumes meaningful real time
- grind + tamp + extraction + pour + serve creates a longer total drink duration
- a `25 second` speed bonus target is too tight for the intended sequence

Reboot rule:

- do not lock final timing values until Slice 1 is manually play-tested

---

## Revised Scoring Direction

### Base Scoring

Four primary judged mechanics:

- Grinding
- Tamping
- Portafilter Lock
- Extraction Timing

### Hold Back For Later

Do not implement these in the first Americano slice:

- wrong drink penalty
- no mistake bonus
- full-stage perfect bonus

These are valuable, but they increase tuning pressure and punish late-stage mistakes too heavily before the base loop is proven.

### Speed Bonus Rule

Speed bonus should be determined only after real playtesting of the Americano slice.

Initial recommendation:

- first prove comfortable completion time
- then set bonus threshold above that comfort line, not below it

---

## Sprite State Map

This remains useful and should be preserved as implementation reference.

```text
pick up portafilter      -> portafilter_empty appears
move to grinder          -> portafilter_empty snaps to grinder
grinding complete        -> portafilter_filled
return to workstation    -> grinder resets, filled portafilter returns
tamping complete         -> portafilter_tamped
lock complete            -> machine_empty -> machine_locked
press extraction         -> shot_glass_empty appears
extraction progress      -> shot_glass_filling
extraction complete      -> shot_glass_full
cup setup                -> cup_plastic_empty -> cup_plastic_ice
pour shot                -> cup_plastic_shot
pour water               -> cup_plastic_americano
pour milk                -> cup_plastic_latte
place lid                -> lidded drink state
serve                    -> finished cup exits to customer
```

---

## Implementation Order

### Phase R1

1. create reboot contract
2. add `DrinkFlowState`
3. add `DrinkFlowController`
4. implement `GrindingMechanic`
5. implement `TampingMechanic`
6. wire Iced Americano only

### Phase R2

7. implement shot-to-cup pour
8. implement water ingredient tap
9. tune Americano full loop

### Phase R3

10. add milk branch for Iced Latte
11. add cup state swaps for latte
12. tune customer patience and stage timing

### Phase R4

13. add lid placement
14. add final cup states
15. polish transitions

### Phase R5

16. add advanced scoring rules
17. add wrong drink detection
18. retune stars and penalties

---

## Success Criteria

The reboot is successful if:

1. A tester can complete an Iced Americano end-to-end without asking what to do next.
2. Grinding, tamping, locking, and extraction feel mechanically distinct.
3. The Americano and Latte builds are visually distinguishable at a glance.
4. The longer process still feels satisfying rather than exhausting.
5. Players voluntarily retry after failure.

---

## Release Readiness Plan

> The reboot slices (Slice 1–4) established the playable MVP.
> This section defines the remaining work required to meet minimum release quality.

### Current MVP Completion Status

The following reboot slices are complete:

- [x] Slice 1: Iced Americano Core
- [x] Slice 2: Iced Latte Branch
- [x] Slice 3: Lid Finish
- [ ] Slice 4: Rule Expansion (partial — base scoring done, advanced rules pending)

Additionally implemented beyond the original plan:

- Hot Americano and Hot Cafe Latte recipes
- SteamMilk mechanic
- HotWaterCup mechanic
- 5-stage progression with all 4 drink types

---

### Release Phase 1: Sound

Priority: **Critical**
Reason: A silent game cannot ship. Sound is the single largest quality gap.

#### Required Sound Effects

- Grinding (continuous loop while held)
- Tamping (short impact)
- Portafilter lock (click/snap)
- Extraction (drip/flow loop)
- Milk steaming (hiss loop)
- Water/milk pouring (liquid flow)
- Lid snap (click)
- Serving (slide/ding)
- Customer arrival (bell/chime)
- Customer timeout (warning tone)
- Score feedback: Perfect, Good, Bad (distinct tones)
- Stage clear / Stage fail (fanfare / sad tone)
- UI button tap

#### Required Music

- BGM: at least 1 looping track (cafe atmosphere)

#### Implementation

- `AudioManager` already exists as a framework
- Add `AudioClip` references and play calls at each mechanic completion event
- Add BGM loop to `GameManager` on stage start

---

### Release Phase 2: Pause and App Lifecycle

Priority: **Critical**
Reason: Mobile apps must handle interruptions gracefully.

#### Pause Screen

- Pause button on HUD (top area)
- Pause overlay with three options: Resume, Restart, Main Menu
- `Time.timeScale = 0` on pause, `1` on resume
- All customer patience timers must freeze during pause

#### App Lifecycle

- `OnApplicationPause(true)` triggers auto-pause
- `OnApplicationFocus(false)` triggers auto-pause
- Prevent state corruption when returning from background

---

### Release Phase 3: Save System

Priority: **Critical**
Reason: Players must retain progress between sessions.

#### Data to Persist

- Highest cleared stage index
- Per-stage best score
- Per-stage best star rating
- Tutorial completion flags (per mechanic)

#### Implementation

- Use `PlayerPrefs` or JSON file via `Application.persistentDataPath`
- Load on app start, save on stage complete
- No cloud save required for initial release

---

### Release Phase 4: Stage Select Screen

Priority: **High**
Reason: Players need to replay stages and see progress.

#### Requirements

- Stage select screen between Title and Stage Intro
- Show stages 1–5 as selectable nodes
- Display star rating per stage (0–3 stars, or locked)
- Locked stages shown as grayed out / padlocked
- Unlock rule: clear previous stage with at least 1 star

#### Flow Change

```text
Title → Stage Select → Stage Intro → Playing → Result → Stage Select
```

---

### Release Phase 5: Tutorial Completion

Priority: **High**
Reason: New players must understand every mechanic on first encounter.

#### Current Tutorial Coverage

- [x] Grinding (gauge hold)
- [x] Tamping (gauge hold)
- [x] Portafilter Lock (rotation gesture)
- [ ] Extraction (tap start / tap stop timing)
- [ ] Steam Milk (temperature gauge)
- [ ] Pour Shot (drag to cup)
- [ ] Ingredient (tap to pour)
- [ ] Lid (drag to cup)
- [ ] Serving (drag to customer)

#### Implementation Rule

- Each mechanic shows a tutorial hint only on the player's first encounter
- Track shown tutorials via save system flags
- Tutorial hint must not block gameplay for more than 3 seconds

---

### Release Phase 6: UI Polish and Transitions

Priority: **High**
Reason: Visual polish is the difference between prototype and product.

#### Screen Transitions

- Fade in/out between all screen changes (Title, Stage Select, Stage Intro, Playing, Result)
- Duration: 0.3–0.5 seconds

#### In-Game Feedback

- Score popup animation on mechanic completion (+points floating up)
- Screen shake or flash on Bad grade
- Particle effect on Perfect grade
- Customer expression change (happy on serve, impatient on low patience)

#### Result Screen Enhancement

- Animated star reveal (one at a time)
- Score counter roll-up animation

---

### Release Phase 7: Scoring Rule Expansion (Slice 4 Completion)

Priority: **Medium**
Reason: Adds gameplay depth but not required for basic playability.

#### Wrong Drink Detection

- Compare served drink type to customer order
- If mismatch: zero points for that customer, penalty applied
- Visual feedback: customer rejection animation

#### Perfect Streak Bonus

- Track consecutive Perfect grades across mechanics within a single drink
- All-Perfect drink: bonus multiplier or flat bonus points

#### No Mistake Bonus

- End-of-stage bonus if no Bad grades were received
- Displayed on result screen as a separate line item

---

### Release Phase 8: Settings Screen

Priority: **Medium**
Reason: Player comfort and accessibility.

#### Options

- BGM volume slider (0–100)
- SFX volume slider (0–100)
- Vibration toggle (on/off, mobile only)

#### Access

- Settings button on Title screen
- Settings button on Pause screen

---

### Release Phase 9: Store Submission Preparation

Priority: **Final step**
Reason: Required for actual distribution.

#### Android (Google Play)

- App icon (512x512 and adaptive icon)
- Splash screen with logo
- Feature graphic (1024x500)
- Screenshots (phone and tablet)
- Store listing text (title, short description, full description)
- Privacy policy URL
- Content rating questionnaire
- Target API level compliance
- SafeArea handling for notch/cutout devices

#### iOS (App Store) — if applicable

- App icon set (all required sizes)
- Launch screen storyboard
- Screenshots (all required device sizes)
- App Review information
- Privacy nutrition labels

#### Both Platforms

- Test on at least 3 different screen aspect ratios (16:9, 19.5:9, 20:9)
- Memory profiling: confirm no leaks on 10+ consecutive stages
- Frame rate target: stable 60 FPS on mid-range devices

---

### Release Priority Summary

| Phase | Area | Priority | Dependency |
|-------|------|----------|------------|
| R1 | Sound | Critical | None |
| R2 | Pause + Lifecycle | Critical | None |
| R3 | Save System | Critical | None |
| R4 | Stage Select | High | R3 (needs save data) |
| R5 | Tutorial Completion | High | R3 (needs tutorial flags) |
| R6 | UI Polish | High | None |
| R7 | Scoring Expansion | Medium | None |
| R8 | Settings Screen | Medium | R1 (needs audio to control) |
| R9 | Store Submission | Final | All above |

R1, R2, R3 have no dependencies and can be developed in parallel.
R4 and R5 depend on R3 for persistence.
R8 depends on R1 for audio volume controls.
R9 is the final gate after all other phases are stable.

---

## Final Recommendation

Treat this document as the new implementation baseline for future work,
but execute it as a migration in slices, not as a full rewrite in one jump.

That is the difference between a strong reboot and a stalled one.
