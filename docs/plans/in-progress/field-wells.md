# Field wells — the lattice shows the planets' gravity from the start

**Status:** next · space shaping · updated 2026-11-11 — a spike (U1 code in; the toggle is on in the play
prefab for the headset pass): the lattice bends toward every planet
and the sun from the job's first frame, so the start check's failure and job 6's well read on sight; the
player's pinch shows as those wells changing plus the zone tint. Kept or dropped after a headset test.

**Related:** [the cube](../../design/systems/cube.md) §Feedback · [core loop](../../design/core-loop.md) §1 ·
[plans board](../README.md)

## Goal

Today the lattice starts flat and bends only with the player's zones, so a well is read from planet
size alone. Test whether drawing the field itself (the textbook "gravity bends the grid" picture, in 3D)
makes the read clearer without hiding the player's edit.

## Contract

- **The lattice shows the field from the start.** Every grid node is pulled toward each source (planets
  and the sun), so lines curve and bunch around deep wells. 3D pull, not a rubber-sheet dip: the
  Cluster jobs and the tilted fields have no "down".
- **Honest direction, compressed depth.** The pull follows the same sources the physics uses
  (`SpaceShapingSession.BuildField`), toward each source, softened by the source's radius. The depth is
  compressed (pull ∝ √strength / softened distance) so the sun does not swamp the planets. A node never
  crosses into a planet; nodes inside one are left alone. With several sources the pulls add, so the
  composed pull is clamped too — a sun behind a planet must not drag a node through it.
- **A picture, not a warp.** The wells bend the lattice and the dust; the ship and its line still fly
  the zones only (the wells' gravity is already in the simulation). Unlike the zone warp the wells do
  not move the trajectory, so the read is "the line passes near a well", not "the line bends through
  space".
- **The edit reads as change.** The zone warp is applied first and the wells after, so a grown planet
  deepens its well live while pinching, and a shrunk one flattens. The zone keeps the accent tint, so
  "what I touched" keeps its own channel. A ghost of the start grid is the fallback in U2, only if the
  headset says the change does not read.
- **A toggle, not a fork.** One serialized switch on the play view; off is today's lattice exactly. The
  map dioramas are out of the spike.

## Units

### U1 — the well pull, behind a toggle

- `WellPull` (plain struct, `Puzzles/Domain/Spacetime`): pulls a point toward each source with
  `gain × √strength / √(d² + radius²)`, each pull capped at `maxFraction` of the gap to the source's
  surface. No sources → identity. **The summed move is clamped too**: it stops where it would come
  closer to any source than that source's own cap allows, so a sun behind a planet cannot drag a node
  through it. A node inside any source stays put, whatever the other sources pull, and `maxFraction`
  is capped at 0.95 so a node never lands on a surface.
- `LatticeWarp` takes an optional `WellPull` and applies it after the zones; the lattice and the dust
  both read it.
- `LatticeWells` (serializable tuning: `enabled`, `gain`, `maxFraction`) on `SpaceShapingView`; it builds
  the `WellPull` from the session.
- Tests (fast suite): identity with no sources; a node moves toward a source; a stronger source pulls
  further; a pull never reaches the surface, even at a full cap; a node inside a source does not move,
  even while another source pulls; a sun and a planet pull a node between them without the node
  landing inside the planet; wells are applied after the zones; `enabled` off returns no wells.
- **Exit:** with the toggle on, the start field shows wells in Play mode; off, nothing changes.

### U2 — tune and test in the headset

- Tune `gain` / `maxFraction` on jobs 1, 3 and 6, then the 12-planet Cluster (job 7). One `wells` block
  in `Puzzle.prefab` serves every job, so this is one global tune; only a render-side ease-in is
  per-moment.
- The play field already runs `gridCount = 8` (a `Puzzle.prefab` override), 4 sides and 32 samples a
  line, so wells should curve smoothly; raise the builder's `samplesPerLine` only if they read as
  corners, and measure frame time on the Quest 3S (the warp runs on the lattice tick, 30 Hz, 6k nodes;
  the dust warps every frame, 400 motes).
- Job 1: if the bent start pulls focus from the Scale prompt, ease `gain` in during the start check
  (per-job tuning would need per-puzzle data and is out of the spike).
- Job 7: the composed clamp keeps nodes out of every source, but it does not stop neighbouring nodes
  from crossing each other; check that 13 added pulls do not fold cells, and lower `gain` if they do.
  The clamp also scales the whole summed move, so a node near one planet can lose its pull toward the
  others: check for pinned, flat cells between close planets too.
- `LatticeWells.For` rebuilds the pull on every lattice tick (30 Hz while pinching); cache it per field
  change if the frame-time measurement is close.
- If the change still does not read, add the start grid as a ghost — as a collaborator on the view, not
  another field in `SpaceShapingView` (it is at 241 of its 250 lines).
- **Exit:** a keep or drop decision, written here.

### U3 — keep it or drop it

- **Keep:** update [the cube](../../design/systems/cube.md) §Feedback ("starts as the field; the player's
  zones change it") and [core loop](../../design/core-loop.md) §1 (the wells are a cue); remove the
  toggle. Capture it for the video.
- **Drop:** delete `WellPull`, `LatticeWells` and the optional `LatticeWarp` argument; note why in
  [lessons](../../lessons.md).

## Headset — 2026-11-11

First pass at the default tune (`gain` 0.07, `maxFraction` 0.6): **the lattice reads as too chaotic.**
The likely cause is the bent start, not the wells: every planet bends the grid from the first frame.
**Second try (on in `Puzzle.prefab`): change only.** `LatticeWells.changeOnly` keeps the start field
(captured whenever every zone is at rest) and draws the difference: the start lattice is flat, a grown
planet's well appears and a shrunk one's flattens. `sunWeight` (0–1) also scales the sun's well for a
try without it. Same `gain` 0.07 and `maxFraction` 0.6. If this still reads as noise, drop the spike.

## Review — 2026-11-11

U1 holds: the sources come from `session.BuildField()` (the zone-applied positions and radii the
physics uses), the wells run after the zones in the same local space as the lattice nodes, a node
inside any source stays put, the summed move is clamped against every source, `maxFraction` is capped
below 1, and the fast suite covers each rule (`6a13099`). For the headset pass:

- **W1 — the toggle is on only in the working tree.** `Puzzle.prefab` has `enabled: 1` uncommitted.
  Keep it out of any commit until U3 decides; the competition build ships it off unless it is kept.
- **W2 — measure the cost before tuning.** Each node walks the sources three times (inside check,
  pulls, reach), about 6k nodes × 13 sources on job 7 at the 30 Hz lattice tick, plus 400 dust motes
  every frame, and `BuildField` allocates a field per render. If the Quest 3S frame time moves, fold
  the inside check into the pull loop and cache the `WellPull` per field change.
- Fixed in passing: the `LatticeWarp` header said the lattice never shows a warp the ship does not fly
  through; it now says the wells are a picture of the gravity, not a warp the ship follows.

## Manual check (headset)

1. Job 1 opens with the lattice bent around the planets and the sun; the start check's crash reads as
   running into a well's blocker.
2. Job 3: the blocker on the line has a visible well; shrinking it flattens the well live.
3. Job 6: the deep well near the line is obvious before the first pinch.
4. A grow pinch deepens the covered planet's well while pinching; the zone still tints accent.
5. Job 7 (12 planets + sun): the lattice is busy but the line's wells stand out; the sun does not
   swamp the field.
6. No lattice line passes through a planet or the sun: the wells bunch at the surfaces but never
   cross them.
7. Frame time holds 72 fps on the Quest 3S while pinching.
8. Toggle off: the lattice is exactly today's.
