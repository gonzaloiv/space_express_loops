# Map arc

**Status:** next · presentation · updated 2026-11-27 — the chapter systems sit on an arc around the
seat, so every job stays in reach as chapter 3 and freelance add systems. Before chapter 3.

**Related:** [plans board](../README.md) · [chapter 3](chapter-3-jobs.md) · [freelance track](freelance-track.md) ·
[cube](../../design/systems/cube.md) (the map uses the same cube)

## Goal

`PuzzleMapLayout.Systems` lines the chapter systems up side by side along `x` (`PuzzleMapLayout.cs:51-71`).
Chapter 3 makes three, freelance four, and the ends drift out of a seated reach. Place the **system
centres on an arc** around the seat instead; each system's spiral stays as it is.

## Decisions

- **An arc of systems, not of jobs.** One curve of 33 nodes would lose "one system per chapter" and
  crowd.
- **The arc is the circle through the map's origin, centred at the seat.** The seat's horizontal
  distance is `SeatSettings.distance` 0.45 m (`Game.unity` §SeatPlacement; `ConsoleLayout.defaultHead`
  is only the editor fallback) over the map root's 1.05 scale (`Game.unity` §MapRoot): **0.4286 map
  units**. The arc is flat (height 0) and passes through the origin, so a single system lands where
  `Galaxy` puts it.
- **The seat is a `Vector3`, but not a `const`.** C# cannot make a `Vector3` `const` and a default
  parameter cannot read a static field, so `Seat` is `public static readonly` and `Systems` keeps its
  current signature, forwarding to an overload that takes the seat. Existing calls
  (`RunMap.cs:29`, `MapChapterTests.cs:34`) keep their meaning. Hardcoding it keeps the domain free of
  wiring; if the seat distance or root scale moves, `Seat` must be updated by hand — only the headset
  read catches a stale one (CI cannot see the scene).
- **Angles follow today's spacing.** A gap between two systems is the chord the `x` line uses today
  (`r_i + r_{i+1} + gap`, clamped at zero), turned into `2·asin(clamp(chord · ArcSpacing / 2R))` with
  one const `ArcSpacing = 1.5f`; the angular span is centred on the front axis and systems are placed in
  chapter order from negative (the player's left) to positive (the player's right). For the shipped
  sizes that is ~61° for two, ~96° for three and ~113° for four with the ten launch contracts (~105°
  once the other ten land). Inner systems follow their own pair's gap, so only the two ends are mirror
  images, not the whole set. `ArcSpacing` is the look's tuning knob and does not change reach.
- **Reach is structural.** Every centre is exactly `R` from the seat, so the farthest job is at most
  `R + the largest system's radius`; `ArcSpacing` cannot improve it. With `R = 0.4286` the outer node
  reaches 0.598 map units for `[7,8]`, 0.572 for `[7,8,6]`, 0.561 for `[7,8,6,10]` and 0.594 for
  `[7,8,6,20]`. The test bound is **0.62 map units (0.65 m)**, which leaves room for the parked
  chapter 2 job to return and for the update.
- **One place.** The seat lives in `PuzzleMapLayout.Systems`, so the route line, snap, fade and pose
  follow it unchanged. The default seat means every build — the selection, the run ending
  (`RunEndingState.cs:59-61`) and the post-story map ([freelance track](freelance-track.md)) — shares
  it with no call-site wiring.
- **Map space, not world space.** The layout stays in map-root local space. `Seat` is the horizontal
  projection `(0, 0, -0.4286)`; the vertical head offset (0.15 in the editor, 0.12–0.35 at runtime)
  does not affect the flat arc. If the seat distance or the root's scale moves, `Seat` goes stale.
- **The map gains depth.** At four systems the end systems sit up to ~0.2 m toward the player (0.20 m
  for `[7,8,6,10]`); the field and console stay ahead of them, and the entry slide hides the difference
  (`MapPose.SlideToSelected`, `MapPose.cs:51-55`).

## Units

### U1 — the arc

- `PuzzleMapLayout.Seat` is `public static readonly Vector3` = `(0f, 0f, -0.4286f)` (the horizontal
  projection; the arc is flat). `Systems` keeps `(sizes, radius = 0.24f, gap = -0.05f)` and forwards to
  `Systems(sizes, seat, radius, gap)`; one system still returns `Galaxy` (`PuzzleMapLayout.cs:36-45`) —
  exact while the seat is straight behind the origin.
- Centres at `seat + new Vector3(sin θ, 0, cos θ) * R`; per-pair `θ` from the chord rule ×
  `ArcSpacing`; radii and spirals unchanged. Keep `PlaceCentres` at ≤5 parameters (fold `radius`/`gap`
  into a small settings value or compute inline) and the type under 250 lines.
- Update the stale comments: `PuzzleMapLayout.cs:47-50` ("side by side … centred on the map") and
  `MapPose.cs:41-42` ("layout is centred on the map's origin") — the span is centred, not every system.
- Exit: EditMode tests — one chapter equals `Galaxy`; every centre is `R` from the seat; the end
  centres are mirror images about the front axis and the angular span is centred (unequal end radii
  make the bounding span asymmetric — assert the centres, not the extents); each adjacent pair is
  today's chord × `ArcSpacing` (no overlap while `r_i + r_{i+1} > 0.15` at the default gap, so test the
  shipped sizes or a positive gap); the systems run left to right in chapter order; the farthest
  actual node is ≤ **0.62 map units (0.65 m)** for `[7,8]`, `[7,8,6]`, `[7,8,6,10]` and
  `[7,8,6,20]`. **Replace** the x-span assertion in `MapChapterTests.Systems_AreCentredOnTheMap`
  (`MapChapterTests.cs:47-55`); it encodes today's line and cannot stay green.
  `Systems_KeepEachChapterInItsOwnCluster` stays green as is (its chord is `r_i + r_{i+1} + gap`, and
  the arc widens it by `ArcSpacing`), provided the seat travels in the overload, not the existing
  parameter list.

### U2 — the read

- In the editor under a moved and yawed `Origin`: the cube reaches every job from the seat, the route
  bends through the systems (it is job-to-job chords, not a smooth arc), entering a job does not jump,
  and the four-system ends do not crowd the console.
- Exit: the manual check below.

## Manual check (headset)

1. Seated, the cube reaches every job in every system, end systems included, without leaning.
2. The systems read in chapter order, left to right; the route bends through the systems.
3. Two, three and four systems each read centred; the map still frames in front of the eyes and the end
   systems do not crowd the console.
4. Entering and leaving a job does not jump, end systems included; the field opens where the job opens.
5. The run ending's light-up animation follows the systems' order and the arc.
6. With chapters 1–2 only, the two systems now sit in a shallow V; add chapter 3 and the freelance
   system and it still holds.
7. `ArcSpacing` reads right at two and four systems; adjust it and re-run the EditMode bounds if it
   moves.
