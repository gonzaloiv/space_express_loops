# Bake — role and sun alignment

**Status:** now · space shaping · updated 2026-11-05 — S1/S2 landed; jobs 1–3 bake; the rejected
second-blocker probe is removed. **Job 4's block is provisional:** `MakeBlockerFatal` is implemented
(original position, move-only, or grow in place at 1.5×/2×/2.5×/3×/4×); §6.6 (job-4 flag), §6.3's
report/score, and §6.7's existence-only role probe/`Zones = 1` trap/cheap prefilter/`WellMove` gates are
landed. **A+B is implemented and negative:** the 6-planet Ring home field is `NoSolution`
(`pairs 0`, `accepted 0`, 6 planets, blocker 0 `r 0.029`), not `ClearSingle`/`Mixed StartTooStraight`; the
6-planet variant has no run. **B is negative; the §7 C/D decision applies** — C (multi-zone Curve job 4,
job 8 first Mixed) is the least invasive; D if schedule wins; E rejected. The contract stays `Mixed`/
`Zones = 2`/`MinRoutes = 2`/`MinBlockers = 1`; `Zones = 1` and `Zones = 3` are out of plan. Jobs 5–8 and S4
remain.

**Related:** [vision](../../design/vision.md) · [jobs](../../design/jobs.md) ·
[progression](../../design/systems/progression.md) · [core loop](../../design/core-loop.md) ·
[the cube](../../design/systems/cube.md)

**Scope:** `PuzzleJobContract`/`PuzzleJobContracts` is the job seed spec, and `PuzzleJobValidator` is an
editor-time acceptance gate — not a separate plan and not a runtime system. Fold its checks into
`PuzzleBrief`/`PuzzleFieldBake` as S1–S3 land; keep the validator test-only.

**Design constraints:** the field is **derived, never edited**; a blocker is a generator rule, not a
hand-edited `PuzzlePlanetListAsset`. The job ladder and roles stay as designed — job 2 is the centre, job 3
has two blockers — and the bake must meet them. If job 2 cannot make the star the cheapest `Curve`, that is
a job/field decision, not a reason to change the field rule silently.

**Current state (2026-11-05):** S1 (`Matches(Any, Other)` is false, role costs logged) and S2
(`PuzzleRunRequest`, planet-blocker/sun-chord preference, `PuzzleRunChoiceTests`) are in, along with the
sun/start rule, contract fields, paths, log, test-only validator, and loose-fallback removal. The focused
`PuzzleRunChoiceTests`/`PuzzleSolutionClassifierTests`/`PuzzleBakeTests`/`PuzzleSearchTests` pass 29/29
(including the 18/18 run/validator tests). The S3 diagnostic is in: `PuzzleRunReport` shows job 2 as
6 pairs / 2 length / 4 no-planet / 0 sun-chord, and job 3 as 10 pairs / 2 length / 8 no-planet / 0
sun-chord; widening the planet band to 4× does not change the counts. The report now counts
`NoPlanetBlocker` and `SunOnChord` independently.
The true pin, separate `SunOnChord`, independent report counts, `Fits` sun check, placement tests, edge
offset (1.5×), six-planet job 3, and the role-dependent `Clear` offset (0.5×) are in;
`PuzzleFieldTests` pass 26/26. The explicit `PuzzleJobBakeTests` is 2/4: jobs 1–3 bake (job 3 seed 146,
two routes); the earlier job-4 scan reported `WrongRoleRoute` on its best post-clear attempt (seed 7 itself
is `NoSolution`) with a pinned accepted run (`pairs 1`, `accepted 1`); jobs 5–8 are not
reached by `JobSet`; the two-ship fixture is the remaining ordinary failure. Job 4 is **`MakeBlockerFatal`**: move the existing blocker or grow its radius in place while keeping it
inside the run's block band, then verify the pinned `TryPick`/`Apply` candidate and the five checks:
(1) the single `Clear` still fails the well, (2) the single `Curve` fails the blocker, (3) the `Mixed`
clears the required sources and grows the well, (4) the `Mixed` is strictly cheapest, and (5) its
two zones cover the required sources. **Landed:** `PuzzlePostClearPlacement` tries the original position,
move-only candidates, and radius scales 1.5×/2×/2.5×/3×/4×; the rejected second-blocker probe, its
report/test fields, and `TryPickCandidate` are removed. The `ref` plumbing and the best-candidate score
are fixed. **The existence-only probe is landed:** `CheckRole` uses `TrySelect` + `Matches` +
`PuzzleVerifier` + `SunRule`, skipping start/cheapest/routes; `CheckRoles` adds the `Zones = 1` singles;
`CheckFull` runs the full `Bake` only for a Mixed existence route with `MixedCovers`; `WellMove.Try` requires
a start crash, no single-zone Clear, a Mixed route with `MixedCovers`, and a Curve search path. **A+B is
implemented:** the no-Curve branch now `Verify`s the home field and keeps it as `sweep.Best`, with no
`WellMove`/sampling changes; job 4 is a 6-planet Ring. **The result is negative:** the 6-planet home field
is `NoSolution` (`pairs 0`, `accepted 0`, blocker 0 `r 0.029`, width 0), not
`ClearSingle`/`Mixed StartTooStraight`; the 6-planet variant has no run. **B is negative; take §7 C/D**
(C: multi-zone Curve job 4, job 8 first Mixed; D if schedule wins). The rejected probe restores the well
(its test passes), the ladder reaches `planetRadiusMax` (4×), and the scanner prefers the furthest stage.
§6.6 scopes the probe to job 4; §6.7's existence/gates are in. The pin (§6.5) is not the
immediate blocker. No second blocker fits (the old Clear half-width 0.036 vs the ~0.120 `Fits` separation);
check `PuzzleZoneCandidates`/the cube max before treating it as viable. Keep
`Mixed`/`Zones = 2`/`MinRoutes = 2`/`MinBlockers = 1`; `Zones = 1` is a job change outside the plan, so
**job 4's block is provisional** — do not silently relax it. Jobs 5–8, the collect-all harness, the
filtered two-ship gate, and S4 remain.

## 1. Why

The 8-job re-bake exposes four mismatches between the design and the bake:

- **The role search rejects legitimate routes.** Search and verification already use the same classifier
  (`PuzzleZoneRun.cs:39`, `PuzzleVerifier.cs:31`); `Variant` only changes budget, frames and zone count,
  none of which feed classification (`PuzzleFieldBake.cs:170-177`). The real hole is `Other`: an off-chord
  shrink that fixes a miss is classified `Other` (`PuzzleSolutionClassifierTests.cs:66-77`), so the
  cheapest `Any` route can be a role the requested search cannot see. Moving the role check after
  verification is a no-op.
- **The sun is a blocker, not a well.** `PuzzleRunChoice.RunIsBlocked` accepts a run blocked by the sun
  (`PuzzleRunChoice.cs:55-62`), so a sun field's cheapest fix is to shrink it (`Clear`). A `Curve` job
  with a sun then falls back to `Clear`.
- **The solved zone ignores the sun.** `BuildFrames` uses `solution.Zones` (`PuzzleFieldBake.cs:180-196`).
  Jobs 3 and 4 currently bake their solved frame at `{0,0,0}` — the sun — against
  [jobs](../../design/jobs.md) ("the sun stays out of the zones"). Job 2 has the opposite requirement:
  its zone must contain the sun.
- **Paths, shape and routes are stale.** The test set lives in `Assets/_Game/Resources/Chapters/01/`, the
  exporter writes to `PuzzleCreator/Resources`, and the settings asset is at
  `PuzzleCreator/Data/SpaceShapingSettings.asset`, not `Puzzles/Data`. The builder never sets
  `fieldShape`, job 5 is a loose one-route bake, and the Creator bake fixtures now fail with `NoSolution`
  on seed 1.
- **A parallel validator is a second truth.** `PuzzleBrief`/`PuzzleSeed` already describe the expected
  bake (`PuzzleBrief.cs:5-18`); the missing constraints belong in the creator and its statuses. The job
  table is the seed spec, not a second system. Keep the post-bake check editor-time (builder/test) as an
  acceptance gate, never runtime code.

The run is the curriculum: if the verified role, zone or route differs from the brief, the level teaches
the wrong lesson. Fix the bake, then author the run.

## 2. Slices

### S1 — diagnose the role search, not the timing

- **Precondition:** restore the known-solvable bake fixture (`PuzzleBakeTests`, now seed 2) so S1 starts
  from a green baseline; the seed move already took the Creator suite from 22 failures to one two-ship
  fixture. That remaining fixture is not S1's exit, and S4 owns it.
- Do not move the role check after verification; it is already the same classifier on the same sources.
- Decide what `Other` means: an off-chord shrink that fixes a miss is either a `Clear` classification
  bug or a route that must be excluded from the cheapest `Any` result. **Landed:** `Other` is excluded
  from `Any`; `Matches(Any, Other)` is false and the compiler tests cover it. The builder’s `FindJob` and
  `PuzzleJobBakeTests` both scan the requested role.
- Keep the cheaper/equal wrong-role rule; if the requested-role route is more expensive than the
  cheapest wrong-role route, the seed still rejects. The status enum has no cost fields: capture the
  role solution in `PuzzleSolutionSelection` and log both costs (or add result fields); do not claim the
  status carries them.
- **Exit:** the `Other` competitor case is covered — an off-chord shrink that clears a miss is either
  `Clear` or excluded, while a grown off-chord source still bakes `Curve`; a cheaper wrong-role route
  still rejects. `PuzzleSolutionClassifierTests`, `PuzzleSearchTests`, and `PuzzleBakeTests` pass.

### S2 — the sun is a well for Curve, and the zone contains it

- Replace `PuzzleRunChoice.TryPick`'s parameter tail with a `RunRequest` struct (planets, distance,
  endpoint clearance, sun radius, blocker preference) so the parameter limit is not broken.
- A `Curve` job must not select a run blocked by the sun: pick a run blocked by a planet and keep the
  sun off the chord as a well. Make "blocked by a planet" versus "blocked by the sun" explicit in the
  request/status. **Landed:** `PuzzleRunRequest` carries `RequirePlanetBlocker` and `AllowSunOnChord`;
  `PuzzleRunChoiceTests` cover sun-only, planet-blocked, sun-on-chord, and `InZone` cases. Job 2 still
  returns `NoRun` under the planet-blocker/sun-off-chord rule, so the derived blocker rule remains S3.
  The start frame (`PuzzleStartRule.FieldCentre` for jobs 5/8) is in `BuildFrames` and the validator.
- Add `PuzzleSunRule` to `PuzzleBrief` and a matching `SunRuleFailed` status. Job 2 is
  `CentredGrown`: the zone must be centred on the star and its scale must grow. `BuildFrames` alone
  cannot enforce this; the creator must reject the seed.
- **Exit:** job 2 bakes a verified `Curve` with `HasSun` and a solved zone centred on the star. If no seed
  can make that route the cheapest `Curve`, record the seed constraint in the Risks section rather than
  falling back.

### S3 — sun out of the zones, routes, and the tutorial

- Job 2 is `CentredGrown`; job 8 is `InZone` (the star is included, not necessarily centred). Jobs
  3/4/5/6/7 are `OutOfZone`. The solved-zone check is already in `PuzzleBrief`/`PuzzleFieldBake`; the
  run choice is S2, and the job seed table and acceptance tests are the remaining wiring.
- Add job 5 to the `minRoutes: 2` set. `HasRoutes` already enforces `brief.MinRoutes`
  (`PuzzleFieldBake.cs:145-149,160-165`); the loose fallback is deleted, not a new check.
- Make the 4-planet tutorial bake a crash. `SectorShape.FillRing` places 2 inner + 2 outer for four
  planets (`SectorShape.cs:58-72`); state the shape (`Ring`) and attempt count, and keep the
  `PuzzleStartCheck` crash requirement (`PuzzleStartCheck.cs:17-18`).
- **Job 1 role fix.** The 3-planet ring only had the blocker as a non-endpoint source, so `Clear` was
  cheaper than the requested `Curve`. Job 1 is now a four-planet ring; seed 37 bakes one `Curve` route
  within 200 attempts. The design table and the tutorial contract follow that change.
- **Job 2/3 diagnostic.** Four-, five-, and six-planet probes (up to 200 attempts) return `NoRun`; more
  attempts do not help. `PuzzleRunReport` shows job 2 as 6 pairs / 2 length / 4 no-planet / 0 sun-chord,
  and job 3 as 10 pairs / 2 length / 8 no-planet / 0 sun-chord; widening the planet band to 4× did not
  change the counts. The no-planet count is the only counted reject, but `Consider` checks it before
  `SunOnChord`, so the sun-chord slot is masked; do not conclude the sun is innocent. Derive a blocker on
  the chosen chord in the generator (no hand-edited `PuzzlePlanetListAsset`, no wider planet band by
  default). Job 3 also has
  seeds where the cheapest role is `Curve` while `Clear` is requested, so the derived blocker must make
  shrinking it the cheapest `Clear`. Job 2’s centre is possible only if growing the sun is the cheapest
  `Curve`; S1’s cheaper-role rule still gates it, so the blocker alone is not enough if `Clear` stays
  cheaper.
- **Derived blocker rule.** The generator needs a two-pass: pick a candidate pair by run length and
  endpoint clearance without requiring a blocker, place one planet within `2 × radius` of the chord for
  job 2 (two for job 3) without overlapping the endpoints, the sun, or other planets, then **pin the
  candidate**: the final selection uses that pair, or verifies the blocker is on the final run.
  **Wired, not a true pin:** the candidate is carried through `PuzzlePlanetListAsset` ->
  `PuzzleRunRequest`, but `TryPick` considers it first and then loops every pair, so another pair can win.
  Pin it: when `HasCandidate`, validate and return the candidate pair, skip the loop. Keep **two**
  candidate runs, one blocker each (the current `TryPickCandidates(count)` + `Place` does this); the
  chosen run is `MinBlockers = 1` and the two blockers give `MinRoutes = 2`, so the player chooses which
  one to shrink (`jobs.md:181`). Do **not** put both blockers on one chord: that makes the single-zone
  `Clear` unsolvable, which is the `01_03` `NoSolution`. Change `PuzzleJobContracts`/`PuzzleJobValidator`/
  the job-bake tests from `MinBlockers = 2` to `1`. Keep the planet band at 2×; a 4× band did not change
  the counts. The blocker is a generator rule, not a hand-edited `PuzzlePlanetListAsset`.
- **Separate SunOnChord field.** Add `sunOnChord` to `PuzzleSeed`/`PuzzleBrief` (all eight false) and use
  it for `AllowSunOnChord`; `SunRule` stays the solved-zone rule — job 8 is `InZone` but `SunOnChord =
  false`. Do not derive the chord rule from `SunRule` or `Mechanism`. Update the placement,
  `BuildRunRequest`, and the validator; the `KeepBlockersOffTheSun` test uses a sun-on candidate for an
  `OutOfZone` job and asserts the chosen candidate run is sun-off. **Landed:** the sun is in `Fits`, the
  `NoSolution` path carries the report, and `PuzzleBlockerPlacementTests` cover the placement.
- **Role-cost check.** The derived blocker must make the requested role the cheapest: `Clear` for job 3,
  and `Curve` for job 2 (the star grows). If the star growth cannot be the cheapest `Curve`, job 2 is a
  field decision — or an explicit job change outside this plan — not a forced `Curve`.
- **Job 4 `MakeBlockerFatal`.** **Landed; A+B is implemented.** §6.6 scopes the probe to job 4. §6.7's
  `CheckRole` is existence-only (`TrySelect` + `Matches` + `PuzzleVerifier` + `SunRule`), `CheckRoles` adds
  the `Zones = 1` singles, `CheckFull` runs the full `Bake` only for a Mixed existence route with
  `MixedCovers`, and `WellMove.Try` keeps its start/no-single-Clear/Mixed/Curve gates. The no-Curve branch
  now `Verify`s the home field and keeps it as `sweep.Best` (no `WellMove`/sampling changes), and job 4 is a
  6-planet Ring. **The probe result is negative:** the 6-planet home field is `NoSolution`
  (`pairs 0`, `accepted 0`, blocker 0 `r 0.029`), not `ClearSingle`/`Mixed StartTooStraight`; the 6-planet
  variant has no run. **B is negative; §7 C/D applies** (C: multi-zone Curve job 4, job 8 first Mixed; D if
  schedule wins). The growth ladder reaches `planetRadiusMax`; §6.2 remains. A 200-seed sweep
  (7–206) finds no `Mixed` bake. `Zones = 1`/`MinRoutes` reset is a job change outside the plan. **Decision:
  provisional; do not relax the contract.**
- **Exit:** job 2 bakes a planet-blocked, sun-off-chord `Curve` whose zone is centred on the star and
  grown; job 3’s derived blockers make `Clear` cheapest and offer two routes; job 4’s existing blocker
  fatal on the single-Curve path makes the single `Clear` fail the well and the single `Curve` fail the
  blocker; the two-zone `Mixed` is the only route; a four-planet `Ring`
  tutorial bakes its requested role and a crash; jobs 7/8 offer two routes; job 5 is no longer a loose
  one-route bake; no fixed-level solved zone sits on the sun except job 2.

### S4 — paths, shape, log, re-bake

- Point the builder at `PuzzleCreator/Data/SpaceShapingSettings.asset`, and make `SaveVariant`/the
  builder write to `Assets/_Game/Resources/Chapters/01/` (the assets the chapter already references).
  Chapter `01.asset` currently references all eight jobs; keep those GUIDs and update in place.
- Add `FieldShape` to the job contract/builder and set `seed.fieldShape`; restore Ring → Spiral →
  Cluster. The builder now sets `seed.fieldShape = job.FieldShape`; keep it.
- Log the baked `Kind` and the bake status. The builder bakes the requested mechanism directly, with no
  `Any` + final-kind filter and no loose/duplicate fallback; keep `FindJob` that way. The builder no
  longer calls `PuzzleJobValidator`; the validator is an EditMode acceptance helper only.
- **Start frame.** `PuzzleStartRule` is in `PuzzleSeed`/`PuzzleBrief`/the contract: jobs 5/8 start at the
  field centre (the sun), jobs 6/7 at the run midpoint. `BuildFrames` and `PuzzleJobValidator` enforce it;
  keep the acceptance coverage before the capstone re-bake.
- **Acceptance fixtures.** The explicit job-bake tests load
  `PuzzleCreator/Data/SpaceShapingSettings.asset`; default-settings tests are not the production ramp.
  Run `PuzzleJobBakeTests` deliberately, not as part of the ordinary suite.
- The two-ship fixture is `[Explicit]` (multi-ship is out of scope). The green gate is the Unity Test
  Runner or a filter that excludes it; the MCP suite includes explicit tests and stays red on that
  fixture.
- **Exit:** eight assets, no duplicates/fallback warnings, chapter references the eight updated assets,
  log shows 1 `Curve`, 2 `Curve`, 3 `Clear`, 4 `Mixed`, 5 `Curve`, 6 `Curve`, 7 `Curve`, 8 `Mixed`,
  routes ≥ 2 on 3/4/5/7/8, and the Creator bake tests are green. **Job 4 is excluded from the S4 gate
  while its block is provisional:** either it bakes `Mixed` after the §6 fixes, or the §6.8 curriculum
  fallback replaces it and the log/board show seven jobs.

## 3. Tests

- The classifier decision for an off-chord shrink that fixes a miss is explicit and covered.
- A requested role bakes only when its route is the cheapest; the role solution and both costs are
  captured in the search/log (the status enum has no cost fields).
- A `Curve` sun job selects a planet-blocked run; job 2's zone is centred on the star and grown.
- The derived blocker picks a candidate run by length/clearance without requiring a blocker, places one
  planet within `2 × radius` of the chord for job 2 (two for job 3) without overlapping the endpoints,
  the sun, or other planets, then pins that candidate for the final selection; a 4× band does not
  substitute for this.
- `PuzzleBlockerPlacementTests` assert the true pin (the final `TryPick` returns the candidate), the two
  job-3 blockers on **two runs with one each** (the chosen run `MinBlockers = 1`, `MinRoutes = 2`), the
  chosen run is sun-off for an `OutOfZone` job, the endpoint/planet separation, and the `NoSolution`
  report is non-zero; `01_02` must show a planet blocker rather than the current 4 no-planet pairs.
- The `NoSolution` path carries the run report; `01_03` reports its pairs/filters instead of 0.
- The derived blocker makes the requested role the cheapest: `Clear` for job 3, `Curve` for job 2; if the
  star growth cannot be cheapest, job 2 is a job/field decision.
- Job 4: `MakeBlockerFatal` is landed; **A+B is negative**: the 6-planet Ring home field is `NoSolution`
  (`pairs 0`, `accepted 0`, 6 planets, blocker 0 `r 0.029`), so §7 C/D applies. The home-field-on-`NoCurve`
  report is landed; Mixed-path/run-band sampling and `WellMove`-without-Curve were not taken (B used the
  6-planet variant). The dedicated explicit tests are `Job4_MakeBlockerFatal_ReportsFiveChecks` (red by
  design; not the green gate) and `Probe_Rejected_LeavesFieldUnchanged` (passes). The best-attempt metric
  uses the furthest stage, then width; the 200-seed sweep is still a log artifact. `01_04` is
  **provisional**, not claimed. `Zones = 1` remains a job change outside the plan.
- Job 2 is `CentredGrown`; job 8 is `InZone`; jobs 3/4/5/6/7 are `OutOfZone` and have no solved zone on
  the sun. Jobs 5/8 start at the field centre; jobs 6/7 start at the run midpoint.
- A four-planet `Ring` tutorial bakes a crash; a one-route field fails `minRoutes: 2` for jobs 3/4/5/7/8.
- The explicit job-bake tests use the production settings asset and are run deliberately.
- The acceptance harness collects every job failure (`Assert.Multiple` / per-job results); one `NoRun`
  does not hide jobs 4–8.
- The start-frame validator test asserts with `Has.Some.Contains` on the message list; the previous
  `Does.Contain` failure was a test assertion, not a validator result.
- The ordinary `PuzzleBakeTests` / `PuzzleSearchTests` / `PuzzleMultiShipBakeTests` still create default
  settings; only `PuzzleJobBakeTests` is production evidence.
- `[Explicit]` is honored by the Unity Test Runner, not the MCP suite; run the ordinary suite from the
  Test Runner, or filter the explicit tests.
- The 8-job build log has no `did not bake`, `duplicated`, `loose`, or bake-status mismatches.

## 4. Out of scope

- Changing the job ladder, lore, or classroom copy.
- Resize, multi-ship, or new gestures.
- The scale ladder and ghost.

## 5. Risks

- **`Other` is a real route.** If it is legitimate, excluding it may hide a valid solution; decide with
  a test, not by moving the role check.
- **Sun as a well.** If no derived blocker makes the star's growth the cheapest `Curve`, the centre job
  needs a job/field decision; record it instead of falling back or forcing the `Curve`.
- **Sparse fields.** A sparse-field `NoRun` may be a run-choice constraint; fix the constraint, not the
  count.
- **Job 1 role fix.** `Clear` was cheaper than `Curve` on the 3-planet field; job 1 is now four planets so
  it has a non-blocker well.
- **Job 4 `MakeBlockerFatal`.** The existing blocker is also the run blocker; moving it can invalidate
  the pinned candidate, and growing it changes the cube range and visual separation. The bounded sweep
  (original + 1.5×/2×/2.5×/3×/4×) is negative, but **`NoCurve` is a probe deadlock**: `TrySweep` aborts
  when no Curve path exists (`PuzzlePostClearPlacement.cs:150-162`), so the home field is never verified;
  `WellMove.Try` requires the Curve path the job wants to eliminate (`:400-406`). The best pre-gate
  `ClearSingle`/`Mixed StartTooStraight` is the real signal. Next: the §7 decision (home-field report, drop
  the Curve requirement, sample from the Mixed path/run band; then a bounded field variant or §6.8). The
  `ref` plumbing, best-candidate score, existence-only `CheckRole`, `Zones = 1`, `MixedCovers`, and well
  restore are landed. Do not relax the contract. `Zones = 1`/`MinRoutes` reset is out of plan. The blocker
  remains a generator rule, not a hand-edited field.
- **Derived blocker.** Job 2/3 must stay derived: a generator rule places a blocker on the chosen chord;
  do not hand-edit `PuzzlePlanetListAsset`. Pin the candidate; the two job-3 blockers use **two runs, one
  each**, with `MinBlockers = 1` on the chosen run and `MinRoutes = 2`; keep `SunRule` for the solved zone
  and add a separate `SunOnChord` field (all eight false), never derive the chord rule from `SunRule`; the
  two-pass placement must respect endpoint clearance, the sun, and planet separation; the placement and
  final request must share the chord rule; the `NoSolution` path must carry the report; the report must
  count the sun independently of the no-planet check. The S1 cheaper-role gate still applies after the
  blocker.
- **Paths.** Moving the exporter without moving the chapter references orphans the assets; update the
  chapter GUIDs in place.
- **Test set.** The loose fallback is gone; the builder only ships strict bakes.
- **Second truth.** A `PuzzleJobValidator` that rechecks the brief will drift from the creator; keep the
  checks in `PuzzleFieldBake` and keep the validator test-only, if it survives at all.

## 6. Code-agent review actions (2026-11-05)

The job-4 block is **provisional** until these land. The contract stays `Mixed`/`Zones = 2`/
`MinRoutes = 2`/`MinBlockers = 1`; do not relax it.

1. **Restore the well on a rejected probe.** `PlaceWellForClear` moves `planets.wellIndex`
   (`PuzzlePostClearPlacement.cs:55-78,114`) and `Apply` restores it when the probe is not kept
   (`:23-29,85-89`). **Landed:** `Probe_Rejected_LeavesFieldUnchanged` passes.
2. **Widen the option the plan names.** The probe only grows the blocker
   (`PuzzlePostClearPlacement.cs:251-254`); settings allow `planetRadiusMax = 0.06`
   (`SpaceShapingSettings.asset:21`) and `CubeRange` allows 0.132 (`PuzzleFieldBake.cs:101-102`). The
   ladder now includes 4× (0.018 → 0.060) and the last report shows `r 0.060`; the growth side is landed.
   The search already sweeps `PuzzleZoneCandidates.Sizes`/`Scales` within that range, so the missing
   experiment is a deliberately wider `cubeSizeMax` (or a targeted covering zone), not another width loop
   inside the probe. Record the best result per scale before concluding a job/field change.
3. **Record why a candidate failed — fix the best-candidate sentinel.** **Landed:** `Verify` passes the
   report by `ref`; `Score` returns 0 for `!Tried`, ranks a Mixed-route field ahead of a no-route field,
   and `MakeBlockerFatal` takes the base report's `Stage` after `TrySweep`. The full-gate best report is
   `NoCurve` (the well move removed the Curve path); the earlier `(0.15,-0.01,-0.23)` /
   `clear/curve True` / `mixed StartTooStraight` result was pre-full-gate and is superseded. `Moved`/`Grown`
   still mean "kept."
4. **Fix the best-attempt metric and commit the sweep.** `PuzzleSeedScanner.Record` now prefers stage, then
   width (`PuzzleSeedScanner.cs:54-90`), and reports the matching seed/status/report; landed. Still open:
   the 200-seed sweep (7–206) as an `[Explicit]` test or log artifact. **Landed:**
   `Probe_Rejected_LeavesFieldUnchanged` now asserts the fixture is rejected instead of ignoring it.
5. **Pin the run actually used — not the immediate job-4 blocker.** The real best candidate has
   `pairs 1`/`accepted 1`; the pinned run is valid. Keep the index pin/fallback test for job 3's two-run
   `Place` (`PuzzleBlockerPlacement.cs:72-80`), but the job-4 gap is pure-route removal and the Mixed start,
   not the pin.
6. **Scope `MakeBlockerFatal` to job 4.** `ShouldRun` gates on `seed.makeBlockerFatal`
   (`PuzzlePostClearPlacement.cs:32-35`), not `Mixed` alone, and job 8 is Mixed
   (`PuzzleJobContracts.cs:60-67`). **Landed:** `PuzzleJobContract.MakeBlockerFatal`/
   `PuzzleSeed.makeBlockerFatal` gate the probe; job 4 is true and job 8 false.
7. **Measure pure-role existence, not just cheapest.** This was the job-4 metric bug.
   `RoleBakes` returned `baked.Baked` (`PuzzlePostClearPlacement.cs:217-226`), but
   `PuzzleSolutionSelection.Select` returns `WrongRoleRoute` when a Clear/Curve search solution exists and
   is merely more expensive (`PuzzleSolutionSelection.cs:25-32`). **Landed:** `CheckRole` is an
   existence-only probe (`TrySelect` + `Matches` + `PuzzleVerifier` + `SunRule`, skipping
   start/cheapest/routes); `CheckRoles` adds the `Zones = 1` singles; `CheckFull` runs the full `Bake` only
   for a Mixed existence route with `MixedCovers`; `WellMove.Try` requires a start crash, no single-zone
   Clear, a Mixed route with `MixedCovers`, and a Curve search path. **With A, the no-Curve branch verifies
   the home field:** the 6-planet Ring variant reports `NoSolution` (`pairs 0`, `accepted 0`), so the
   home-field diagnostic and B are both negative. **Take §7 C/D.** The 200-seed artifact remains open.
8. **Decide the curriculum fallback (design-only, no code).** Job 4 is the Ring act's Mixed lesson. If it
   stays blocked, decide whether the run ships seven jobs, substitutes a Mixed job, or brings job 8's
   lesson forward; update `jobs.md` and the chapter. Do not leave job 4 in the S4 exit gate while
   withholding it.

**Next order:** B is negative; take C/D. Do not run another seed sweep or add probe features. §6.5 is job-3
hygiene, not the job-4 blocker.

## 7. Job 4 decision (2026-11-05)

The probe is now sound; `NoCurve` is a sampling deadlock, not a field verdict. The best pre-gate candidate
has `ClearSingle=True` and `Mixed StartTooStraight`, and the 200-seed sweep (7–206) is negative. The
8-planet Ring is likely over-constrained; another probe feature or seed sweep is not a plan.

**Decision (2026-11-05): A+B; B is negative.** The 6-planet Ring home field is `NoSolution`
(`pairs 0`, `accepted 0`, 6 planets, blocker 0 `r 0.029`), not `ClearSingle`/`StartTooStraight`; the
6-planet variant has no run. Take C (or D if schedule wins). E is rejected: do not relax "Mixed is the only
route" without a design change.

**Recommendation (record):** one last measurement, one bounded field variant, then the curriculum decision.

1. **Last measurement (diagnostic only).** Evaluate the home field when no Curve path exists and report
   the home-field `Mixed`/`ClearSingle`/`StartTooStraight`. Do **not** change `WellMove` or add
   Mixed-path/run-band sampling in this step; those are probe features and the plan says no more probe
   features. If the home-field report already shows `Mixed StartTooStraight` + `ClearSingle`, go straight to
   the bounded variant.
2. **One bounded field variant (one morning).** Try a **6-planet Ring** (design allows 6–8; this changes the
   job contract from `PlanetCount = 8`, so update `PuzzleJobContracts`/`jobs.md`/the chapter if it wins),
   with one bounded blocker placement sweep. Keep `Zones = 2`, `MinRoutes = 2`, and "Mixed is the only
   route." `Zones = 3` is not authorized by the contract and is out of this plan.
3. **If the variant is negative, take the curriculum fallback.** Least invasive: job 4 becomes a multi-zone
   **Curve** (it still teaches "several zones") and job 8 becomes the first Mixed. Other options: ship seven
   jobs or substitute a Mixed job.
4. **Do not silently relax "only route."** If a pure Clear alternate is acceptable, that is a deliberate
   `jobs.md` change to "Mixed is the cheapest route"; it skips the job-4 lesson and needs a design call.

**Options**

| # | Option | Cost | Risk |
|---|---|---|---|
| A | Last measurement on the current 8-planet field | small | may only confirm the field is negative |
| B | A + 6-planet Ring / bounded blocker placement (contract `PlanetCount` change) | one morning | may still hit `ClearSingle`/`StartTooStraight` |
| C | Job 4 becomes multi-zone Curve; job 8 is the first Mixed | content + chapter change | job 4 loses the Mixed lesson; job 8 carries it |
| D | Ship seven jobs or substitute a Mixed job | content change | the Ring act loses its Mixed beat |
| E | Relax "only route" to "Mixed cheapest" | design change | pure Clear escape skips the lesson |

**Decision:** A+B is negative; take C (or D if schedule wins). E is rejected. Time-box to one morning; no
more seed sweeps and no more probe features.

**Decision rule:** if the home-field report shows `Mixed StartTooStraight` and no blocker placement can
remove `ClearSingle`/`StartTooStraight`, the field/contract is the problem, not the probe. Then pick C or D.
