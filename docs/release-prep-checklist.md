# Release Prep Checklist

- Date: 2026-04-12
- Scope: Android release packaging prep and final art handoff prep

## Android Packaging Baseline

- Company name: `CoffeeKing`
- Product name: `Coffee King`
- Android application id baseline: `com.coffeeking.game`
- Android build profile baseline:
  - release build
  - app bundle enabled

## Still Required Before Store Submission

- replace the baseline application id if a different publisher namespace is required
- configure a real Android keystore and key alias
- add Android launcher icons and adaptive icons
- add release splash assets if desired
- build and verify a signed release bundle

## Final Art Handoff Baseline

- Runtime now prefers `Resources/Sprites/Final` automatically.
- Missing final sprites still fall back to `Resources/Sprites/Graybox`.
- Final art handoff should mirror the shipping sprite names listed in:
  - `Assets/Resources/Sprites/Final/README.txt`
  - `CoffeeKing.EditorTools.ReleasePrepAudit`

## Audit Command

Use the release audit to see the remaining gaps:

```text
ReleasePrepBatchEntry.Run
```

The direct audit implementation lives in `CoffeeKing.EditorTools.ReleasePrepAudit`.
