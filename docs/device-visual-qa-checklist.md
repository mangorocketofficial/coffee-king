# Device Visual QA Checklist

- Date: 2026-04-12
- Package: `com.coffeeking.game`

## Install And Launch

```text
adb devices -l
adb install -r build/qa/CoffeeKing-qa.apk
adb shell monkey -p com.coffeeking.game 1
```

## Capture Screens

Collect at least:

- title screen
- Stage 1 first customer
- grind step
- tamp step
- lock step
- cup + ingredient step
- result screen clear
- result screen time up or fail

## Visual Checks

### Layering

- customer body and patience bar render above props
- speech/order content does not hide key gameplay targets
- cup, lid, shot glass, and serving tray render in the intended order

### Pivots And Snaps

- lid lands centered on cup tops
- shot glass aligns cleanly with the cup during pour
- portafilter sits correctly at the workbench, grinder, and machine lock point
- hot water cup and pitcher anchors do not appear offset

### Readability

- Stage 1 tutorial panels do not cover required interaction targets
- HUD timer remains readable in normal, warning, and critical colors
- result screen title and summary read cleanly on device width
- order monitor text remains readable without clipping

### Art Path

- no tracked gameplay sprite falls back to flat placeholder rectangles
- final sprites look consistent across hot and iced drink branches

## If A Problem Appears

Record:

- screen name
- exact visible issue
- affected asset name if known
- whether the problem is layer, pivot, scale, or text readability
