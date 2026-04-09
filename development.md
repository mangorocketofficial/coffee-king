# Coffee Meister — Development Plan

> Build the entire game programmatically. No scene editing in Unity Editor.
> Unity Editor is used **only** for play-testing and building.

---

## Architecture Overview

```
CoffeeMeister/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs          # Master controller, state machine
│   │   │   ├── SceneBootstrapper.cs     # Entry point — spawns everything
│   │   │   └── GameConfig.cs            # All tuning constants (ScriptableObject)
│   │   │
│   │   ├── Stage/
│   │   │   ├── StageData.cs             # Per-stage config (customer count, speed, menus)
│   │   │   ├── StageManager.cs          # Stage flow: intro → play → result
│   │   │   └── StageLibrary.cs          # Defines all 5 stages in code
│   │   │
│   │   ├── Customer/
│   │   │   ├── Customer.cs              # Customer entity (order, patience, state)
│   │   │   ├── CustomerSpawner.cs       # Queue logic, simultaneous wait cap
│   │   │   └── CustomerView.cs          # Visual: sprite + speech bubble + patience bar
│   │   │
│   │   ├── Orders/
│   │   │   ├── DrinkRecipe.cs           # Definition: drink name → required operations
│   │   │   ├── DrinkLibrary.cs          # All 3 recipes defined in code
│   │   │   └── OrderDisplay.cs          # Bottom-left order list UI
│   │   │
│   │   ├── Mechanics/
│   │   │   ├── PortafilterMechanic.cs   # Drag + snap + rotate gesture
│   │   │   ├── ExtractionMechanic.cs    # Tap-start, gauge fill, tap-stop timing
│   │   │   ├── SteamMilkMechanic.cs     # Insert + slide up/down + temp gauge
│   │   │   ├── SyrupMechanic.cs         # Simple tap to add (vanilla latte only)
│   │   │   └── ServingMechanic.cs       # Drag finished drink to serving area
│   │   │
│   │   ├── Scoring/
│   │   │   ├── ScoreManager.cs          # Per-drink scoring, stage totals
│   │   │   ├── ScoreRules.cs            # Perfect/Good/Bad thresholds
│   │   │   └── StarRating.cs            # Stage-end star calculation
│   │   │
│   │   ├── Input/
│   │   │   ├── GestureDetector.cs       # Unified touch input: drag, rotate, tap, hold
│   │   │   └── DragHandler.cs           # Reusable drag-and-snap component
│   │   │
│   │   ├── View/
│   │   │   ├── GameSceneBuilder.cs      # Lays out all visual elements programmatically
│   │   │   ├── BackgroundBuilder.cs     # Wall + counter + wood edge
│   │   │   ├── MachineView.cs           # Espresso machine sprite + slot positions
│   │   │   ├── IngredientTrayView.cs    # Left-side ingredient containers
│   │   │   ├── ServingAreaView.cs       # Right-side serving zone
│   │   │   └── GaugeView.cs            # Reusable gauge bar (extraction, temperature)
│   │   │
│   │   ├── UI/
│   │   │   ├── UIBuilder.cs             # Creates all Canvas UI in code
│   │   │   ├── HUDView.cs              # Score, timer, stage progress
│   │   │   ├── ResultScreenView.cs      # Stage clear screen with stars
│   │   │   ├── TitleScreenView.cs       # Simple title + start button
│   │   │   └── TutorialOverlay.cs       # Stage 1 gesture hints
│   │   │
│   │   └── Util/
│   │       ├── SpriteFactory.cs         # Loads sprites from Resources, generates placeholders
│   │       ├── ColorPalette.cs          # All colors defined as constants
│   │       └── AudioManager.cs          # Minimal SFX (click, success, fail)
│   │
│   ├── Resources/
│   │   ├── Sprites/                     # All sprite assets (PNG)
│   │   │   ├── Graybox/                # Colored rectangles for prototyping
│   │   │   └── Final/                  # Pre-rendered 3D / asset store sprites
│   │   ├── Audio/                       # SFX files
│   │   └── Config/                      # ScriptableObject instances
│   │
│   └── Scenes/
│       └── Main.unity                   # ONE scene. Empty except for bootstrapper
```

---

## Scene Setup (One-Time Manual Step)

The only thing done in Unity Editor:

1. Create `Main.unity` scene
2. Add empty GameObject named `[Bootstrapper]`
3. Attach `SceneBootstrapper.cs`
4. Set camera to orthographic, landscape (16:9)
5. That's it. Never touch the scene again.

```csharp
// SceneBootstrapper.cs — the only MonoBehaviour on the scene
public class SceneBootstrapper : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        var go = new GameObject("[GameManager]");
        DontDestroyOnLoad(go);
        go.AddComponent<GameManager>();
    }
}
```

---

## Phase 1: Graybox Mechanics (Day 1–3)

**Goal**: All 3 mechanics playable with colored rectangles. No art.

### Day 1 — Project Setup + Portafilter

**Tasks:**

- Unity project creation via CLI (`unity -createProject`)
- Implement `SceneBootstrapper`, `GameManager` state machine
- Implement `SpriteFactory` that generates colored rect sprites at runtime
- Build `GameSceneBuilder`: lay out background, machine, ingredient tray as colored rects
- Implement `GestureDetector`: distinguish drag vs. tap vs. rotate
- Implement `PortafilterMechanic`:
  - Gray rect representing portafilter
  - Drag to machine slot (snap point)
  - Rotate gesture to lock (detect angular change > threshold)
  - Visual feedback: color change on snap, on lock

**Portafilter mechanic detail:**

```
State machine:
IDLE → (pick up) → DRAGGING → (near slot) → SNAPPING → (rotate) → LOCKED

Drag: follow touch position, offset from grab point
Snap: when within 50px of slot center, lerp to exact position
Rotate: track two-finger angle OR single-finger circular motion around snap point
Lock: when cumulative rotation > 90°, lock in place, trigger success
```

### Day 2 — Extraction + Steam Milk

**Tasks:**

- Implement `ExtractionMechanic`:
  - Tap button to start extraction
  - Gauge fills over time (0→100 in ~35 seconds)
  - Golden zone: 60–75 on gauge (~21–26 seconds)
  - Tap again to stop
  - Score based on stop position: Perfect (65–72), Good (55–80), Bad (outside)

- Implement `SteamMilkMechanic`:
  - Drag steam wand into milk pitcher (snap)
  - Slide up/down to control heat rate
  - Temperature gauge rises; speed depends on wand depth
  - Target zone: 60–70°C
  - Tap to finish; score based on final temperature
  - Over 75°C = milk burned = Bad

- Implement `GaugeView`: reusable horizontal bar with target zone highlight

**Extraction timing detail:**

```
gauge_speed = 100.0 / 35.0  (units per second)
perfect_zone = [65, 72]
good_zone = [55, 80]

On tap-stop:
  if value in perfect_zone → Perfect (100pts)
  elif value in good_zone → Good (60pts)
  else → Bad (20pts)
```

**Steam milk detail:**

```
wand_depth = normalized 0..1 based on Y position
heat_rate = lerp(0.5, 3.0, wand_depth)  °C per second
foam_rate = lerp(2.0, 0.5, wand_depth)  (shallow = more foam)

perfect_temp = [60, 70]
good_temp = [55, 75]
burned = > 75  →  forced stop, Bad score
```

### Day 3 — Drink Assembly + Serving Flow

**Tasks:**

- Implement `DrinkRecipe` and `DrinkLibrary`:
  - Americano: portafilter → extraction
  - Caffe Latte: portafilter → extraction → steam milk
  - Vanilla Latte: portafilter → extraction → syrup tap → steam milk

- Implement recipe state machine:
  ```
  WAITING_FOR_PORTAFILTER → PORTAFILTER_LOCKED →
  EXTRACTING → EXTRACTION_DONE →
  (if latte) STEAMING → STEAM_DONE →
  (if vanilla) SYRUP_ADDED →
  READY_TO_SERVE
  ```

- Implement `ServingMechanic`: drag completed drink to serving area
- Implement `Customer` basic: appears, shows order, waits
- Wire up: customer order → recipe start → mechanics sequence → serve → score

---

## Phase 2: Game Loop (Day 3–4)

**Goal**: Complete stage loop with scoring and progression.

### Day 3 (continued) — Customer Queue

**Tasks:**

- Implement `CustomerSpawner`:
  - Spawns customers based on `StageData`
  - Controls simultaneous wait cap (1→2→3)
  - Random order selection from stage's allowed drink list
- Implement `CustomerView`:
  - Sprite slides in from top
  - Speech bubble with drink name
  - Patience bar decreasing over time
  - Exits after served or timeout

### Day 4 — Stage System + Scoring

**Tasks:**

- Implement `StageLibrary` with 5 stages:

  ```
  Stage 1: 3 customers, americano only, 1 simultaneous, slow patience
  Stage 2: 5 customers, +latte, 1 simultaneous, normal patience
  Stage 3: 7 customers, all drinks, 2 simultaneous, normal patience
  Stage 4: 10 customers, all drinks, 2 simultaneous, fast patience
  Stage 5: 12 customers, all drinks, 3 simultaneous, fast patience
  ```

- Implement `StageManager` flow:
  ```
  STAGE_INTRO (show stage number, 2s) →
  PLAYING (customers spawn, timer runs) →
  STAGE_COMPLETE (all served or timed out) →
  RESULT_SCREEN (show score + stars)
  ```

- Implement `ScoreManager`:
  - Accumulates per-drink scores
  - Bonus for speed (under X seconds per drink)
  - Penalty for timeout (customer leaves)

- Implement `StarRating`:
  - Total score / max possible score = percentage
  - ≥90% = ★★★, ≥70% = ★★, ≥50% = ★, <50% = fail

- Implement `ResultScreenView`:
  - Stars display
  - Score breakdown
  - "Next Stage" / "Retry" buttons

---

## Phase 3: UI + Tutorial (Day 4–5)

### Day 4 (continued) — HUD

**Tasks:**

- Implement `UIBuilder`: creates Canvas + all UI elements in code
- Implement `HUDView`:
  - Score counter (top-left)
  - Stage progress bar (top-center)
  - Timer countdown (top-right)
  - Order list (bottom-left)
- Implement `TitleScreenView`:
  - Game title
  - "Start" button → Stage 1

### Day 5 — Tutorial + Polish

**Tasks:**

- Implement `TutorialOverlay` for Stage 1:
  - Arrow pointing to portafilter: "Drag to machine"
  - Rotation hint: "Rotate to lock"
  - Gauge hint: "Tap in the green zone"
  - Only shows on first play, each hint once
- Add `AudioManager`:
  - Snap sound (portafilter lock)
  - Gauge stop sound (perfect/good/bad variants)
  - Serve sound
  - Stage complete jingle
- Overall flow test: Title → Stage 1 (tutorial) → Stage 2–5 → Game complete

---

## Phase 4: Asset Swap (Day 5–6)

**Goal**: Replace graybox with real visuals. Zero code changes.

### Asset Strategy

```
Resources/Sprites/
├── Graybox/              # Auto-generated colored rects (Phase 1–3)
│   ├── machine.png
│   ├── portafilter.png
│   ├── cup.png
│   └── ...
└── Final/                # Drop-in replacements (Phase 4)
    ├── machine.png       # Same filenames, same dimensions
    ├── portafilter.png
    ├── cup.png
    └── ...
```

**SpriteFactory** has a toggle:

```csharp
public static class SpriteFactory
{
    public static bool UseGraybox = false; // flip to true for prototyping

    public static Sprite Load(string name)
    {
        string path = UseGraybox ? $"Sprites/Graybox/{name}" : $"Sprites/Final/{name}";
        var sprite = Resources.Load<Sprite>(path);
        if (sprite == null && UseGraybox)
            return GenerateColoredRect(name); // runtime fallback
        return sprite;
    }
}
```

### Asset List (MVP)

| Asset | Description | Size (px) | Notes |
|-------|-------------|-----------|-------|
| `bg_wall` | Background wall | 1920×540 | Upper half |
| `bg_counter` | Wooden counter surface | 1920×540 | Lower half |
| `machine` | Espresso machine front view | 400×400 | Center piece |
| `machine_slot` | Portafilter slot highlight | 120×60 | Snap target |
| `portafilter` | Portafilter top-down | 200×100 | Draggable |
| `cup_empty` | Empty coffee cup | 100×120 | Per-order |
| `cup_americano` | Filled americano | 100×120 | After extraction |
| `cup_latte` | Filled latte | 100×120 | After steaming |
| `cup_vanilla` | Filled vanilla latte | 100×120 | After syrup |
| `steam_wand` | Steam wand | 60×200 | Draggable |
| `pitcher` | Milk pitcher | 100×140 | Steam target |
| `tray_beans` | Coffee beans container | 160×160 | Ingredient |
| `tray_milk` | Milk carton | 160×160 | Ingredient |
| `tray_syrup` | Vanilla syrup bottle | 160×160 | Ingredient |
| `customer_01` | Customer upper body | 200×240 | Front facing |
| `speech_bubble` | Order speech bubble | 300×80 | With text area |

**Source options for MVP:**
- Unity Asset Store: search "isometric kitchen", "cafe 2D", "food game sprites"
- AI generation: Midjourney/DALL-E prompt: `isometric [item], 3D rendered, soft lighting, white background, game asset, clean edges`
- Blender pre-render: model once, render all assets with same camera/lighting setup

---

## Phase 5: Final Polish + Build (Day 6–7)

### Day 6

- Visual polish: particle effects (steam, extraction drip) using code-spawned ParticleSystem
- Screen shake on Bad score
- Smooth animations: customer slide-in/out, score pop-up fly
- Juice: cup fill animation, gauge glow on perfect zone

### Day 7

- Full playthrough testing (all 5 stages)
- Difficulty tuning: adjust patience timers, gauge speeds, spawn rates
- Mobile build test (touch input validation)
- Platform builds:
  ```bash
  # iOS
  unity -batchmode -buildTarget iOS -executeMethod Builder.BuildIOS -quit
  # Android
  unity -batchmode -buildTarget Android -executeMethod Builder.BuildAndroid -quit
  ```
- Store submission

---

## CLI Workflow Reference

```bash
# Create project (one-time)
unity -createProject ./CoffeeMeister -quit

# Run tests
unity -batchmode -runTests -projectPath ./CoffeeMeister -quit

# Build Android APK
unity -batchmode -projectPath ./CoffeeMeister \
  -executeMethod Builder.BuildAndroid \
  -logFile build.log -quit

# Build iOS Xcode project
unity -batchmode -projectPath ./CoffeeMeister \
  -executeMethod Builder.BuildIOS \
  -logFile build.log -quit
```

---

## Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Scene count | 1 (Main.unity) | Everything spawned by code, no scene switching |
| Rendering | Unity 2D + Sprite | Simplest for pre-rendered 3D assets |
| UI system | Unity Canvas via code | Full programmatic control |
| Input | Custom GestureDetector | Need drag + rotate + tap + hold in one system |
| Asset loading | Resources.Load | Simple, no Addressables overhead for MVP |
| Graybox toggle | SpriteFactory.UseGraybox | One flag swaps entire visual layer |
| Audio | AudioManager singleton | Minimal SFX, no music for MVP |
| Data | Hardcoded in *Library classes | No JSON/XML parsing needed for 5 stages |
| State management | Enum state machines | Simple, debuggable, no framework needed |

---

## Reusable Modules (for Series Expansion)

These modules transfer directly to next game (nail shop, bakery, etc.):

| Module | Reuse Level | Notes |
|--------|-------------|-------|
| `SceneBootstrapper` | 100% | Identical |
| `GameManager` | 100% | State machine is universal |
| `GestureDetector` | 100% | Touch input is game-agnostic |
| `DragHandler` | 100% | Snap logic reusable |
| `GaugeView` | 100% | Any timing/precision mechanic |
| `CustomerSpawner` | 95% | Swap customer types |
| `StageManager` | 95% | Swap stage configs |
| `ScoreManager` | 90% | Swap score rules |
| `UIBuilder` | 90% | Swap colors/layout |
| `SpriteFactory` | 100% | Change sprite paths only |
| Mechanics | 0–50% | New mechanics per game, but pattern is same |

**Estimated reuse for Game #2: ~70% of codebase.**
**By Game #10: new game = new mechanics + new assets only. ~2 weeks per title.**

---

## Success Criteria

The MVP is successful if:

1. A tester voluntarily retries a failed stage without being asked
2. A tester clears Stage 3+ and wants to continue
3. The portafilter lock gesture feels satisfying within 3 attempts
4. Total play session exceeds 10 minutes organically

If these are NOT met, the mechanics need re-tuning before any asset work.