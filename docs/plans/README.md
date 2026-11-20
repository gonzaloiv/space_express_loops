# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Road to the store

**Target 2026-12-06, €4, 40–45 minutes** (set 2026-11-18): ~21 story jobs across chapters 1–3, then
20 freelance contracts. Scope, content and schedule live in [release](../design/release.md); the game
shape in [chapters](../design/chapters.md). Out of this release: the daily contract and leaderboards,
freelance tiers and scores, resize and rotate (chapter 4, the first update).

## Now

Active work.

- [Fleet sun answer](in-progress/fleet-sun-answer.md) — space shaping: fleet fields bake with the sun
  off the ships' corridor or no sun; 2-1 keeps its sun.

Past post-mortems are in [lessons](../lessons.md).

## Next

The store release ([release](../design/release.md)), in schedule order:

- **18–22 Nov:** [fleet rebake](backlog/fleet-rebake.md) · [story arc](backlog/story-arc.md) ·
  [entitlement check](backlog/entitlement-check.md) · [store page](backlog/store-page.md) (find Meta's
  review time first)
- **23–29 Nov:** [chapter 2 to eight jobs](backlog/chapter-2-to-eight.md) ·
  [multi-zone fleet bake](backlog/multi-zone-fleet-bake.md) · [chapter 3](backlog/chapter-3.md)
- **30 Nov–4 Dec:** [freelance](backlog/freelance.md) · [release build](backlog/release-build.md)
  (timing pass, build, submit)

## Later

Design-only or deferred. Nothing here is scheduled.

- [Boot on one line](backlog/boot-one-line.md) — the console boot types through one localized line
  instead of 13 per-line GameObjects (after the store update).

- The contract card stays available during the run as a static reference; otherwise
  help reaches the player at the start (Jim's line, the gesture prompts) and the end (the complete/fail
  message) — no reactive in-solve hint surface.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion (deferred), feel polish, playable map field.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Job 7's manual shortening (from run fixes).** Waits on the lore rethink: does the "eleven years"
  note move or stay. Likely settled by [story arc](backlog/story-arc.md).
- **Field wells: kept (2026-11-11, headset).** The change-only wells are on in `Puzzle.prefab`
  (committed). Left to do: describe them in [the cube](../design/systems/cube.md) §Feedback (the lattice
  starts flat; a grown planet's well opens, a shrunk one flattens) and decide whether `LatticeWells`
  keeps its toggle as a tuning block. The lattice still runs 48 points a line, uncommitted
  (`LatticeMeshBuilder.SamplesPerLine`); with the line smoothing in, it can go back to 32.
- **Endpoint rings (from endpoint shapes).** Second try (2026-11-11): a small ring *under* the planet
  (a `Base` child at -0.7 body radius, planet-sized, not spinning) instead of around it, and the
  endpoint bodies spin like every planet again (`ConstantRotation`). Third pass: one ring for every
  endpoint, smaller (the filled band at ×0.7, 0.21–0.35 of the body radius), so origin and destination
  no longer differ by shape — only the route colour and the line's direction tell them apart, and the
  interchange keeps its markers. `OpenRing.asset` is now unused. Update
  [the cube](../design/systems/cube.md) §Feedback ("filled/open ring around the planet") once settled.
- **The ending on the headset.** The exit cue and the lit-map cue start almost together; keep one if
  they clash. The ending moves to chapter 3 ([story arc](backlog/story-arc.md)).
- **Route end fallback.** `MapRouteLine.EndIndex` returns `-1` when no job is `Current` or solved
  (unreachable today); make it safe if that can ever happen.
- **Gesture prompt icons — read well on the headset (2026-11-18).** Rendered by
  `Tools/Our Space/Render Gesture Prompt Art` (`GesturePrompts/Editor`); re-run it after tuning a motion.
- **Job 1 on the headset.** Rebaked 2026-11-06 (six planets, a sun the cube does not cover, seed 141);
  it still needs a headset read. The job set's bake and shipped-asset tests are green (2026-11-20).
- **Ship-zone cue.** The ship has a trail; there is no zone indicator. No plan owns it yet; multi-zone
  fleets in [chapter 3](backlog/chapter-3.md) may need it.
- **Resize.** Working, but the explanation (size is scope, not precision) is not ready. Chapter 4's
  first lesson, the first update ([chapters](../design/chapters.md)).
- **Mixed.** Owned by [chapter 3](backlog/chapter-3.md) U3; it falls to the first update if it does
  not bake in time.

## Conventions

- Every plan starts with a status line:
  `**Status:** <state> · <area> · updated <yyyy-mm-dd> — <one-line summary>`
  where `<state>` is one of `now`, `next`, `later`.
- `<area>` is one of `space shaping`, `universe`, `presentation`, `flow`.
- Folders: `now` → `in-progress/`; `next` and `later` → `backlog/`. Move the file and update this
  board in the same change.
- **Done plans are removed.** When a plan lands, add its post-mortem to [lessons](../lessons.md),
  then delete its file and its board row; the code and tests are the record. Do not archive it in a
  `done/` folder.
- Run `docs/check.ps1` after moving files — it runs `check-plans.ps1` (links, status/folder, board) and
  `check-docs.ps1` (design docs).
