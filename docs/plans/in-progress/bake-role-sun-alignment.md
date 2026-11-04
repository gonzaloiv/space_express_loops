# Bake — role and sun alignment

**Status:** now · space shaping · updated 2026-11-04 — S1/S2 landed; job 1 bakes with four
planets (seed 37); S3 is blocked on jobs 2/3 returning `NoRun`.

**Related:** [jobs](../../design/jobs.md) · [progression](../../design/systems/progression.md) ·
[core loop](../../design/core-loop.md) · [the cube](../../design/systems/cube.md)

**Scope:** `PuzzleJobContract`/`PuzzleJobContracts` is the job seed spec, and `PuzzleJobValidator` is an
editor-time acceptance gate — not a separate plan and not a runtime system. Fold its checks into
`PuzzleBrief`/`PuzzleFieldBake` as S1–S3 land; keep the validator test-only.

**Current state (2026-11-04):** S1 (`Matches(Any, Other)` is false, role costs logged) and S2
(`PuzzleRunRequest`, planet-blocker/sun-chord preference, `PuzzleRunChoiceTests`) are in, along with the
sun/start rule, contract fields, paths, log, test-only validator, and loose-fallback removal. The focused
`PuzzleRunChoiceTests`/`PuzzleSolutionClassifierTests`/`PuzzleBakeTests`/`PuzzleSearchTests` pass 29/29.
The explicit `PuzzleJobBakeTests` run is 1/4: job 1 passes (four planets, seed 37, one route); jobs 2/3
return `NoRun`; jobs 4–8 are not reached by `JobSet`. The `Validate_MovableStartFrame_Fails` assertion is
fixed. S3 is the field/run authoring work; the re-bake (S4) is not done.

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
  returns `NoRun` under the planet-blocker/sun-off-chord rule, so the field/run authoring remains S3.
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
- **Job 2/3 still no run.** Four-, five-, and six-planet probes (up to 200 attempts) return `NoRun`
  under the planet-blocker/sun-chord constraints; more attempts do not help. S3 needs a
  run-choice/field-authoring pass before the re-bake.
- **Exit:** a four-planet `Ring` tutorial bakes its requested role and a crash; jobs 3/4/7/8 offer two
  routes; job 5 is no longer a loose one-route bake; no fixed-level solved zone sits on the sun except
  job 2.

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
- The two-ship fixture is marked `[Explicit]` (multi-ship is out of scope); keep it out of the ordinary
  suite so S4 can close on the single-ship jobs.
- **Exit:** eight assets, no duplicates/fallback warnings, chapter references the eight updated assets,
  log shows 1 `Curve`, 2 `Curve`, 3 `Clear`, 4 `Mixed`, 5 `Curve`, 6 `Curve`, 7 `Curve`, 8 `Mixed`,
  routes ≥ 2 on 3/4/5/7/8, and the Creator bake tests are green.

## 3. Tests

- The classifier decision for an off-chord shrink that fixes a miss is explicit and covered.
- A requested role bakes only when its route is the cheapest; the role solution and both costs are
  captured in the search/log (the status enum has no cost fields).
- A `Curve` sun job selects a planet-blocked run; job 2's zone is centred on the star and grown.
- Job 2 is `CentredGrown`; job 8 is `InZone`; jobs 3/4/5/6/7 are `OutOfZone` and have no solved zone on
  the sun. Jobs 5/8 start at the field centre; jobs 6/7 start at the run midpoint.
- A four-planet `Ring` tutorial bakes a crash; a one-route field fails `minRoutes: 2` for jobs 3/4/5/7/8.
- The explicit job-bake tests use the production settings asset and are run deliberately.
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
- The scale ladder and ghost ([space-shaping-scale-ghost](../backlog/space-shaping-scale-ghost.md)).

## 5. Risks

- **`Other` is a real route.** If it is legitimate, excluding it may hide a valid solution; decide with
  a test, not by moving the role check.
- **Sun as a well.** If no seed makes the star's growth the cheapest `Curve`, the centre job needs a
  field-authoring pass; record it instead of falling back.
- **Sparse fields.** A sparse-field `NoRun` may be a run-choice constraint; fix the constraint, not the
  count.
- **Job 1 role fix.** `Clear` was cheaper than `Curve` on the 3-planet field; job 1 is now four planets so
  it has a non-blocker well.
- **Paths.** Moving the exporter without moving the chapter references orphans the assets; update the
  chapter GUIDs in place.
- **Test set.** The loose fallback is gone; the builder only ships strict bakes.
- **Second truth.** A `PuzzleJobValidator` that rechecks the brief will drift from the creator; keep the
  checks in `PuzzleFieldBake` and keep the validator test-only, if it survives at all.
