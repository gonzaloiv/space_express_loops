# Reviews — playbook

**Purpose:** a review is dated and disposable; the lesson is not. This page is the common source for both:
the checklist a review runs, and the findings this project keeps producing. Read it before a review; add to
it after one.

**Coverage:** the catalogue is mined from the whole history (2026-04 → 2026-10, 146 commits), not just the
latest slice. The April–June block is the previous loop prototype; its lessons are the repeat-fix and
restore-path entries. Where a review existed, it is cited by date; otherwise the commit is the evidence.

A review file lives beside the plan it audits and is deleted when that plan lands — the code and tests are
the record. Before deleting it, promote any **new class** of finding into
[Recurring findings](#recurring-findings), and fix the inbound links. Do not archive reviews.

The project rules in [`AGENTS.md`](../../AGENTS.md) are the standard; this page is how a review checks them.

## How to run a review

1. **Read first.** The plan, the design docs it cites ([vision](../design/vision.md),
   [cube](../design/systems/cube.md), [progression](../design/systems/progression.md),
   [jobs](../design/jobs.md)), and the code at `HEAD` — not the plan's summary of the code.
2. **Two passes.** Contract first (is it right?), then hygiene (is it reviewable?). Findings from the
   contract pass outrank nits.
3. **Evidence, not adjectives.** Every finding cites `file:line` or a commit. If you cannot point at it,
   it is a question, not a finding.
4. **Rank.** Silent wrongness (a player cannot reproduce the verified solution) first; size limits and
   naming last.
5. **Separate settled from open.** Say what the code already decided, then what still needs a decision.
6. **Apply.** Record what the review changed in the plan or code, dated. Then promote the generalizable
   lesson here before deleting the review.

## Checklist

**Plan**

1. Can one agent land this in a morning, with an exit per slice?
2. Was every "open question" checked against the design docs before being asked?
3. Is the plan stated against the code at `HEAD` (no stale file names, no done work listed as next)?
4. Is missing scope called out — flow/mode contract, persistence, performance, device pass?

**Contract**

5. Does every authored value come from the one settings asset the runtime reads?
6. Does the bake use the same zone geometry, falloff, capture radius and step budget the player gets?
7. Can the player reproduce the verified solution exactly, with the same numbers?
8. Do spawn and restore go through the same init path, and is the load-from-save path tested?
9. Does the runtime asset carry only runtime fields — no authoring metadata or solution arrays?

**Model**

10. Is the physics the actual equation at toy scale, with a formula test — not a tuned clamp?
11. Is the domain rule expressed in a type/status, not in an `if` at the call site?
12. Is each invariant checked after the step that can change it (solve, then re-check the start)?

**Tests**

13. Does each test pin its input (a known seed, an explicit window) instead of scanning for one?
14. Does each assertion test a behaviour, not restate the value the code just produced?
15. Is the failing path tested — wrong window, no budget, crash vs miss, outside the zone?
16. Is the restore/resume case covered, not only the fresh start?
17. Would the assertion still pass if the feature were removed?

**Boundaries**

18. Is the code in the feature/asmdef that owns it, and is each type and method under the size limits?
19. Does a type answer one question, or is it doing search + geometry + simulation in one class?
20. Is this the nth patch in the same area? If so, the fix is a unification, not another patch.

**Assets and data**

21. Does the POC spawn the production prefab/view rather than its own copy?
22. Do re-baked assets keep their GUIDs (load-and-overwrite, not delete-and-create)?

**Platform and device**

23. Does it change a build-defining setting, and was the result measured with a fresh device build?
24. Does the exit include a device pass — sleep/wake, unmount/remount, resume?

**Hygiene**

25. Is the diff reviewable (no generated scene/meta churn dominating it)?
26. Is the commit one slice, with plan moves and docs updated in the same change?
27. Do the tests and `docs/check.ps1` pass?

## Recurring findings

Grouped by the pass that catches them. Each entry is a failure mode, its tell, and the fix that worked.
Entries are unnumbered: add a new class where it belongs; add evidence to an existing one.

### Plan

**The plan is a project, not a morning.**
*Tell:* a dozen slices, weeks of work, or a review that says "this is a project".
*Fix:* split at the gate; each plan lands in one sitting with an exit per slice. Move the overflow to its
own plan instead of growing the board.
*Seen:* integration review (2026-10-27) §4; the creator plan grew across `9489959`–`2d86ac1`.

**The plan is stale against `HEAD`.**
*Tell:* file names, field names or "still to do" items that the code no longer matches.
*Fix:* review the code, not the plan's account of it; update the plan in the same change that lands the
work.
*Seen:* creator review (2026-10-27) found six stale lines; integration review found five. `fb80e70` still
claimed the 2-zone POC asset was wired while the same commit cleared `SpaceShapingState.puzzles`; fixed
2026-10-28.

**Settled questions are re-opened.**
*Tell:* an "open question" already answered by vision, variations or progression.
*Fix:* search the design docs first; record the decision with its evidence, then keep only the genuinely
open questions.
*Seen:* integration review §1 (`variations`, `progression`, `vision`). `fb80e70` added two-fixed-cube
levels after `68e9b31` had settled `moves ≥ 2 ⟹ translatable`; removed 2026-10-28.

**Only the wrong things are in scope.**
*Tell:* no flow/mode contract, no save policy, no performance budget, no device pass.
*Fix:* the plan names those seams even when the slice does not build them.
*Seen:* integration review §3.

### Contract

**Authoring geometry differs from runtime geometry.**
*Tell:* "the search found a solution" but the same numbers do not reproduce in the POC.
*Fix:* bake with the view's region — half-extents, `zoneFalloff`, `contentInset`, capture radius and step
budget — and keep one owner for each value. `size` was renamed to `halfExtents` for this reason.
*Seen:* creator review (2026-10-27) item 8; fixed in `2d86ac1` and `f4de6e2`.

**The same constant lives twice.**
*Tell:* a sim/settings field copied into a second struct, or tuning split between creator and game.
*Fix:* pass one snapshot (`PuzzleSimSettings`, `ZoneConditions`) or read the one settings asset. Do not
mirror fields "for convenience".
*Seen:* `SpaceShapingSetup` duplicating the sim; fixed to hold `Sim`.

**Spawn and restore take different paths.**
*Tell:* a panel, handler or state machine is right on a fresh start and wrong after loading a save;
restored entities skip the init the spawner does.
*Fix:* one `Init`/`BeginIdle` path used by both; the review asks for the load-from-snapshot case, not just
the first boot.
*Seen:* `68f17ac` (planet availability on load), `96b0b31` (grab panel on load), `f61f516` (grab state on
relaunch), May–June 2026.

### Model

**A fudge factor stands in for the equation.**
*Tell:* `clamp(1 + scale * potential)`, a "strength" that is not the physical one, a magic margin.
*Fix:* write the two-term form at toy scale (`sqrt(1 + 2phi/c^2 - v^2/c^2)`, `mass = r^3`), keep `c` in
settings, and test the formula itself.
*Seen:* `dd5fdc9`; tests `Clock_MatchesTheTwoTermForm`, `Mass_ScalesWithRadiusCubed`.

**The rule lives at the call site.**
*Tell:* a status called `StartNotMiss` while the design says "fail, not crash"; the caller compensating.
*Fix:* name the rule in the type (`StartNotFailed`, computed by the verifier) and let call sites read it.
*Seen:* `dd5fdc9`; `PuzzleVerifier.StartFailed`.

**An invariant is checked before the step that changes it.**
*Tell:* "start fails" checked before the solution is applied, then never re-checked.
*Fix:* derive the window, solve, re-check the start on the final field. Bake order is part of the contract.
*Seen:* `0a6b438`; `PuzzleFieldBake.Bake`.

### Tests

**The test derives its expectation from the code.**
*Tell:* the test sets `tmin`/`tmax` from the `ShipTime` it just found; the assertion cannot fail.
*Fix:* fix the input window, then assert the outcome; add a wrong-window case that must reject.
*Seen:* creator review item 2; fixed in `b512667`.

**Seed scans hide tuning coupling.**
*Tell:* `for seed = 1..32` and "no seed produced a field" as the failure message.
*Fix:* pin a known-good seed; assert the bake status and the contract, not the generator's tuning.
*Seen:* `b512667` replaced the scans in the bake and search suites.

**The negative case is missing.**
*Tell:* only the happy path is covered; crash, miss, outside-window and zero-budget are untested.
*Fix:* one test per rejected path, named after the rule.
*Seen:* `PuzzleSearchTests` gained window, budget, crash and miss cases.

**The assertion is always true.**
*Tell:* `assert radius.max < runDistance` where the values can never meet.
*Fix:* assert the meaningful extent (`radius.max + planetRadius.max` inside the run) or delete the test.
*Seen:* creator review "test hygiene"; fixed in `b512667`.

**The commit leaves the suite pinning the old rule.**
*Tell:* code flips a domain rule, the tests still assert the previous one, and the corrected assertions sit
uncommitted — so `HEAD` is red while the branch is dirty.
*Fix:* run the suite before the commit and update the assertion in the same change; a rule change and its
test are one slice.
*Seen:* `fb80e70` (`PuzzleBrief.SupportsMoves` vs `PuzzleMultiZoneTests` / `PuzzleBakeTests`); fixed in
the 2026-10-28 working tree.

**Shared mutable fixtures make tests order-dependent.**
*Tell:* a puzzle built in `OneTimeSetUp` and mutated by several tests.
*Fix:* build per test; pin the seed inside the fixture; destroy the asset in the same test.
*Seen:* creator review "test hygiene"; fixed in `b512667`.

**Only the fresh-start path is tested.**
*Tell:* green tests, but the bug only appears after a reload; the restore path has no case.
*Fix:* load from a snapshot in the test, then assert the same invariants as a fresh start.
*Seen:* `68f17ac`, `96b0b31`, `f61f516` (May–June 2026).

### Boundaries

**A subfolder with its own asmdef and tests is a feature.**
*Tell:* `SpaceShaping/Scripts/Creator` owning data, domain, editor and tests.
*Fix:* promote it (`PuzzleCreator`) with its own asmdef, namespaces and test assembly.
*Seen:* `996b034`.

**One class answers several questions.**
*Tell:* `PuzzleSolutionSearch` doing window, centres, zones and simulation; a `Bake` method over 25 lines.
*Fix:* split by question — `PuzzleWindow`, `PuzzleZoneCandidates`, `PuzzleZoneRun`, `PuzzleClockSearch` —
and give `Bake` one small step per method.
*Seen:* `01493a1`, `0a6b438`.

**The same patch lands N times.**
*Tell:* the log reads "Fix grabbable" six times, each fixing a symptom in a different place.
*Fix:* stop patching; give the interaction one owner (`PlanetUniverseHandle`), then fix once. The nth fix
is a design signal, not a bug.
*Seen:* `ccc28f6`–`96b0b31` (2026-04 → 05), unified in `ecbe16f` (2026-10-20).

**The method outgrows the limit as the slice lands.**
*Tell:* a new `Render`, `Search`, `Update` or gesture method at 30–47 lines, usually a whole pipeline in
one body.
*Fix:* one step per method (`BuildVertices`, `Evaluate`, `ApplyFeedback`); the size limit is the signal
that a second question arrived.
*Seen:* `68e9b31`–`515e07c`: `SpacetimeMeshView.Render` (47), `FixedZoneGesture.Track` (38, since deleted),
`PuzzleFixedSearch.Search` (34, since deleted), `PuzzleSolutionSearch.TryBestSingleAtWidth` (32),
`TrajectoryIntegrator.Integrate` (28).

### Assets and data

**The POC invents its own view.**
*Tell:* a POC-only prefab/view that duplicates the game's planet.
*Fix:* spawn the production prefab (`FieldPlanet`) and pool it; delete the POC copy so integration is a
rewire.
*Seen:* `01493a1`; integration review "Applied 2026-10-28".

**Authoring metadata ships in the player.**
*Tell:* `PuzzleData.solution` in the runtime asset.
*Fix:* return the solution from the bake as an out-param; keep it in the creator artifact only.
*Seen:* `b512667`.

**Re-baking churns GUIDs.**
*Tell:* `DeleteAsset` + `CreateAsset` on every export; references break silently.
*Fix:* `LoadAssetAtPath` + `EditorUtility.CopySerialized`; create only when absent.
*Seen:* creator review "Improvements"; fixed in `PuzzleArtifactExporter.Write`.

**A code commit empties a serialized reference.**
*Tell:* `puzzles: []` in the scene, an empty list where the scene used to point at assets; it compiles and
throws on `Enter`/`Start`, so nothing catches it until Play mode.
*Fix:* re-assign the scene refs in the same change that renames or removes a serialized field, and
smoke-test Play mode before committing. The missing reference is a broken scene, not a case for a null
check.
*Seen:* `fb80e70` (`SpaceShapingPOC.unity` `puzzles: []` after `2fc1172` had wired it); restored
2026-10-28.

### Platform and device

**A build-defining setting is part of the change.**
*Tell:* the fix is a Project Settings asset (graphics API, XR loader) with no code; or a device-only
regression survives every rendering tweak.
*Fix:* name the setting and why, verify with a fresh build over at least three wake cycles, and record the
cost of the workaround (GLES3 is legacy; foveation is inert). Keep the measurement, not just the switch.
*Seen:* `eea2a35` (Vulkan → GLES3 for the HMD resume leak; the note
`docs/solutions/vulkan-hmd-resume-gpu-memory-leak.md` lived two days and was deleted in `f65b806` — recover
it with `git show eea2a35:docs/solutions/vulkan-hmd-resume-gpu-memory-leak.md`); `8321ec0` (XR loader off
for editor play).

**A device-only failure is diagnosed on device.**
*Tell:* FPS collapse after headset sleep/wake, no managed exception, process death; "fixes" that only
shift the cycle count.
*Fix:* `dumpsys meminfo` and the exit-info record before touching rendering; three cycles; one change at a
time. An OS kill is not a crash.
*Seen:* Vulkan note (2026-10-21); `eea2a35`.

### Hygiene

**The diff is unreviewable.**
*Tell:* a 90k-line scene YAML, 84 files, or generated `.meta` churn dominating the change.
*Fix:* commit scene/prefab wiring separately from content; do moves in a commit that changes nothing else;
review by `--stat` and targeted hunks.
*Seen:* `8a2662d` (90,451 insertions, mostly `SpaceShapingPOC.unity`), `047a6dd` (84 files, tidy-up),
`5fb1bc3` (76 files).

**Docs taxonomy churns.**
*Tell:* a new `done/`, `solutions/`, or nested `backlog/gameplay|universe|presentation/` appears; every move
breaks links.
*Fix:* one board (`docs/plans/README.md`) and this playbook; delete done plans; never a parallel knowledge
folder that will itself be deleted.
*Seen:* `047a6dd` created `backlog/` and `done/`; `73c6489` deleted `done/`; `f65b806` deleted
`docs/solutions/`; `b512667` flattened `backlog/`. `fb80e70` moved `space-shaping-in-game.md` to
`in-progress/` without touching its 30 inbound links or its `next` status, leaving `docs/check.ps1` with
32 problems; moved back 2026-10-28.

**The commit mixes slices.**
*Tell:* one commit carrying plan work, several slices, scene edits and cleanup (30–84 files).
*Fix:* one slice per commit; docs moves in the same change as the code they describe.
*Seen:* `9489959` (P1/P2 + plan + scene cleanup); `996b034` (84 files); `68e9b31` (71 files: six-plan split
+ POC wiring + bakes); `2fc1172` (722-line scene plus a `link.xml` deletion later restored).

## Template

```markdown
# Review — <plan or scope>

**Date:** yyyy-mm-dd.
**Scope:** <plan> against the code (`Assets/_Game/...`) and the design docs.
**Verdict:** <one line: direction, then the blocking problem>.

## Findings

1. **<finding>** — <evidence `file:line`>. <fix>.

## Settled

- <decision> — <evidence>.

## Applied

- yyyy-mm-dd — <what changed in the plan or code>.
```

## Maintaining this page

- **One row per class.** If a review repeats a finding, add its evidence to the existing entry; do not add
  a second entry. Older evidence goes in the same `Seen` line with its date.
- **Short entries, long memory.** Keep tell/fix/seen; the commit holds the detail. When mining older
  history, add only a class that is not already here — the fifth commit about the same area is a signal,
  not a new row.
- **Permanent links only.** This page outlives reviews, so cite commits in text and link only to design
  docs and plans.
- **Delete when done.** When a plan lands, delete its review and its board row; the lesson stays here.
- Run `docs/check.ps1` after editing.
