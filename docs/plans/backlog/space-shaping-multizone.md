# Space Shaping — Multi-Zone Solutions

**Status:** next · space shaping · updated 2026-10-28 — bake `moves = 2+` solutions so jobs 3 and 6–8 can
be authored. Job 3 is translatable (decided 2026-10-28). Depends on the physics slice (landed 2026-10-28)
and the creator’s runtime locks (landed 2026-10-27).

**Related:** [integration](space-shaping-integration.md) ·
[progression](../../design/systems/progression.md) · [jobs](../../design/jobs.md) ·
[cube](../../design/systems/cube.md) ·
[review](../../reviews/integration-plan.md)

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

## 2. Approach — bounded beam search

The runtime already accumulates multiple `SpaceScaleCommand`s and `PuzzleVerifier` already applies every
zone in `puzzle.solution`; only the creator search is single-zone. Add a forward beam search:

- **Candidates:** the existing `Centres` (sources + source pairs, max 8), 3 sizes (min/mid/max), and 6–8
  scales from `PuzzleSimSettings` (skip scales ≈ 1).
- **Beam:** width 8; `moves = 2–3`; max 4,000 evaluations. For each step, append one candidate zone to
  every beam entry, simulate the combined session, and keep the best by entropy spent, then distance of
  ship time to the window centre.
- **Order:** apply zones in the found order; `PuzzleSolution.Zones` records that order. Dedupe permutations
  by the sorted zone set, keeping the lower cost.
- **Guard:** no duplicate zones; reject candidates too close to an existing zone (start at `0.25 ×` the
  smaller size, tune). Overlap is allowed; the soft brush still blends.
- **Final:** require `Arrive` inside `[tmin, tmax]` after exactly `moves` zones. No solution → `NoSolution`;
  the seed scan retries.
- **Determinism:** fixed loops, no random, no LINQ.
- **Budget:** the entropy budget is still derived from the solution’s total cost (`×3`), so the search only
  ranks cost.

`moves = 1` keeps the existing path unchanged. `rotate` and `moves > 3` stay unsupported for this slice.

## 3. Bake and verify

- **`PuzzleBrief`** — replace `SupportsSingleZone` with `SupportsMoves` (`Moves <= 3 && !Rotate`); keep
  `SupportsSingleZone` only if the one-zone tests still use it.
- **`PuzzleSolutionSearch`** — dispatch `moves = 1` to the existing scan; `moves = 2–3` to the beam;
  `moves > 3` or `rotate` → `UnsupportedBrief`.
- **`PuzzleFieldBake`** — remove the single-zone rejection. `Variant` uses `solution.Zones[0]` for the baked
  cube frame (`zoneCentre`, `cubeSize`) and keeps all zones in `puzzle.solution`; `moves = solution.Length`;
  `entropyBudget = total × 3`.
- **`PuzzleVerifier`** — already applies every zone. Add checks that the solution’s zone count matches the
  brief, every size is inside `[cubeSizeMin, cubeSizeMax]`, and every scale is inside the envelope.
- **`PuzzleData`** — no schema change; `moves` already exists.

## 4. Runtime proof

- The POC already applies multiple zones sequentially; the player must be able to **move the cube** between
  commits (`translatable`).
- Depends on the brief’s runtime locks: `SpaceZoneView.SetConditions` → `SpaceShapingView.SetConditions` →
  `SpaceShapingState` passes `PuzzleData.translatable` / `resizable`. If that has not landed, wire it here;
  it is small.
- Committed zone frames are not drawn; the warped lattice is the record, as designed. Reset clears every
  command.
- Fixed + `moves > 1` remains rejected until §1 is resolved.

## 5. Slices

### M1 — Search core

- Add the beam search and candidate/ranking helpers; tests on a synthetic field.
- **Exit:** a translatable 2-zone solution is found deterministically; a 3-zone case is covered or capped;
  one-zone results are unchanged.

### M2 — Bake

- Remove the single-zone rejection; update `Variant`, `PuzzleVerifier`, status and tests.
- **Exit:** `Bake` returns a 2-zone `PuzzleData` for a known seed; start misses, the solution arrives in the
  window, entropy is within budget, and the zone count matches the brief.

### M3 — POC proof

- Wire the runtime conditions if needed; bake one 2-zone puzzle; play it in `SpaceShapingPOC.unity`.
- **Exit:** the POC loads the 2-zone puzzle and the player can reproduce the solution with two moved zones;
  console clean; EditMode green.

## 6. Tests

- Search: finds two zones, deterministic, respects the window, zone count, sizes and envelope; no shared
  mutable fixture.
- Bake: 2-zone verifies; fixed + multi-zone rejects; rotate rejects; one-zone still passes; deterministic.
- Verifier: applies every solution zone; over-budget fails; a missing zone does not solve.
- Runtime: the baked solution reproduces through `SpaceShapingSession`.

## 7. Out of scope

- Rotate and `moves > 3`.
- Multi-ship and loop routes.
- Final HUD/readout is separate; the lab readout landed 2026-10-28.
- Runtime generation; the catalogue is baked offline.
- Authoring the full 8–10 jobs.

## 8. Risks

- **Search cost** grows with `moves`; cap the width/evaluations and measure a seed scan.
- **Fixed-cube contradiction** (§1) — **resolved 2026-10-28:** job 3 is translatable. The search still
  rejects fixed multi-zone.
- **Rebake:** a physics change invalidates every multi-zone bake; land physics first.
- **Legibility:** two zones are only visible as lattice warp; validate in the POC before the catalogue.
