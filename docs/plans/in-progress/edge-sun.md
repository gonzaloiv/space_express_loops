# Edge sun

**Status:** now · space shaping · updated 2026-11-26 — chapter 2's suns sit at the edge of the system,
level with the planets; a check keeps a wrong placement from baking, 02_09 introduces it, and the map
centres on the field, not the sun. U1–U3 built (dotnet build only); Unity tests, the commit and
the merge into both bake checkouts are left.

**Related:** [plans board](../README.md) · [fleet rebake](fleet-rebake.md) U4 (the rebakes) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release) ·
[field elements](../../ideas/element-pack.md) (edge suns as a recurring variant)

**Progress (2026-11-25):** U1's validator and placement choice, U2's 02_09 line and docs, and U3's
map code are in. A centred sun keeps the sun anchor, so chapter 1 and 2-4 are exactly unchanged; the
job tag now rides over the field centre too. The fleet validation fixtures use 02_05 as the edge
fixture until 02_01 rebakes; the rebakes remain [chapter 2 bakes](chapter-2-bakes.md).

**Left (review 2026-11-26):**

- Run `PuzzleSunPlacementTests`, `PuzzleSunPlacementValidationTests`, the fleet validation and
  fixed-frame tests and the `PuzzleSelection` suite (only `dotnet build` passed); then commit U1–U3 and
  merge into `bake` and `bake2` before any chapter 2 scan.
- `ShippedJobs_MeetTheContract` is red for the six enabled chapter 2 jobs: their assets carry the old
  above-field sun, which the new check now catches. Chapter 2 bakes clears it job by job.

## Why

A centred sun cannot meet `NoSunAnswer` in a fleet: every run a cube rescues, a cube on the sun rescues
too (02_01: 48 seeds, 02_05: 24, no bake; with `SunRule = Any` 02_01 bakes at the centre, so the sun is
the answer). Dropping `NoSunAnswer` brings back the old sun-shrink wins; an inert sun is a new mechanic
and overlaps chapter 3's anchors. **Decided 2026-11-26:** the edge sun, the U4 fallback, for every
chapter 2 job but 02_04 (centred, unchanged).

## Units

### U1 — the placement, checked

- **Code and tests only; the contracts flip per job in [chapter 2 bakes](chapter-2-bakes.md).**
  `ShippedJobs_MeetTheContract` validates every enabled contract, and today's 02_01–02_03, 02_09 and
  02_10 have their suns above or below the planets (y = 0.480 / -0.417 / 0.449 / 0.449 / 0.480 against a
  band of about ±0.2). Flipping their contracts here would turn the shipped check red until every rebake
  lands. So each job's `SunPlacement = OffCorridor` lands in the same commit as its rebake.
- `PuzzleSunPlacementChoice.TryHome` keeps the sun level with the planets (the height band on `bake`:
  three heights across the planets' `y` spread, no poles) and outside the rim. Check that at the top
  and bottom band heights the sphere's `horizontal = sqrt(radius² − y²)` still clears the planets'
  horizontal reach; if not, place on a cylinder (fixed horizontal radius) instead of the sphere.
- **The validator checks where the sun is.** Today nothing does: 02_06 was committed with an
  above-the-field sun under a `Centre` contract (a4bad465). In `CheckSources`:
  - `Centre`: the sun's `home == Vector3.zero` within a small tolerance (the field origin, not
    `puzzle.origin`, which is the run's start);
  - `OffCorridor`: its `y` within the planets' `y` spread plus a margin, **and** its horizontal
    distance from the field's axis beyond the planets' largest horizontal radius. The `y` check alone
    passes a centred sun (y = 0 sits inside most bands).
  One check, wherever the validator runs (scan, `JobSet_MeetsTheContract`, `ShippedJobs_MeetTheContract`).
- **Test fixtures follow the asset.** `PuzzleFleetValidationTests` and `FixedFleetFrameTests` used
  contracts with no `SunPlacement` (the default, `Centre`) on 02_01. The accepting tests now use 02_05,
  the first edge-sun fleet, and derive the placement from the asset's sun home; 02_01 stays the
  negative fixture. 02_01's rebake keeps its own tests' positive cases green.
- Keep `PuzzleSunPlacementChoice` and the `OffCorridor` path; drop fleet rebake U4's "delete once
  centred" step.
- Exit: Creator tests (seconds): a flat ring's sun sits in the band and outside the rim; a sun above
  the band fails `Validate`; a **centred sun under `OffCorridor` fails**; a sun off the origin under
  `Centre` fails; the fleet validation and fixed-frame tests stay green. Committed and merged into `bake`
  and `bake2` before any further chapter 2 scan.

### U2 — 02_09 introduces it

- Chapter 2 opens in a new sector, and its first job says the star sits at the edge out here. 02_09's
  manual entry keeps its fleet rule and swaps the fine print for the edge: *"Two ships, one origin;
  bend near the start. Out here the star sits at the edge."* (79 characters; "Meals may swap" goes).
  Optionally Jim's `level_02_09_init_01` names it once (its clip is not generated yet, so the edit is
  free; the key is already in the downloader list).
- Chapter 1 is untouched: no rebake there (the alternative, one late chapter 1 job rebaked as an edge
  variant, costs a bake and a re-check for no extra read).
- Update [chapters](../../design/chapters.md)' "Every system keeps a star" note: centred in chapter 1
  and 2-4, at the edge in the rest of chapter 2. In [jobs](../../design/jobs.md)' chapter 2 cards,
  label the intro rows with their ids (`intro 1 (02_09)`, `intro 2 (02_10)`) and update 02_09's entry.
- Exit: the 02_09 card shows the line; `Levels_en`, the note and the `intro 1 (02_09)` row match.

### U3 — the map centres on the field

- **Decided (2026-11-26): the map cube centres over the field's centre, not the sun.** Today the map
  anchors on the sun: `RunSlotView.SunMapPosition` (the diorama's sun in map space) is the point the
  cube focuses on (`PuzzleMapView.SunFocus`), the map slides to on entering (`MapPose`), the route line
  joins (`MapRouteLine`), and `PlaceSunAt` lifts to the node's height. The slot's own origin is already
  the field's centre (`PlaceSunAt` sets `localPosition = node` first).
- Switch that one point to the field's centre: `FieldMapPosition` uses the slot's centre for an edge
  sun and keeps the sun for a centred sun (`PlaceSunAt` → `PlaceAt`, `SunFocus` → `FieldFocus`), so
  chapter 1 and 2-4 are exactly unchanged and an edge-sun system sits around its node with the star
  beside it.
- **The entry zoom pivots on the sun too.** `RunSlotView.SetZoom` grows the diorama around
  `SunLocalAtRest()` so the sun holds its node while `MapEnter` zooms in (to about 3.6×). Once the map
  slides to the field centre, an edge-sun diorama would drift by nearly its sun offset. Pivot on the
  field centre instead (`visualRoot`'s origin after `FieldContentView.Fit`, or zero) for an edge sun;
  a centred sun keeps the old pivot. The job tag follows the same centre.
- Exit: the fast `PuzzleSelection` tests pass; in the editor, chapter 2's map with 02_05 (baked at the
  edge) shows the cube over the system's centre, the route line between system centres, and entering
  02_05 (an edge sun) zooms in without a drift or a jump; chapter 1's map is unchanged.

## Then

The rebakes are [chapter 2 bakes](chapter-2-bakes.md), after U1 lands in both bake checkouts.

## Manual check (headset)

1. Every chapter 2 sun sits beside its system, level with the planets; none floats above or below.
2. 02_09's card says the star sits at the edge before the first launch.
3. A cube on the sun does not solve any chapter 2 job.
4. On the map the cube sits over each system's centre, not its star; entering a job does not jump.
5. 2-4 keeps its centred sun and plays as before.
