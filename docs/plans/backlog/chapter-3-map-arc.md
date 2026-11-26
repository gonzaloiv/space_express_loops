# Map arc

**Status:** next · presentation · updated 2026-11-26 — the chapter systems sit on an arc around the
seat, so every job stays in reach as chapter 3 and freelance add systems. Before chapter 3.

**Related:** [plans board](../README.md) · [chapter 3](chapter-3-jobs.md) · [freelance track](freelance-track.md) ·
[the cube](../../design/systems/cube.md) (the map uses the same cube)

## Goal

`PuzzleMapLayout.Systems` lines the chapter systems up side by side along `x` (`PuzzleMapLayout.cs:51-71`).
Chapter 3 makes three, freelance four, and the ends drift out of a seated reach. Place the **system
centres on an arc** around the seat instead; each system's spiral stays as it is.

## Decisions

- **An arc of systems, not of jobs.** One curve of 33 nodes would lose "one system per chapter" and
  crowd.
- **The arc is the circle through the map's origin, centred at the seat.** The seat is the authored
  Origin head `(0, 0.15, -0.45)` — 0.45 m behind the field — i.e. `(0, 0, -0.4286)` in map units (the
  map root is scaled 1.05, `Game.unity` §MapRoot). The arc's radius is its horizontal distance, so a
  single system lands where `Galaxy` puts it and the middle system stays on the field.
- **Angles follow today's spacing.** A gap between two systems is the chord the `x` line uses today
  (`r_i + r_{i+1} + gap`), turned into `2·asin(clamp(chord · k / 2R))` with one const `k = 1.5f` in
  `PuzzleMapLayout`; the sum is centred on the front axis. That reaches about 120° for four systems,
  keeps two and three on the same rule, and never overlaps — a fixed 40° gap would overlap the
  two-system map (chord 0.15 vs the 0.24 today's spacing gives). `k` is the look's tuning knob.
- **One place.** The arc lives in `PuzzleMapLayout.Systems`, so the route line, snap, fade and pose
  follow it unchanged. The seat is a `Vector3` parameter defaulting to a `PuzzleMapLayout.Seat` const,
  so every build — the selection, the run ending (`RunEndingState.cs:59-61`) and the post-story map
  ([freelance track](freelance-track.md)) — shares it with no call-site wiring.
- **Map space, not world space.** The layout stays in map-root local space. `Seat` is `(0, 0, -0.4286)`:
  the 0.45 m seat behind the field over the map root's 1.05 scale (`Game.unity` §MapRoot). If either the
  seat distance or the root's scale moves, `Seat` moves with it — the headset read catches a stale one.
- **The map gains depth.** At four systems the end systems sit up to ~0.2 m toward the player; the
  field and the console stay ahead of them, and the entry slide hides the difference
  (`MapPose.SlideToSelected`, `MapPose.cs:51-55`).

## Units

### U1 — the arc

- `PuzzleMapLayout.Systems`: the seat parameter and `Seat` const; centres at `seat + xz(angle) · R`;
  per-gap angles from the chord rule × `k`; per-system radii and spirals unchanged; one system still
  returns `Galaxy` (`PuzzleMapLayout.cs:36-45`).
- Exit: EditMode tests — one chapter equals `Galaxy`; two, three and four systems are symmetric about
  the front axis with every centre `R` from the seat; adjacent systems keep today's chord × `k` (no
  overlap); the farthest *job* is at most **0.60 map units (0.63 m)** from the seat. **Replace** the
  x-span assertion in `MapChapterTests.Systems_AreCentredOnTheMap` (`MapChapterTests.cs:47-55`); it
  encodes today's line and cannot stay green. `Systems_KeepEachChapterInItsOwnCluster` stays green as
  is (its chord is `r_i + r_{i+1} + gap`, and the arc widens it by `k`).

### U2 — the read

- In the editor under a moved and yawed `Origin`: the cube reaches every job from the seat, the route
  line follows the arc, entering a job does not jump.
- Exit: the manual check below.

## Manual check (headset)

1. Seated, the cube reaches every job in every system, end systems included, without leaning.
2. The systems read in chapter order, left to right; the route line follows the arc.
3. Two, three and four systems each read centred; the map still frames in front of the eyes.
4. Entering and leaving a job does not jump, end systems included; the field opens where the job opens.
5. The run ending's light-up animation follows the same arc.
6. With chapters 1–2 only, two systems sit centred; add chapter 3 and the arc opens up; add the
   freelance system and it still holds.
