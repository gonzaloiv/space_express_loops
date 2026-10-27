# Review — puzzle-creator plan implementation

**Date:** 2026-10-27.
**Scope:** the in-progress [puzzle creator improvements](../plans/in-progress/puzzle-creator-improvements.md)
plan against the code (`Assets/_Game/Features/SpaceShaping`, `Assets/_Game/Features/PuzzleCreator`).

**Verdict:** four of six slices are landed (1, 2 mostly, 3, 6); **Slice 4 (the brief) and Slice 5 (the
physics) are not started** — the plan's headline work (per-level conditions, the physical clock) is not in
the code yet.

| Slice | Status | Evidence |
|---|---|---|
| 1 — constants | **Landed** | `runDistance`, `cubeSize`, `planetRadius` in `SpaceShapingSettings.cs:10-24`; no `launchSpeed` there; `CaptureRadius = planetRadius` (`PuzzleSimSettings.cs:26`) |
| 2 — spawn cube | **Mostly** | the view instantiates the cube from a prototype (`SpaceShapingView.cs:77-78`), the frame is set per puzzle (`SpaceShapingState.cs:90-96`). **Gap:** the region's size/falloff are still global (`settings.cubeSize` / `zoneFalloff`); only `zoneCentre` is per-puzzle (`PuzzleData.cs:12`) |
| 3 — density / corridor | **Landed** | endpoints are planets with a bounded run + clearance (`PuzzleFieldBake.cs:75-125`); not axis-locked; `zoneCentre` follows the run (`PuzzleFieldBake.cs:24`); the destination is excluded from the crash test (`TrajectoryIntegrator.cs:56`) |
| 4 — brief | **Not started** | `PuzzleSeed.cs` has no conditions; the search is scale-only, fixed centre/size, fixed sample count (`PuzzleSolutionSearch.cs:31-58`); no runtime locks (`ZoneGrabListener.cs:92-99` always allows move/resize/scale); `PuzzleData` carries no conditions |
| 5 — physics | **Not started** | the clock is still the tuned clamp `Clamp(1 + scale·Potential, 0.05, 1)` (`TrajectoryIntegrator.cs:34-35`); mass ∝ radius (`sourceStrengthScale` = "strength = radius × this"), not `radius³` |
| 6 — entropy | **Landed** | the budget is derived from the solution: `entropyBudget = EntropySpent × 3` (`PuzzleFieldBake.cs:142`) |

## Partial

- **The window is derived, but not as `Tcross`** — it is `solution.ShipTime ± tightness/2`
  (`PuzzleFieldBake.cs:140-141`), not `Tcross × [lo, hi]`.
- **"One best variant" is not enforced** — `PuzzleSeedScanner.Find` returns up to `variants`
  (`PuzzleSeedScanner.cs:16-24`); the count is caller-supplied, not ranked to one.
- **The search grid is not adaptive** — a fixed sample count, sorted by `|scale − 1|`
  (`PuzzleSolutionSearch.cs:48-56`).

## Tech debt (untouched)

- `PuzzleVerification.StartFails` is still `!= Arrive` (`PuzzleVerification.cs:16`) — a `Crash` passes as
  "start fails".
- `PuzzleArtifactExporter` GUID churn, `SpaceShapingSetup` duplicating `PuzzleSimSettings`,
  `PuzzleData.solution` still shipped — see [tech debt](../plans/backlog/gameplay/space-shaping-tech-debt.md).

## Recommendation

**Slice 5 first.** Slices 1–4 and every bake run against that sim, so landing the two-term clock and
`radius³` before Slice 4 avoids re-baking every puzzle twice — and it is the validation-lab work. Slice 4
is the bigger lift: seed conditions → search over centre × size → bake reject → runtime locks.
