# Space Shaping — Reframing and Variations (Recorded)

**Status:** backlog - design only, recorded. Not started. The actionable slice is
[Space shaping — POC](space-shaping.md).

**Related:** [POC](space-shaping.md) · [View frame (bubble window)](../../done/universe-bubble-window.md) · [Sector generation](../universe/universe-sector-generation.md) · [Sector field](../universe/universe-infinite-sector.md)

**Wiring rule:** all visuals and interaction props stay authored in the scene/prefabs
with serialized references. No runtime component lookup, mesh/material creation, or
hierarchy building. Follow the SerializeField wiring rules in the project `AGENTS.md`.

## 1. The reframing

The game becomes a physics puzzle about **shaping space**, not drawing paths and not a
management loop.

1. **Map View (macro).** A hologram of the universe in front of the player; rotate and
   pan to bring any sector in front of them.
2. **Sculpt View (micro).** A frame opens over a sector; the spacetime grid inside the
   frame is visualized and the player edits its **sources** (the grid is derived).
3. **Shape the space.** The player only moves the sources (planets, and optionally
   wells). The sources curve spacetime, so a different arrangement bends the ship's path
   differently.
4. **Finite routes.** A route is A -> B or A -> B -> C, not an infinite loop. The ship
   is a constant-thrust launch in a direction; the field decides whether it arrives.

Route guidance is a constant prediction line in Sculpt View plus faint "desire lines" in
Map View showing which outposts need service.

Naming: "Sculpt View" is the micro view (the earlier "Edition View" collided with
`EditionState`).

## 2. Views

- **Map View = the existing Bubble, evolved.** The frame model in
  [view frame](../../done/universe-bubble-window.md) already gives the movable box
  `{F, viewRadius, yaw}` over a universe fixed in content space; panning brings a
  distant sector in front. Map View is that, at a wide aperture.
- **Sculpt View = the same frame, narrow aperture, source editing active.** Opening/
  closing the aperture moves between Map and Sculpt; one frame, two apertures.
- **Why two views:** the forces read best at a bigger (zoomed-out) view. Map View is for
  reading the field and the desire lines; Sculpt View is for the close edit.
- **Room mode is superseded.** The done
  [two views + interaction](../../done/universe-views-and-interaction.md) and
  [bubble map drag](../../done/universe-transform.md) plans define Room/Bubble. Room is
  repurposed: Map replaces Bubble's role, Sculpt is new. The two-hand scale gesture and
  the frame survive; the "Room" presentation state does not.
- The grid is content-local, so it survives Map/Sculpt aperture changes and any content
  transform `D`.

## 3. Multi-ship and multi-destination

There is **one shared field and one region per puzzle**: all ships read the same planet
positions. That is what makes it one puzzle.

```text
degrees of freedom = source state (positions/strengths/wells)   (few)
constraints        = sum over (ships x destinations)   (grows fast)
```

When constraints outnumber degrees of freedom the level has **no solution** — not
"hard", unsolvable — and chaos makes the search non-smooth. Everything here is about
staying off that cliff.

### 3.1 Multiple ships — simultaneous

- All ships launch into the shared field and all must succeed.
- This is a full conjunction; only viable with a hard cap (2-3 ships), generous
  capture radii, and few destinations per ship.
- Level generation must verify a **single** arrangement that routes every ship.
- Ship-to-ship collisions are a later addition; for now ships only interact with
  planets.

### 3.2 Multiple destinations — sequential

- A route with several stops is a sequence of **re-launched legs**: the ship relaunches
  at each stop with a new direction. A -> B -> C is then a sequence of single-leg
  problems sharing the field.
- This maps directly onto the existing ordered-stops model (`LoopData` / `RouteLeg`):
  each leg is one launch, independently verifiable, and `RouteLeg.positions` holds the
  sampled path.
- The alternative — one continuous trajectory threading every waypoint — is a much
  tighter constraint and gets near-chaotic fast; do not start there.

### 3.3 Sequential-with-lock (fallback for many ships)

If simultaneous ships prove too constrained, freeze a solved ship's recorded path so
later planet moves do not disturb it, then solve the next ship against the evolving
field. Each solve is single-ship; the cost is a small fiction (a solved ship follows its
recorded path rather than re-simulating). Prefer this over per-ship separate fields if
you want many ships but still one world.

## 4. Level generation — editor-authored plus a seed-based creator

Forward (sources -> path) is easy and deterministic. The inverse (path -> sources)
is **underdetermined and chaotic**: many arrangements produce the same path, and
near-solutions diverge. Do not try to invert the trajectory map.

Two ways to make a level:

- **Editor-authored (primary).** The designer places planets and tunes the puzzle by
  hand. This is the way for now, and it lets players compare the same puzzle fairly.
- **Seed-based automatic creator (keep in mind).** A level seed carries ship count,
  planet density, and the physics variation (per-planet force multipliers). The creator
  generates candidate arrangements and verifies solvability.

For either path, verification is search-and-test, not inversion:

1. The target is the destination(s) (and optionally a desired path).
2. **Search** arrangements, forward-simulate each, and keep one that arrives within
   tolerance. That is a **solved** arrangement `S`.
3. The player's **start** is `S` with the solution's commands applied in reverse (a
   scrambled arrangement that misses).
4. Verify the start: it is **not already solved**, and **at least one** edit sequence
   reaches the target. Non-uniqueness is expected and fine.

- Solvability is guaranteed by construction; nothing is inverted.
- "Deform the initial map using the inverse actions the user has to make" is exactly
  step 3 when the moves are simple position translations.

## 5. Commands and scoring

- Every edit is a **command** with `Apply`/`Undo`. The player and the system share the
  same command type; a level's solution is a command sequence.
- **Minimum-move scoring:** the secured solution gives a known move count; the player's
  score rewards matching it. This is only meaningful because generation produces a
  verified solution.
- Scoring is **per puzzle**. There is no legacy economy (letters, money, rounds,
  `PlanetStore`, `EditionState`); each puzzle has its own score.

## 6. Data model

- Source state is the shared field state, stored once: planet positions/strengths and
  any wells. The field and grid are derived, not stored.
- Each planet's strength comes from its radius plus a seed-driven multiplier; store the
  multiplier so restore reproduces the field.
- Mass is coupled to size for now; a later element could decouple them (a small heavy
  planet), which would need a separate mass value.
- Each ship carries origin, ordered destinations, and its launch direction — the
  existing ordered-stops shape (`LoopData` / `RouteLeg`), with `RouteLeg.positions`
  produced by the integrator instead of a drawn/sampled line.
- No separate field persistence: the field is derived from positions and multipliers.
- Snapshot: the puzzle identity/seed + positions + ship/route list + the level's
  solution move count for scoring.

## 7. Relationships to other plans

Parked (collide with this framing):

- [Planet route drawing](../../parked/planet-route-drawing.md) — drawn routes are
  replaced by shaping space.
- [Ambient round activation](../../parked/universe-ambient-round-activation.md) — a
  management game, not the physics puzzle.

Relied on (support the framing):

- [View frame (bubble window)](../../done/universe-bubble-window.md) — the Map/Sculpt
  frame.
- [Sector generation](../universe/universe-sector-generation.md) — the structured,
  seeded system layout the sources come from.
- [Sector field](../universe/universe-infinite-sector.md) — the bounded, pre-spawned
  field Map View pans; the frame culls out-of-view systems.

Supersedes / repurposes:

- The Room presentation state in the done view plans (Map replaces its role; Sculpt is
  new). The two-hand scale gesture and the frame survive.
- The legacy round/letter/money economy (`EditionState` and friends), replaced by
  per-puzzle scoring.

## 8. Open questions

1. **Map/Sculpt transition.** One gesture that changes aperture, or an explicit open
   action on a sector? Default: aperture gesture plus an open action.
2. **Simultaneous ship cap.** Start at 2; raise only if generation stays feasible.
3. **Region size.** How many planets per puzzle before the search space is too chaotic?
   Driven by the seed's planet density.
4. **Level-gen cost.** Where does the automatic creator run — authored offline, or at
   load? Keep it out of the player's frame budget.
5. **Desire lines.** Are "outposts that need service" a demand layer that must exist, or
   decoration? A real demand layer is new gameplay and needs its own design.
