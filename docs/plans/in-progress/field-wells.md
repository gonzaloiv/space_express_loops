# Field wells — the lattice shows the planets' gravity from the start

**Status:** now · space shaping · updated 2026-11-11 — a spike: the lattice bends toward every planet
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
  crosses into a planet; nodes inside one are left alone.
- **The edit reads as change.** The zone warp is applied first and the wells after, so a grown planet
  deepens its well live while pinching, and a shrunk one flattens. The zone keeps the accent tint, so
  "what I touched" keeps its own channel. A ghost of the start grid is only added if the headset says
  the change does not read.
- **A toggle, not a fork.** One serialized switch on the play view; off is today's lattice exactly. The
  map dioramas are out of the spike.

## Units

### U1 — the well pull, behind a toggle

- `WellPull` (plain struct, `Puzzles/Domain/Spacetime`): pulls a point toward each source with
  `gain × √strength / √(d² + radius²)`, each pull capped at `maxFraction` of the gap to the source's
  surface. No sources → identity.
- `LatticeWarp` takes an optional `WellPull` and applies it after the zones; the lattice and the dust
  both read it.
- `LatticeWells` (serializable tuning: `enabled`, `gain`, `maxFraction`) on `SpaceShapingView`; it builds
  the `WellPull` from the session.
- Tests (fast suite): identity with no sources; a node moves toward a source; a stronger source pulls
  further; a pull never reaches the surface; a node inside a source does not move.
- **Exit:** with the toggle on, the start field shows wells in the editor; off, nothing changes.

### U2 — tune and test in the headset

- Tune `gain` / `maxFraction` on jobs 1, 3 and 6, then the 12-planet Cluster (job 7).
- Lattice density: if the wells look coarse at `gridCount = 4`, rebuild the authored mesh denser
  (`DigitalLove/Space Shaping/Build Lattice Mesh`) and measure frame time on the Quest 3S; the warp
  runs on every preview frame.
- Job 1: if the bent start pulls focus from the Scale prompt, soften its wells or ease them in during
  the start check.
- **Exit:** a keep or drop decision, written here.

### U3 — keep it or drop it

- **Keep:** update [the cube](../../design/systems/cube.md) §Feedback ("starts as the field; the player's
  zones change it") and [core loop](../../design/core-loop.md) §1 (the wells are a cue); remove the
  toggle. Capture it for the video.
- **Drop:** delete `WellPull`, `LatticeWells` and the optional `LatticeWarp` argument; note why in
  [lessons](../../lessons.md).

## Manual check (headset)

1. Job 1 opens with the lattice bent around the planets and the sun; the start check's crash reads as
   running into a well's blocker.
2. Job 3: the blocker on the line has a visible well; shrinking it flattens the well live.
3. Job 6: the deep well near the line is obvious before the first pinch.
4. A grow pinch deepens the covered planet's well while pinching; the zone still tints accent.
5. Job 7 (12 planets + sun): the lattice is busy but the line's wells stand out; the sun does not
   swamp the field.
6. The trajectory never crosses a drawn well in a way the ship does not fly (the lines and the path
   agree).
7. Frame time holds 72 fps on the Quest 3S while pinching.
8. Toggle off: the lattice is exactly today's.
