# Chapter 2 bakes

**Status:** now · space shaping · updated 2026-11-26 — every chapter 2 scan and bake in one list, at
the edge sun, after [edge sun](edge-sun.md) U1 is in both bake checkouts.

**Bakes:** off the editor — every unit ([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor),
`docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [edge sun](edge-sun.md) ·
[fleet rebake](fleet-rebake.md) (contracts for 2-1 to 2-3) · [chapter 2 intro](chapter-2-intro.md)
(02_09, 02_10) · [fleet capstone](fleet-capstone.md) (02_05) · [fleet Clear](fleet-clear.md) (02_06–02_08)

**Progress (2026-11-25):** edge-sun U1 is committed (`ccd27b7c`) with its test fix (`edd8a79a`) and
cherry-picked into `bake` (`71fe462c`, `8347e802`) and `bake2` (`03a5ec50`, `7caa53ed`); the
placement tests pass 5/5. U2/U3 (edge-sun docs and map) are still uncommitted in `our-space` and do
not gate these bakes. No job rebaked yet.

## Goal

Nine chapter 2 jobs rebaked at the edge (02_01–02_03, 02_05–02_10) and in play order in
`Chapters/02.asset`; 02_04 stays centred and is not baked or validated here (it has no contract).
The owning plans keep their contracts and decisions; this plan owns the scans, the bakes and the order.

## Before any scan

- [Edge sun](edge-sun.md) U1 committed and merged into `bake` and `bake2`: the height-band `TryHome`
  and the validator's sun-position check. The contracts are **not** flipped there.
- Stop any scan still running at the centre (02_01, 02_02 started 13:39 on the old setup).
- One Unity per checkout. Never stage the deleted `Google Sheets Service.asset.meta` in a bake checkout;
  `git add` explicit paths only.
- Both checkouts edit `PuzzleJobContracts.cs` and `Chapters/02.asset`, so the U4 merge will conflict there:
  keep both sides' pins, re-run the shipped check, and set the chapter order once afterwards.

## Per job

Set the job's `SunPlacement = OffCorridor`, scan (`-Mode scan -Seeds <n>`, add
`-ProjectPath ..\our_space_bake2` for `bake2`), pin `Seed` from the scan (set `Attempts` to 1), keep the
job's other pins, `-Mode bake`. The scan report prints seed, window, scale delta and start distance but
not the sun's home or `SunOnChord`: read those from the baked asset (`PuzzleJobValidator.SunOnChord`) or
add them to `PuzzleBatchBake`'s report. The flip and the rebake land in one commit, so a job's commit
does not leave an edge contract on an old asset. (`bake` already flipped 02_05–02_10 in `06b3e0f8`, so
its shipped check stays red until each rebakes; run it at U4.) Before committing,
read the asset: the `centreOnly: 1` source sits level with the planets, not above or below them, and
outside their rim (a recentred centred sun can pass a y-only check). Commit only that job's asset, its
`.meta`, its contract pin and its `Chapters/02.asset` slot.

## Units

### U1 — the jobs that ship today (2-1 to 2-3)

- 02_01 (`bake2`) and 02_02 (`bake`), then 02_03 in whichever frees first. ~24 seeds each to start.
- Check after the first pair: if both come back empty, stop and report before spending seeds on the rest.
- Exit: three assets with an edge sun, each from a seed whose scan entry meets the contract. The full
  `ShippedJobs_MeetTheContract` runs at U4: it covers every enabled job, and 02_09/02_10 still carry
  above-band suns until their rebake.

### U2 — the intros and the capstone (02_09, 02_10, 02_05)

- 02_09 and 02_10 (fixed fleets; `MaxScaleDelta` 0.5 stays the headset item). Rescan both: the edge
  placement changes the run (no sun radius, sun may sit on the chord), so the fixed frame can change and
  the current seeds may not bake; re-check `MaxScaleDelta` against the new answer.
- 02_05: already baked at the edge (`33f624f7`); re-check it under the new sun check. Rebake only if
  it fails; if that rebake fails, take [fleet capstone](fleet-capstone.md) U2's fallback and record the
  gap. Its `MinScaleDelta = 0.35f` / `MinStartDistance = 0.15f` were dropped by the bake checkout's
  recentre commit: restore them from `our-space` before the bake (they are bake-time bounds; the shipped
  check skips them).
- Exit: each asset's scan entry meets the contract; 02_05 keeps its high-scramble bounds. The full
  `ShippedJobs_MeetTheContract` is U4 (it covers every enabled job).

### U3 — the Clear jobs (02_06–02_08)

- 02_06: rebake (`a4bad465` has the sun above the field). Seed 19 was the Clear seed; rescan if it no
  longer bakes. Clear `Pending` only once the seed and bounds are pinned ([fleet Clear](fleet-clear.md) U3);
  until then the shipped check skips the job.
- 02_07 and 02_08: ~200 seeds each, up to ~400 (Clear is filtered after the search; expect many
  `WrongRole`). If one has no bake by then, ship the ones that did and record the gap in
  [fleet Clear](fleet-clear.md) U3.
- Exit: every baked Clear job's scan entry meets the contract and none solves in place; the full
  `ShippedJobs_MeetTheContract` is U4.

### U4 — order, merge and tests

- Merge both bake branches into `our-space` (assets come back through git, never copied by hand). Expect
  conflicts in `PuzzleJobContracts.cs` and `Chapters/02.asset`; keep both sides' pins.
- Set `Chapters/02.asset` by hand once, after both merges: 02_09, 02_10, 02_01, 02_02, 02_03, 02_04,
  02_05, 02_06, 02_07, 02_08 (minus any Clear job that did not bake).
- Extend `FleetJobsTests` to what shipped (ids, ship counts, chapter count); run it and
  `ShippedJobs_MeetTheContract` in a checkout that has the merged assets (sync `our-space` into `bake`
  first; `bake2` lacks the Clear assets).
- Exit: both tests green; the board and each owning plan's progress note say what baked.

## Report

Per job: seed, window, scale delta, start distance, the `centreOnly` source's `home` (read from the
asset), `SunOnChord`, and any job that could not bake.
