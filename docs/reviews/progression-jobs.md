# Review — progression and jobs (design + build state)

**Date:** 2026-10-28.
**Scope:** [progression](../design/systems/progression.md), [jobs](../design/jobs.md),
[core loop](../design/core-loop.md), [resources](../design/systems/resources.md),
[cube](../design/systems/cube.md), and the progression plan
against the code (`Assets/_Game/Features/SpaceShaping`, `Assets/_Game/Features/PuzzleCreator`,
`Assets/_Modules/Casual`) and the saved assets/scenes.
**Verdict:** the design is viable for a **3–5 job slice (jobs 1–4)** — the bake, session, readout and the
existing cookie/selector plumbing support it. It is **not yet viable as the 1–8 run**: the resize lesson
has no bake or gesture contract, and the progression/content layer is unbuilt.

## Status — what is done

| Area | State | Evidence |
|---|---|---|
| Scale, physics, session | **Done** | `SpaceShapingSession`, `TrajectoryIntegrator`; determinism/solve/rescale tests |
| Entropy, fail, star formula | **Done** | `SpaceShapingSession.Grade`, `IsFailed`; `PuzzleGrade`; readout tests |
| Readout + lab panel | **Done** | `PuzzleReadout`, `ReadoutChanged`, `PuzzleLabPanel` |
| Multi-zone search + bake (M1–M2) | **Done** | `PuzzleBeamSearch`, `PuzzleFieldBake`, 39/39 creator tests |
| Move gesture + condition locks | **Done** | `ZoneConditions`, `ZoneGrabListener.Move`, `SpaceShapingState` |
| Resize gesture | **Partial — not teachable** | see finding 1 |
| Rotate | **Not started** (deferred) | `PuzzleBrief.SupportsMoves` rejects it |
| 2-zone POC proof (M3) | **Not proven** | see finding 5 |
| Jobs 1–2 (scale) | **Not authored** | no baked `PuzzleData` asset |
| Job 3 (move + several zones) | **Bake path ready; not authored** | beam search + `FewerZones` guard |
| Job 4 (time tutorial) | **Bake path ready; not authored** | `PuzzleClockSearch`, `PuzzleClockBakeTests` |
| Jobs 5–8 (resize / combine) | **Blocked** | see finding 1 |
| Campaign map, cubes, numbering | **Not started** | no `CampaignView`, no `CampaignState` |
| Progression save P1–P3 | **Not started** | see finding 5 |
| Job cards, TTS, localization | **Not started** | see finding 4 |
| HUD, result/fail panel | **Not started** (lab only) | `PuzzleLabPanel` is the temporary surface |
| Legacy cut | **Not started** | `GameStartState`/`GameSnapshot` still boot |
| Quest 3S / 72 fps pass | **Not started** | no device capture |

## Findings

1. **Job 5's resize is not a taught mechanic.** The briefs mark job 5 and jobs 6–8 `resizable`
   ([jobs.md:23–24](../design/jobs.md#L23)), and the design says the aperture is the player's
   ([progression.md:66–67](../design/systems/progression.md#L66)). But the bake starts the frame at the
   **solution** size (`PuzzleFieldBake.cs:147–154`) and the search uses one size per chain
    (the multi-zone plan §2), so the found solution never needs
   the gesture. The runtime only **grows** the frame: `ResizeFactor` clamps to `[1, maxGrow]`
   (`ZoneGesture.cs:39–40`) with `frameGrowRatio = 2` (`ZoneGrabListener.cs:19`); job 5's "bigger, smaller"
   ([jobs.md:44](../design/jobs.md#L44)) is not true of the saved pose. The verifier only checks shared size
   when `!resizable` (`PuzzleVerifier.cs:53–54`), so a single-size solution passes a resizable brief.
   **Fix:** add a `startCubeSize` (bake the start frame larger/smaller than the solution) and a
   `ResizeRequired` rejection mirroring `FewerZones`, or explicitly cut jobs 5–8 from the slice until
   multi-size chains are supported. Decide the shrink range in the same change.

2. **The entropy dial is derived, not a dial.** `progression.md:131` lists "Entropy budget — loose → tight"
   as a variation axis, and the job table has an "entropy" dial ([jobs.md:18–25](../design/jobs.md#L18)).
   The budget is always `solution cost × 3` (`PuzzleGrade.BudgetFactor = 3`, `PuzzleFieldBake.cs:134`;
   [resources.md:94–96](../design/systems/resources.md#L94)), and the seed exposes no multiplier
   (`PuzzleSeed` has window/start/moves only). Every job therefore has the same grade pressure.
   **Fix:** either rename the column to "solution cost" (an outcome, not a knob) or add a per-job
   `budgetFactor` to the seed/bake and read it at runtime.

3. **The 3-star gate is anchored to the found cost, not a proven optimum.** Three stars requires
   `spent ≤ budget / 3` (`PuzzleGrade.cs:7–16`), and the budget is `3 × found cost`, so the 3-star boundary
   is exactly the bake's solution. A 1% overshoot drops to 2 stars, while a cheaper solution the search did
   not find would earn 3 stars. The bake rejects a shorter *zone* solution (`FewerZones`) but does not prove
   the found cost is minimal. **Fix:** either prove/seek the minimum cost, or widen the 3-star band (for
   example `≤ 1.25 × found cost`) and test the boundary. Record the choice in `resources` tuning.

4. **The job card has two entries but one key.** `jobs.md:54` says the manual entry and the tech-voice line
   are "two entries keyed from the job's id (`LevelData.IntroKey`)". `LevelData.IntroKey` is a single key,
   `level_{id}_init` (`LevelData.cs:16`), and `TTSHelper.Say` takes one key (`TTSHelper.cs:30`). No job keys
   exist in the locale assets. **Fix:** add two explicit keys (manual/tech) to `LevelData` or a job-card
   asset, and author both for jobs 1–4 before the map/tutorial slice.

5. **The POC proof is not reproducible, and no baked job exists.** The saved POC has an empty array
   (`SpaceShapingPOC.unity:45037` — `puzzles: []`) while `SpaceShapingState.Enter` indexes `puzzles[0]`
   (`SpaceShapingState.cs:9,36–37`); no asset in the project matches the `PuzzleData` script GUID, and no
   `ChapterData`/`LevelSelector` asset exists. `Game.unity` has no puzzle wiring either, so none of this is
   reachable from the game yet (the in-game plan §1). The multizone
   review's "wired" claim ([multizone.md:40–42](multizone.md#L40)) is stale. The seed assets changed at 18:42 today, so this may be
   in-flight, but the saved scene cannot boot the 2-zone puzzle as it stands.
   **Fix:** bake one asset, wire it (or switch to the single `PuzzleData` field the in-game plan already
   chose, the in-game plan §1), and run M3 on the headset.

6. **`offPath` is a real brief condition missing from the brief.** `PuzzleBrief.OffPath` exists and rejects
   multi-zone off-path bakes (`PuzzleBrief.cs:16–19`), and job 4 depends on it
   ([jobs.md:22](../design/jobs.md#L22)), but the brief contract lists only `translatable`, `moves`,
   `resizable`, `rotate`, `scalable` ([progression.md:80–90](../design/systems/progression.md#L80)).
   **Fix:** document it as a creator-only condition with the invariant `offPath ⇒ moves = 1`, or add it to
   the brief table.

7. **The verifier's zone-count check is tautological.** `Variant` sets `puzzle.moves = solution.Zones.Length`
   (`PuzzleFieldBake.cs:152`) before `PuzzleVerifier.Fits` checks `solution.Length != puzzle.moves`
   (`PuzzleVerifier.cs:40`), so the check can never fail and does not assert the **brief's** `Moves`.
   `FewerZones` covers only fewer zones at the same width. **Fix:** check against `brief.Moves` before
   `Variant` (or set `moves = brief.Moves` and verify), and add a test where the search returns the wrong
   count.

## Settled

- **Job 3 is translatable and the time tutorial is job 4.** The three docs and the code agree, and the search
  rejects fixed multi-zone (`jobs.md:27–29`, `progression.md:59–75`, `PuzzleBrief.SupportsMoves`).
- **One geometry for author and player.** The bake and verifier share `SpaceShapingSetup` / `PuzzleZoneRun`,
  so a found 2-zone solution reproduces through `SpaceShapingSession`.
- **Runtime does not count moves.** `moves` is authored metadata and the game never shows it
  (`cube.md:36`); this is consistent with the continuous-verb design.
- **Progression-only save is coherent.** Player progression (solved jobs + stars) is the right persistence
  boundary; the baked field makes a mid-level snapshot unnecessary
  ([progression.md:140–144](../design/systems/progression.md#L140)).
- **The plumbing exists.** `LevelCompleteCookie` + `LevelSelector` + `PlayerData` are the right reuse
  (`LevelCompleteCookie.cs`, `LevelSelector.cs`); the plan's P1–P3 is buildable without a new save schema.

## Open decisions

- **Resize contract:** add a start-size field and a required-resize guard, or cut jobs 5–8 from the slice.
- **Star threshold:** prove minimal cost, or widen the 3-star band; pick one before the first bake ships.
- **Two job-card keys:** manual and tech-voice need separate keys; `IntroKey` alone cannot carry both.
- **All-solved chapter:** `LevelSelector.GetFollowingLevel` returns the last completed level when everything
  is solved (`LevelSelector.cs:15–22`); decide replay-last vs chapter-complete (and `null`).
- **Bare cookie metadata:** `LevelCompleteCookie.Metadata` deserializes without a default
  (`LevelCompleteCookie.cs:18–21`), so a cookie written without metadata throws on `HasBeenCompleted`;
  P1 should use `LevelCompleteCookieMetadata.Parse` and test the empty-metadata round trip.

## Applied

- 2026-10-28 — review written; no code or design docs changed.
