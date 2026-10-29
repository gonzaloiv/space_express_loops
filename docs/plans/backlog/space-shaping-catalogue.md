# Space Shaping — Catalogue + Acceptance

**Status:** next · space shaping · updated 2026-10-28 — author 3–5 baked jobs, then 8–10 as stretch, then the
Quest 3S acceptance pass. Content work, not engine work; gated on validation 2 and the
[multi-zone](space-shaping-multizone.md) bakes.

**Related:** [multi-zone](space-shaping-multizone.md) · [in the game](../in-progress/space-shaping-in-game.md) ·
[campaign map](space-shaping-campaign-map.md) · [jobs](../../design/jobs.md) ·
[progression](../../design/systems/progression.md) · [variations](../../ideas/variations.md) ·
[cube](../../design/systems/cube.md)

## 1. Context

Integration consumes **baked** `PuzzleData`; there is no runtime generation. The creator bakes offline from a
seed + brief — fixed `sources` remain the hand-tuned fallback
([variations](../../ideas/variations.md) §3). The catalogue is one ordered `ChapterData` list; `PuzzleData.id`
uses the `XX_YY` shape the level cookies parse, and the map number is the global index + 1.

Dependency: `moves = 2+` jobs (job 3 and 6–8) need the [multi-zone](space-shaping-multizone.md) search. If a
job needs a multi-zone bake that does not exist yet, it moves to the stretch slice rather than shipping a
one-zone stand-in.

## 2. Slices

### K1 — First three jobs

- Bake and author three jobs in order: the first-scale job, the movable/several-zones job (job 3,
  translatable), and the time tutorial.
- Per job: brief per [jobs](../../design/jobs.md), the baked asset, **one authored cube position** on the
  map, the **manual entry** and **tech-voice line** localization keys (`LevelData.IntroKey`).
- **Exit:** three jobs are selectable in order, each with its card playing, each solvable through the
  session; the baked entropy budget matches the solution cost band.

### K2 — Full run (8–10, stretch)

- Extend to the design's run once the multi-zone bakes are available; keep the order and the ids stable.
- **Exit:** every job in the chapter is baked, ordered, star-bearing and solvable; no placeholder briefs.

### K3 — Acceptance

- Quest 3S: 72 fps capture across boot → map → pick → shape/fly/check in the same Bubble view → solve →
  back → reload.
- Legacy route/hub/round content absent; setup/splash untouched; reset behaves as specified in
  [progression](space-shaping-progression.md).
- Tune `travelSecs` on device; run the SFX pass (travel, scale, grade) **or explicitly cut it**.
- Feel polish stays in [feel polish](../../ideas/feel-polish.md) and is not duplicated here.
- **Exit:** acceptance recorded with the frame capture and the tuned values; open tuning items named.

## 3. Tests

- Every baked job loads into the session and verifies against its brief (zone count, sizes, envelope,
  budget).
- Cookie ids parse for every job; map numbering matches the order.
- A job's card keys resolve in every shipped locale.

## 4. Out of scope

- The multi-zone search itself — [multi-zone](space-shaping-multizone.md).
- New mechanics (rotate, `moves > 3`).
- Acts/chapters split beyond chapter `01`; streaming and galaxies.
- Final art and audio direction.
