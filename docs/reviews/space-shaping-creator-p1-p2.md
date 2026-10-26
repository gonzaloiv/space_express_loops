# Review — Space Shaping puzzle creator (P0 leftovers + P1 + P2)

**Scope:** commit `9489959` "Add P1 and P2 in the puzzle creator plan" against
[the plan](../plans/in-progress/space-shaping-puzzle-creator.md) and the
[design docs](../design/README.md).

**Verdict:** the implementation matches the plan's *shape* well (P0 leftovers done, P1/P2
present, right assemblies, right data model), but three behaviours the plan specifies are not
actually implemented, and the plan doc has drifted behind the code in several places.

## What matches

- **P0 switch done.** `PuzzleFieldGenerator.Generate` calls `SectorShape.Generate` directly
  (`PuzzleFieldGenerator.cs:10`); the `SectorField.Generate` / `cellSize` trick is gone.
- **P0 separation margin done.** `PuzzleFieldBuilder.SeparationMargin = 1.1f` →
  `minSeparation = separationScale × 1.1` (`PuzzleFieldBuilder.cs:15,46`), matching §4/§6.
- **P0 artifact export done.** `PuzzleShapeAsset` (wraps `SectorShapeData`), `PuzzlePlanetListAsset`
  (wraps `PuzzleFieldPlanet[]`), `PuzzleArtifactFactory` + Editor-only `PuzzleArtifactExporter`.
- **P1 done.** `PuzzleZone`, `PuzzleData.solution[]`, `PuzzleVerifier`, extracted
  `SpaceShapingSetup.Build(puzzle, settings)`, plain `PuzzleSimSettings`.
- **P2 done (mechanically).** `origin`/`destination` derived from `distance`
  (`PuzzleData.cs:17-19`), `PuzzleSolutionSearch` with a bounded scale-only grid.
- **Assembly placement is right.** Verifier/search in the pure `…Creator` runtime asmdef, window
  and exporter under `Creator/Editor/`, game never references the creator.
- **Design docs honoured.** One hard fail (window) + one score (entropy); entropy charged per
  modification; launch stays a global settings constant; scale-only v1 (region job 3, rotate job 4);
  discreteness is authoring-only; no runtime generation; no LINQ; size/parameter limits respected.

## Mismatches vs the plan (should fix)

1. **The verifier accepts a crashing start.** `PuzzleVerification.StartFails` is
   `StartOutcome != Arrive` (`PuzzleVerification.cs:11`), so `Crash` counts as "start fails".
   §5 step 4 and §9 require the start to **fail on time — a near-miss, not a crash**. Should be
   `StartOutcome == Miss`, with `Crash` explicitly rejected.

2. **The search ignores the window and the budget.** `PuzzleSolutionSearch.Arrives` returns on
   `Outcome == Arrive` only (`PuzzleSolutionSearch.cs:50-60`). §6 P2 says it must return a
   candidate that arrives **in the target window**. Nothing checks `[tmin,tmax]` or
   `EntropySpent ≤ budget`. `PuzzleSearchTests.FoundSolution_VerifiesGreen` hides this by *setting*
   the window from the found `ShipTime` — circular, so the `windowTightness` dial is never enforced.

3. **The "reject and resample" step is missing.** Nothing checks that the bare start fails, or that
   it crashes; `TryFind` returns the first arriving zone set regardless (§5 step 4). Combined with
   (1) and (2), P3's "force the start to fail on time" has no foundation yet.

4. **The v1 cube spans the whole run.** `TryFind` builds one zone with
   `size = Vector3.one * puzzle.distance`, `centre = Vector3.zero`, `falloff = 0`
   (`PuzzleSolutionSearch.cs:18,38-47`), so the only v1 solution is a **global uniform zoom** — not
   the design's local **shortcut** (cube.md: "a shrunk zone is a shortcut"; vision §3: "the cube's
   size is the aperture"). §5 fixes the cube *centre* at v1 but never says it is the size of the
   run. Fix the cube to a fraction of the run (e.g. around the cluster/path midpoint) so v1
   solutions demonstrate the actual verb.

5. **Intermediates are committed, and split across folders.** §9/§10 say intermediates live under
   `Generated/` and **only the final `PuzzleData` is committed**. The commit adds
   `Generated/PuzzleSeed_Shape_Planets.asset` (stage 2) *and* `Resources/PuzzleSeed_Shape.asset`
   (stage 1) — one committed intermediate plus an inconsistent home for stage 1 (the exporter's
   `GeneratedFolder` is `Generated`, `PuzzleArtifactExporter.cs:9`).

6. **Plan doc is stale relative to the code** — worth a doc pass:
   - §6 P0 "Still to do: export each stage's artifact" — done.
   - §3/§8 "Stage 1's artifact is currently a `SectorFieldData`; becomes `SectorShapeData` when §3's
     switch lands" — the switch landed.
   - §2 lists `FieldGap` as a builder constant; the code has `SeparationMargin` and no `FieldGap`.
   - §8 lists `SpaceShapingSetup` under `…Creator`; it now lives in the **game** assembly
     (`Scripts/Domain/Puzzle/SpaceShapingSetup.cs`), which is correct because `SpaceShapingState`
     uses it — the §8 list should move.
   - §2 makes the seed **three fields** (`seed`, `distance`, `planetCount`), but §6 P2 and §9 still
     reference a seed **`steps` target** ("reports the actual modification count against the seed's
     `steps` target"). No such field exists.

7. **§4 contradicts itself.** §4 calls planet size "the one open decision in this plan… Pick one and
   write it down", while §13 says "the bridge's `radius * 2f` stays **as-is** for now". State it once
   in §4 and drop the open-decision framing.

## Improvements

- **`PuzzleArtifactExporter` churns GUIDs.** `DeleteAsset` + `CreateAsset`
  (`PuzzleArtifactExporter.cs:21-22`) mints a new GUID on every re-export, breaking any reference to
  the previous artifact — the opposite of "each stage is re-runnable". Load-and-overwrite
  (`AssetDatabase.LoadAssetAtPath` + `EditorUtility.CopySerialized`, or create only when absent).
- **`SpaceShapingSetup` duplicates `PuzzleSimSettings`.** It re-declares the eight sim scalars as
  flat fields (`SpaceShapingSetup.cs:9-15`) plus puzzle fields. Hold a `PuzzleSimSettings` snapshot
  instead — one value, not parallel fields.
- **`SourceSpecs.From` and `MoveRange`.** `MoveRange = scale` is per-source, but §4 says the whole
  envelope is "derived from `SpaceShapingSettings`". `SourceSpec.MoveRange` is already flagged as
  vestigial in cube.md §Open — either state it is per-planet or drop it here.
- **`PuzzleZone.size` → half-extents footgun.** `ToSpaceZone` silently halves `size`
  (`PuzzleZone.cs:13`) while `SpaceZone.HalfExtents` is named for what it is. Rename the field to
  `halfExtents` (no comment needed) so callers can't pass a full size.
- **Test hygiene.** `PuzzleSearchTests` mutates a shared `puzzle` across tests
  (`PuzzleSearchTests.cs:14-24,41-60`) → order-dependent; build a puzzle per test. The seed scan
  `1..32` with a hard `Assert` couples the suite to generator tuning — pin a known-good seed or
  assert search behaviour instead.
- **`Build_FieldFitsThePlaySpace` is trivial.** `shape.radius.max < seed.distance`
  (`PuzzleFieldTests.cs:39`) is always true (≈6.2 < 12) and tests nothing; the meaningful fit check
  is `Cluster_FitsInsideTheRun`. Assert the extent (`radius.max × spread`) or drop it.
- **Commit hygiene.** The commit mixes P0 leftovers, P1, P2, and unrelated scene cleanup
  (`samplesPerLine`, `hud`/buttons) — split for reviewability.
- **`PuzzleData.solution` ships in the player asset.** §7 says the runtime loop does not need it, so
  it is authoring metadata carried into the build. Fine if intended; otherwise keep it in the
  creator artifact.

8. **The creator's zone geometry ≠ the game's zone geometry.** The view builds its zone as
   `new(Centre, Size * 0.5f, scale, falloff)` with `falloff = 0.5` and a `contentInset = 0.8`
   (`SpaceZoneView.cs:7-8,14,36`), so the scaleable region is the *inset* box. The creator's
   `PuzzleZone.ToSpaceZone()` uses `size * 0.5f` with **no inset** and `falloff = 0`
   (`PuzzleZone.cs:13`, `PuzzleSolutionSearch.cs:46`). A solution found by the creator therefore
   does not reproduce in the game with the same numbers — the search must use the same region and
   falloff the view uses.

## Decisions (settled)

- **Planet size band — keep the larger band, revisit with tests.** The builder's
  `distance × 0.06 ± 30 %` (0.50–0.94 at distance 12, ×2 through the bridge) stays. Whether the
  puzzle should match the universe's smaller look is a **lab/test** question, not a doc decision.
  Record it as such in §4 instead of "the one open decision".
- **v1 cube — smaller, and it is the puzzle's authored region.** The cube is smaller than the run,
  but only for the puzzles whose cube is **not** player-editable (progression.md §4: jobs 1–2 are a
  *fixed region*, only the grab-inside scale moves). Consequences:
  - The fixed cube (centre + half-extents + falloff) must be the **same authored source** the view
    presents — the creator must not invent `size = distance`. Today the view's cube comes from the
    `SpaceZoneView` transform, so the creator should read that source (or the puzzle should carry it).
  - The search grid is then over **scale** within that fixed region (§5's "scale at the cube's
    centre" is only true once the region is the authored cube, not the whole run).
  - For puzzles where the player *can* resize the cube (job 3+), no authored cube size is needed —
    the player picks the region. Those are out of this slice.

## Suggested next step

Land (1)–(3) and (8) before P3 — they are the contract P3's bake depends on. (4) is settled: a
smaller, authored v1 cube; bake variants only once the creator's cube is the same region the view
shows, so generated puzzles read as shortcuts rather than a global zoom.
