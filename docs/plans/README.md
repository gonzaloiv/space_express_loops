# Plans — the board

What is being built: **Now → Next → Later**. Each plan's `**Status:**` line matches its row here.
Store release: €4, 6 Dec, ~23 story jobs + 20 freelance; scope and schedule in
[release](../design/release.md). Post-mortems in [lessons](../lessons.md).

## Now

- **25–26 Nov:** [fleet rebake](in-progress/fleet-rebake.md) · [story arc](in-progress/story-arc.md)
  (clips, a lighter job card)

## Next

- **27–30 Nov:** chapter 2 to ten jobs: [intro](backlog/chapter-2-intro.md) → [fleet Clear](backlog/fleet-clear.md) · [capstone](backlog/fleet-capstone.md) → [lines and wire](backlog/chapter-2-lines.md)
- **1–3 Dec:** [chapter 3](backlog/chapter-3.md) (anchored bodies, repulsors and the ending)
- **3–5 Dec:** [freelance](backlog/freelance.md) + timing pass, build, upload

## Later

- [Boot on one line](backlog/boot-one-line.md) — only if there is time.
- Ideas (not plans): [docs/ideas](../ideas/README.md).

## Open tails

One line each: what is left and where it lands.

- **Field wells** (on in `Puzzle.prefab`): describe them in [the cube](../design/systems/cube.md)
  §Feedback; decide whether `LatticeWells` keeps its toggle; `LatticeMeshBuilder.SamplesPerLine` back
  from 48 to 32 (uncommitted).
- **Endpoint rings** (one small ring under every endpoint; `OpenRing.asset` unused): update
  [the cube](../design/systems/cube.md) §Feedback once settled; delete `OpenRing.asset`.
- **Route end fallback:** make `MapRouteLine.EndIndex` safe when it returns `-1` (unreachable today).
- **Gesture prompt icons:** re-run `Tools/Our Space/Render Gesture Prompt Art` after tuning a motion.
- **Job 1 on the headset:** rebaked 2026-11-06 (seed 141), still needs a headset read.

## Conventions

- Status line: `**Status:** <now|next|later> · <space shaping|universe|presentation|flow> · updated
  <yyyy-mm-dd> — <summary>`.
- Folders: `now` → `in-progress/`; `next`/`later` → `backlog/`. **Only the user closes a plan.**
  When they say to close it, follow [AGENTS](../../AGENTS.md) (post-mortem in lessons, then delete).
  Run `docs/check.ps1` after moving files.
