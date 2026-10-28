# Review — Space shaping multi-zone

**Date:** 2026-10-28.
**Scope:** [multi-zone plan](../plans/in-progress/space-shaping-multizone.md) against the code
(`Assets/_Game/Features/PuzzleCreator`, `Assets/_Game/Features/SpaceShaping`) and
[cube](../design/systems/cube.md), [progression](../design/systems/progression.md),
[jobs](../design/jobs.md).
**Verdict:** the bake and the player share one geometry and the two-zone solution reproduces through the
same session; the blockers were author-facing copy and a sort contract, now fixed. M3's headset pass is the
only open gate.

## Findings

1. **The author is told the wrong fix.** `UnsupportedBrief` said "this creator ships one zone … Set moves to
   1" (`PuzzleCreatorWindow.cs:196`), and the seed tooltip said multi-zone waits for this plan
   (`PuzzleSeed.cs:19`). A fixed two-zone job-3 seed would be downgraded instead of made translatable.
   Fixed: the message now names the real rules, the tooltip says 1–3 zones and translatable.
2. **The beam sorted with an epsilon comparator.** `Order` used `Precedes` with `SameMargin`/`Cheaper`
   (`PuzzleBeamSearch.cs:159`), whose epsilon ties are not transitive; `List.Sort` may throw an
   inconsistent-comparer error or return an order-dependent beam. Fixed: `Order` compares exact margin,
   then exact cost, then window distance.
3. **The disjoint rule lived in the search and had no negative test.** The overlap guard was private to
   `PuzzleBeamSearch`, and `Bake_ProducesTwoZones` only checked that the *found* zones do not overlap.
   Fixed: the rule is `PuzzleZone.Overlaps(other, gapFactor)` (`PuzzleZone.cs:17`) and
   `PuzzleMultiZoneTests.Zone_OverlapUsesTheSharedGapRule` covers touching, same-centre and far boxes.
4. **Off-path multi-zone passed the gate.** `SupportsMoves` allowed `OffPath && Moves > 1`
   (`PuzzleBrief.cs:20`), and `PuzzleSolutionSearch.TrySelect` routed every `Moves > 1` brief to the beam
   before the clock path (`PuzzleSolutionSearch.cs:20`). The beam then accepted an arrival in the default
   `0…12` window because an off-path window is only derived after solving
   (`PuzzleFieldBake.cs:101`). Fixed: off-path stays single-zone in `SupportsMoves`; a test pins it.
5. **The plan read as pre-implementation.** It said "only the creator search is single-zone"
   (`space-shaping-multizone.md:37`) after the beam landed. Fixed in the same change.
6. **The bake proved two zones but not that one zone fails.** Seed 1's baked level also had a one-zone
   arrival in the same window, so the POC could be solved with a single cube. Fixed:
   `PuzzleFieldBake.HasShorterSolution` checks every smaller zone count at the baked cube size and fails
   `FewerZones` (`PuzzleFieldBake.cs:64`); seed 1 is the negative test, seed 2 is the POC bake.

## Open

- **M3 is not proven until the headset pass.** The 2-zone asset is baked and wired as
  `SpaceShapingState.puzzles[0]` in `SpaceShapingPOC.unity` (`Generated/Multizone_Seed2_Puzzle.asset`),
  but nobody has played it. The plan's M3 checklist still owns "reproduce it with two moved zones".
- **The beam's first step is a heuristic.** One chain per centre + size, ranked by closest approach
  (`PuzzleBeamSearch.cs:44`). Seed 1 bakes on the first attempt, but a job with no margin-competitive prefix
  needs the beam widened (or two slots per centre + size), not another special case.
- **The verifier does not re-check disjointness.** `Fits` checks count, size, shared size and scale
  (`PuzzleVerifier.cs:35`), not overlap. The search only ever emits disjoint chains, so this cannot go
  silently wrong today; if a hand-authored solution ever enters `Bake`, add the check there.
- **The baked asset has no `id`.** `LevelData.id` is empty, which is fine for the POC; set it when the
  level enters the campaign, before any save key is derived.
- **The POC scene was saved for the wiring.** Keep that scene diff with the asset, not with unrelated
  content, when it is committed. The superseded `Generated/Multizone_Seed1_Puzzle.asset` is on disk but
  unreferenced; delete it with the commit.

## Settled

- **One geometry for author and player.** Search zones use `PuzzleZoneCandidates.Zone` → half-extents and
  `settings.ZoneFalloff`; the runtime reads the same values through `SpaceShapingView.SetZoneFrame` →
  `SpaceZoneView.Zone`; the bake and verifier share `PuzzleZoneRun` / `SpaceShapingSetup`.
- **One size per chain** — job 3 is `resizable = no` (`jobs.md` row 3), so the frame gesture cannot change
  size; the baked cube size is the chain size.
- **No solution array ships.** The bake returns `PuzzleZone[]` and persists only zone 0's frame
  (`PuzzleFieldBake.cs:129`); `moves` is the only zone metadata in `PuzzleData`, and the game never shows
  it (`cube.md:36`).
- **Offline cost is acceptable.** Seed 1's two-zone bake is found on the first attempt; the 16-attempt
  probe took about five seconds. No pruning was added for speed.
- **Negative paths are covered** — fixed multi-zone, rotate, wrong window, over-budget and missing solution
  all have tests; the overlap rule now has its own.

## Applied

- 2026-10-28 — fixed the unsupported-brief copy and seed tooltip; made `Order` a strict comparator; moved
  the overlap rule to `PuzzleZone.Overlaps` and tested it; rejected off-path multi-zone in
  `SupportsMoves`; updated the plan's implementation sentence and linked this review; added
  `PuzzleMultiZoneTests` (2-zone bake, determinism, midpoint frame, 3-zone search, runner overload,
  overlap rule, shorter-zone rejection). Verified: 39/39 creator tests, 123/123 EditMode,
  `docs/check.ps1` green.
