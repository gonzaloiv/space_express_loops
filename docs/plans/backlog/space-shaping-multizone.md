# Space Shaping — Multi-Zone Solutions

**Status:** later · space shaping · updated 2026-10-29 — parked. The bake cannot produce a level that
*needs* two zones: every run is a straight shot at the destination, so a single zone can always tune the
arrival. The search, bake and runtime locks landed; the mechanic is unproven.

**Stalled 2026-10-29 — why.**

- `ShipLaunch` aims at the destination and `PuzzleRunChoice` requires a clear straight corridor, so the bare
  run always *reaches* the destination and only misses on time. Every level is a timing puzzle, and time is
  one-dimensional: one zone tunes it.
- Seed scan (seeds 1–12, `moves: 2`, translatable, no resize): tightness 0.25 rejects every seed as
  `FewerZones`; tightness 0.18 and 0.10 find no two-zone solution at all.
- Anchoring the window to the found solution (§3) does not help: at ±5%, ±2% and ±1% a one-zone solution
  still lands inside the band, within 3.5 ms of the two-zone arrival.
- Off-axis launch pilot (angle wired through `PuzzleData.launchAngleDegs` → `SpaceShapingSetup` →
  `ShipLaunch`, 3 seeds × 6 angles): at 8–12° the bare shot misses by 0.10–0.17 m (capture radius 0.063 m)
  but no two-zone solution exists; at 6–7° seed 2 finds two zones yet a one-zone solution also fits. Capping
  the per-zone scale at 1.25× removes the two-zone solution entirely.
- Conclusion: the angle is **necessary** — it removes the "straight shot arrives" degeneracy — but not
  **sufficient**. Two zones are only required when the level has two spatial deficits; the generator has one
  (one destination, one time window).
- The four "two-zone" assets (`Job_01_05`–`01_08`) predate the minimality rule and are single-solution
  levels: a player can solve them with one zone. They do not prove multi-zone play.

**Way back in (needs a design decision and its own plan):** add a second spatial constraint — a mid-run gate
the ship must pass through *before* arriving (two targets), or a required final approach direction. Then
re-run the angle pilot, re-bake jobs 3 and 6–8, and replace `01_05`–`01_08`. The alternative is to cut the
mechanic and keep single-zone translatable levels. The creator suite still fails on the four fake assets
(`FewerZones` ×3 and the 3-zone search), so those tests stay red until this is decided.

**Related:** [catalogue](../backlog/space-shaping-catalogue.md) ·
[in the game](../in-progress/space-shaping-in-game.md) ·
[progression](../../design/systems/progression.md) · [jobs](../../design/jobs.md) ·
[cube](../../design/systems/cube.md) ·
[review](../../reviews/multizone.md) · [integration review](../../reviews/integration-plan.md)

## 1. The fixed-cube constraint (read first)

`jobs.md` used to say job 3 is **fixed · 2+**, but a fixed cube cannot reach two distinct zones. The brief
defines `translatable` as “the cube may be moved”; `moves` is how many zones the solution scales.

**Decided 2026-10-28 — (a).** `moves ≥ 2` requires `translatable`. Job 3 becomes the movable /
several-zones step; the time tutorial becomes job 4. It keeps the cube model honest.

The model could only satisfy `moves ≥ 2` with one of these; (a) is the choice:

- **(a) `moves ≥ 2` requires `translatable`** — job 3 becomes the movable/several-zones step, and job 4
  becomes the time tutorial. **Decided.**
- **(b) `moves` counts repeated scales at the same fixed zone** — preserves job 3 but does not teach
  “where do I scale”, which is the point of job 3.
- **(c) Multiple fixed sub-zones** — a new mechanic; not this plan.

The search rejects `Moves > 1 && !Translatable` as `UnsupportedBrief`. The slices below land the search on
translatable 2–3 zone puzzles; [jobs](../../design/jobs.md) and
[progression](../../design/systems/progression.md) now carry job 3 as translatable and the time tutorial
as job 4.

**Reverted 2026-10-28 (later).** `fb80e70` shipped a two-fixed-cube path that contradicted this decision
(`PuzzleFixedSearch`, `PuzzleData.fixedZones`, authored fixed seeds). It is removed: `Moves > 1` with
`Translatable == false` is `UnsupportedBrief` again, and the fixed single-zone levels (jobs 1–2, 4) keep
their authored zone.

## 2. Approach — bounded beam search

The runtime already accumulates multiple `SpaceScaleCommand`s and `PuzzleVerifier` already applies every
zone in the solution array it is handed; the creator now has the forward beam search below.

- **Candidates:** the existing `Centres` (sources + source pairs, max 8), 3 sizes (min/mid/max), and
  scales from `PuzzleSimSettings` (skip scales ≈ 1): 8 scales for `moves = 2`, 6 for `moves = 3`.
  Every zone must contain at least one source — the same rule the clock path already enforces
  (`ContainsOnlySource`); the players' planets are the field, so a zone with no planet in it does nothing.
- **Size:** every chain shares one size — the cube's baked size. Job 3 is `resizable = no`, so it *must* be
  this way, and keeping it for every brief means a baked solution never depends on the resize gesture. The
  search tries the 3 sizes across chains, not within one. Multi-size chains (jobs 5–8) are out of scope for
  this slice: the view only grows the frame to 2× (`ZoneGrabListener.frameGrowRatio`) and that limit is not
  yet shared with the creator.
- **Disjoint zones (search only):** candidates in one chain may not overlap. Separate the boxes per axis
  by their half-extents plus a small gap (start `0.25 ×` the smaller size, tune). This is a creator-side
  guard, not a new runtime rule: the player may still overlap zones and the soft brush still blends. Two
  consequences make the search cheap: no source sits in two zones, so effects never stack, and no
  permutation changes the world, so chains compare directly. It also kills the same-centre duplicate — a
  "second zone" at the same place with another size overlaps and is rejected.
- **Beam:** width 8; `moves = 2–3`; max 5,000 evaluations. The first step keeps the best chain per centre
  **and size** (up to 24), so no region or size is dropped before its second zone; later steps keep the 8
  best. Prefixes rank by closest approach to the destination, then cost — a cheap first zone that does not
  bend the path must not crowd out the one that does. Final chains rank by cost, then window fit.
- **Order:** apply zones in the found order; `PuzzleSolution.Zones` records that order. Dedupe permutations
  by the sorted zone set; with disjoint zones every permutation is the same world at the same cost, so
  keep the first.
- **Final:** require `Arrive` inside `[tmin, tmax]` after exactly `moves` zones, **and no solution with
  fewer zones at the same cube size**. A shorter solution makes the extra zone a decoration, so the bake
  rejects the seed as `FewerZones` and the seed scan retries.
- **Determinism:** fixed loops, no random, no LINQ.
- **Offline:** the bake runs once per job, so a scan may take minutes. Keep the fixed loops, let it run,
  and only widen the beam or the attempt count if a job has no seed. No early-exit pruning.
- **Budget:** the entropy budget is still derived from the solution’s total cost (`×3`), so the search only
  ranks cost.

`moves = 1` keeps the existing path unchanged. `rotate` and `moves > 3` stay unsupported for this slice.

## 3. Bake and verify

- **`PuzzleBrief`** — replace `SupportsSingleZone` with
  `SupportsMoves` (`Moves <= 3 && !Rotate && (Moves <= 1 || (Translatable && !OffPath))`); keep
  `SupportsSingleZone` only if the one-zone tests still use it. The translatable guard is what rejects
  fixed multi-zone (§1), and off-path stays the single-zone clock tutorial.
- **`PuzzleSolutionSearch`** — dispatch `moves = 1` to the existing scan; `moves = 2–3` to the beam;
  `moves > 3` or `rotate` → `UnsupportedBrief`.
- **`PuzzleFieldBake`** — remove the single-zone rejection. `Variant` keeps today's frame rule:
  `zoneCentre` stays the run midpoint for a translatable level (a fixed level uses the zone), `cubeSize`
  comes from `found.Zones[0]`, and `moves = found.Zones.Length`; `entropyBudget = total × 3`. The baked
  asset persists only that frame — the full zone array stays the bake/verify artifact returned to the
  creator window and stops there. No `PuzzleData.solution` field, no schema change. If a chain with fewer
  zones solves at the same cube size, fail `FewerZones` (§2).
- **`PuzzleVerifier`** — already applies every zone. Add checks that the solution’s zone count matches
  `puzzle.moves`, every size is inside `[cubeSizeMin, cubeSizeMax]`, a `!Resizable` solution shares one
  size, and every scale is inside the envelope.
- **`PuzzleData`** — no schema change; `moves` already exists.

## 4. Runtime proof

- The POC already applies multiple zones sequentially; the player must be able to **move the cube** between
  commits (`translatable`).
- The runtime locks landed 2026-10-27: `SpaceZoneView.SetConditions` → `SpaceShapingView.SetConditions` →
  `SpaceShapingState` passes `PuzzleData.translatable` / `resizable`, and `ZoneGrabListener` gates the move
  and resize gestures on them. No wiring is expected here; verify it in M3.
- Committed zone frames are not drawn; the warped lattice is the record, as designed. Reset clears every
  command.
- `moves` is authored metadata, **not** a runtime cap: the game never shows or enforces a move count
  ([cube](../../design/systems/cube.md)). Do not add a runtime limit in this slice.
- Fixed + `moves > 1` is rejected up front by `SupportsMoves` (§1, §3).

## 5. Slices

### M1 — Search core

- Add a multi-zone `PuzzleZoneRun.TryArrive(puzzle, settings, PuzzleZone[] zones, out solution)` overload
  that applies every zone to one session; keep the single-zone call as a wrapper so one-zone results are
  unchanged.
- Add the beam search and candidate/ranking helpers; tests on a synthetic field.
- **Exit:** a translatable 2-zone solution is found deterministically; the same path handles a 3-zone case
  (covered by a unit test, not a baked level); one-zone results are unchanged.

### M2 — Bake

- Remove the single-zone rejection; update `Variant`, `PuzzleVerifier`, status and tests.
- **Exit:** `Bake` returns a 2-zone `PuzzleData` for a known seed; start misses, the solution arrives in the
  window, entropy is within budget, the zone count matches the brief, and a `!Resizable` chain shares one
  cube size (so the frame gesture can reproduce it).

### M3 — POC proof

- Wire the runtime conditions if needed; bake one 2-zone puzzle; play it in `SpaceShapingPOC.unity`.
- **Exit:** the POC loads the 2-zone puzzle and the player can reproduce the solution with two moved zones;
  console clean; EditMode green. The manual pass is quick and stays in this plan.
- **Status 2026-10-29:** not met — no seed bakes a level that needs two zones (see *Stalled*). The four
  assets in the POC are single-solution levels.

## 6. Tests

- Search: finds two zones, deterministic, respects the window, zone count, sizes and envelope; rejects
  overlapping and same-centre candidates; a `!Resizable` chain shares one size; a permutation of a found
  chain gives the same cost and ship time; no shared mutable fixture.
- Bake: 2-zone verifies; fixed + multi-zone rejects; rotate rejects; a seed solved by fewer zones rejects
  as `FewerZones`; one-zone still passes; deterministic.
- Verifier: applies every solution zone; over-budget fails; a missing zone does not solve.
- Runtime: the baked solution reproduces through `SpaceShapingSession`.

## 7. Out of scope

- Rotate and `moves > 3`.
- Resizable multi-size chains (jobs 5–8): the view's 2× frame-grow limit must be shared with the creator
  first, so a baked chain is actually reachable.
- Multi-ship and loop routes.
- Final HUD/readout is separate; the lab readout landed 2026-10-28.
- Runtime generation; the catalogue is baked offline.
- Authoring the full 8–10 jobs.

## 8. Risks

- **Search cost** grows with `moves`, but the bake is offline and runs once per job: a scan may take
  minutes and that is acceptable. Keep the fixed loops, cap at 5,000 evaluations, and widen only if a job
  refuses to bake.
- **Dead-end prefixes:** a cost-first beam can keep eight chains that cannot reach the window. Prefixes are
  ranked by closest approach instead, and the first step keeps one chain per centre and size, so every
  region and size gets its second zone. No pruning heuristics beyond that.
- **Fixed-cube contradiction** (§1) — **resolved 2026-10-28:** job 3 is translatable. The search still
  rejects fixed multi-zone.
- **Rebake:** a physics change invalidates every multi-zone bake; physics landed 2026-10-28, so bake after
  it, not before.
- **Legibility:** two zones are only visible as lattice warp; the POC pass (M3) proves it before the
  catalogue.
