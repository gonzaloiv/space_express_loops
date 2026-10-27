# Space Shaping — Tech Debt (creator + runtime)

**Status:** now · space shaping · updated 2026-10-27 — **landed**: the solution is off the player asset,
the setup holds the sim snapshot, zones carry half-extents, `MoveRange` is gone, and the tests are
pinned. No feature work.

**Related:** [creator improvements](puzzle-creator-improvements.md) ·
[cube](../../design/systems/cube.md) · [progression](../../design/systems/progression.md)

## Correctness

- **The verifier accepts a crashing start — FIXED (2026-10-27).** `PuzzleVerification.StartFails` now
  requires `Miss`, matching the bake's contract (`PuzzleFieldBake.StartMisses`).
- **The creator's zone geometry vs the game's.** *(corrected 2026-10-27)* The view's physics zone is
  `new(Centre, Size × 0.5, scale, falloff)` — **no inset**; `contentInsetFraction` only shrinks the
  *content* visual / grab region. So the creator's `PuzzleZone.ToSpaceZone()` already matches it. The
  only real mismatch was the **falloff**, fixed in the improvements plan's Slice 1 (now a fraction of
  the cube's half-size). **Nothing left to change** — do not add an inset to the search.
- **`PuzzleData.solution` ships in the player asset — FIXED (2026-10-27).** Removed from `PuzzleData`;
  the bake returns it via `out` and `PuzzleVerification.Solution` carries it for the creator preview.
  The shipped asset no longer contains the solution.

## Authoring hygiene

- **`PuzzleArtifactExporter` GUID churn — FIXED (2026-10-27).** It now loads and overwrites an existing
  asset (`LoadAssetAtPath` + `EditorUtility.CopySerialized`), so re-exports preserve the GUID and keep
  references. See `PuzzleArtifactExporter.cs:21-43`.
- **Intermediates are committed and split across folders — FIXED (2026-10-27).** The shape/planet
  intermediates were removed; `Generated/` holds only the final `PuzzleData`, and the creator
  regenerates the intermediates on export.

## Code shape

- **`SpaceShapingSetup` duplicates `PuzzleSimSettings` — FIXED (2026-10-27).** It now holds one
  `Sim` snapshot; the session reads `setup.Sim.*`.
- **`PuzzleZone.size` → half-extents footgun — FIXED (2026-10-27).** Renamed to `halfExtents`;
  `ToSpaceZone` no longer halves anything.
- **`SourceSpec.MoveRange` is vestigial — FIXED (2026-10-27).** Dropped from `SourceSpec` and the
  constraint clamp; `SourceConstraints` only clamps the scale and separates the planets.

## Tests

- **`PuzzleSearchTests` mutates a shared `puzzle` — FIXED (2026-10-27).** Each test builds its own
  puzzle.
- **The seed scan `1..32` with a hard `Assert` — FIXED (2026-10-27).** Seed 1 is pinned as the fixture in
  both test suites; no scan coupling.
- **`PuzzleFieldTests.Build_FieldFitsThePlaySpace` is trivial — FIXED (2026-10-27).** It now asserts the
  field's outer extent is between half and all of the run.

## Settled (recorded, not a task)

- **Planet-size band — keep.** The bridge's `radius * 2f` and the builder's `distance × 0.06 ± 30 %`
  stay; matching the universe's smaller look is a lab/test question, not a doc decision.
- **The fixed cube is the authored region the view presents.** For non-translatable puzzles
  (progression jobs 1–2), the creator must not invent the cube size — the puzzle carries the region the
  view shows.
