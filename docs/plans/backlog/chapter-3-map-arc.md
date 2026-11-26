# Map arc

**Status:** next · presentation · updated 2026-11-26 — the chapter systems sit on an arc around the
seat, so every job stays in reach as chapter 3 and freelance add systems. Before chapter 3.

**Related:** [plans board](../README.md) · [chapter 3](chapter-3-jobs.md) · [freelance track](freelance-track.md) ·
[the cube](../../design/systems/cube.md) (the map uses the same cube)

## Goal

`PuzzleMapLayout.Systems` lines the chapter systems up side by side along `x`. Chapter 3 makes three,
freelance four, and the ends drift out of a seated reach. Place the **system centres on an arc** around
the seat instead; each system's spiral stays as it is.

## Decisions

- **An arc of systems, not of jobs.** One curve of 33 nodes would lose "one system per chapter" and
  crowd. About 120° for four systems; fewer systems sit centred in front.
- **One place.** The arc lives in `PuzzleMapLayout`, so the route line, the cube's snap and `MapPose`
  follow it unchanged.
- **The seat is outside the map.** Express the arc's centre in the map root's space and convert through
  its transform ([AGENTS](../../../AGENTS.md#field-relative-layout)); world/parent mix-ups pass every
  editor check and break only on the headset.

## Units

### U1 — the arc

- `PuzzleMapLayout.Systems` places centres by angle on a circle around the seat point (map space)
  instead of `x` along a line; the radius keeps today's distance to the middle system.
- Exit: `MapChapterTests` — every system centre is the same distance from the seat, chapter order runs
  left to right, two, three and four systems each centre on the player's front; existing layout tests
  green.

### U2 — the read

- In the editor under a moved and yawed `Origin`: the cube reaches every system from the seat, the
  route line follows the arc, entering a job does not jump.
- Exit: the manual check below.

## Manual check (headset)

1. Seated, the cube reaches every system without leaning.
2. The systems read in chapter order, left to right; the route line follows the arc.
3. Entering and leaving a job does not jump; the selected system still opens where the field opens.
4. With only chapters 1–2 baked, the two systems sit centred in front.
