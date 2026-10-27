# Space Shaping — Tech Debt (creator + runtime)

**Status:** later · space shaping · updated 2026-10-27 — code/authoring hygiene carried out of the
[creator improvements](../../in-progress/puzzle-creator-improvements.md) plan. None of it is feature work;
it should land alongside the slices, not before them.

**Related:** [creator improvements](../../in-progress/puzzle-creator-improvements.md) ·
[cube](../../../design/systems/cube.md) · [progression](../../../design/systems/progression.md)

## Correctness

- **The verifier accepts a crashing start — FIXED (2026-10-27).** `PuzzleVerification.StartFails` now
  requires `Miss`, matching the bake's contract (`PuzzleFieldBake.StartMisses`).
- **The creator's zone geometry vs the game's.** *(corrected 2026-10-27)* The view's physics zone is
  `new(Centre, Size × 0.5, scale, falloff)` — **no inset**; `contentInsetFraction` only shrinks the
  *content* visual / grab region. So the creator's `PuzzleZone.ToSpaceZone()` already matches it. The
  only real mismatch was the **falloff**, fixed in the improvements plan's Slice 1 (now a fraction of
  the cube's half-size). **Nothing left to change** — do not add an inset to the search.
- **`PuzzleData.solution` ships in the player asset — remove it (settled 2026-10-27).** It is
  creator-only (the verifier and the creator preview); the runtime never reads it. Keep it in the
  creator's verification record, not in the shipped `PuzzleData`.

## Authoring hygiene

- **`PuzzleArtifactExporter` GUID churn — FIXED (2026-10-27).** It now loads and overwrites an existing
  asset (`LoadAssetAtPath` + `EditorUtility.CopySerialized`), so re-exports preserve the GUID and keep
  references. See `PuzzleArtifactExporter.cs:21-43`.
- **Intermediates are committed and split across folders.** Keep intermediates under `Generated/` and
  commit only the final `PuzzleData`; stage 1 currently lives outside `Generated/`.

## Code shape

- **`SpaceShapingSetup` duplicates `PuzzleSimSettings`.** It re-declares the sim scalars as flat fields;
  hold a `PuzzleSimSettings` snapshot instead.
- **`PuzzleZone.size` → half-extents footgun.** `ToSpaceZone` silently halves `size` while
  `SpaceZone.HalfExtents` is named for what it is. Rename the field to `halfExtents`.
- **`SourceSpec.MoveRange` is vestigial.** `SourceSpecs.From` sets it to the planet's scale and the
  constraint clamp never binds (also flagged in [cube](../../../design/systems/cube.md) §Open). Drop it
  or give it a meaning.

## Tests

- **`PuzzleSearchTests` mutates a shared `puzzle`** across tests → order-dependent; build a puzzle per
  test.
- **The seed scan `1..32` with a hard `Assert`** couples the suite to generator tuning; pin a known-good
  seed or assert search behaviour.
- **`PuzzleFieldTests.Build_FieldFitsThePlaySpace` is trivial** (`shape.radius.max < seed.distance` is
  always true); assert the extent or drop it.

## Settled (recorded, not a task)

- **Planet-size band — keep.** The bridge's `radius * 2f` and the builder's `distance × 0.06 ± 30 %`
  stay; matching the universe's smaller look is a lab/test question, not a doc decision.
- **The fixed cube is the authored region the view presents.** For non-translatable puzzles
  (progression jobs 1–2), the creator must not invent the cube size — the puzzle carries the region the
  view shows.
