# Puzzle Creator

How to bake a level with **DigitalLove/Space Shaping/Puzzle Creator**.

## The window

| Field / button | What it is |
|---|---|
| **Seed asset** | The working brief. Every button reads or writes this asset. |
| **Settings** | `SpaceShapingSettings` — the sim constants (scales, entropy, spacing). |
| **Shape** / **Planets** | Outputs of *Export shape* / *Export planets*, exposed so you can also hand-assign an authored pair. |
| **Export shape** | Builds the planet layout from the seed (`seed`, `planetCount`, `fieldShape`). |
| **Export planets** | Generates the planet positions from the shape. |
| **Generate (rebuild)** | Exports shape + planets, then bakes a level for the seed's brief. |
| **Find working seed** | Scans seeds until one bakes. **Single-ship only.** |
| **Save level** | Writes `Resources/{seed}_Shape_Planets_Puzzle.asset` — this **overwrites** any level already saved for that seed. |

## The seed

| Field | Meaning |
|---|---|
| `seed` | Field-generation key. It only decides the planet layout — nothing in the bake or the runtime reads it. |
| `planetCount` | Planets in the field. **The single most important knob** for multi-ship levels (`ships > 1`). |
| `fieldShape` | `Auto` derives Ring / Spiral / Cluster from the seed. |
| `translatable` | The cube may be moved. A fixed level places several cubes instead. |
| `zones` | Zones the solution scales (1–3). **Ignored by multi-ship levels** — they always ship one movable frame. |
| `ships` | Ships the level must land. `1` is a single-ship puzzle; `2+` bakes a multi-ship level. |
| `resizable` | The frame may be resized at runtime (1x–2x). |

## Ships

| `ships` | Bake | Ships relate by |
|---|---|---|
| `1` | `PuzzleFieldBake` | One ship: `origin` → `destination`. |
| `2+` | `PuzzleMultiShipBake` | Each ship gets its own pair of planets. |

A multi-ship brief needs `ships >= 2`; anything else takes the single-ship bake.

## Setup

### Single-ship

```
planetCount   9
translatable  true (or false for a fixed multi-cube level)
zones         1–3
ships         1
```

`Generate` → `Save`. Use **Find working seed** if the bake rejects the seed — a single-ship level
needs a failing start, and a movable one-zone level needs at least two routes.

### Multi-ship

```
planetCount   12
translatable  true
zones         1
ships         2
```

`Generate` → `Save`.

**`planetCount` is what makes or breaks a multi bake.** The bake only counts rescues the *field
alone* produces (the runtime number is inflated by time dilation: a zone's scale raises the global
step budget, so it "rescues" runs it never touches). Measured on seed 1:

| `planetCount` | best rescues, runtime budget | best rescues, field alone |
|---|---|---|
| 6 | 7 | **1** — no multi bake possible |
| 9 | 23 | 5 |
| 12 | 39 | 6 |

Below ~9 planets there is nothing for a second ship to ride, and `Generate` returns `NoSolution`.

## Gotchas

- **`zones` is silently ignored by multi-ship levels.** A multi level is always one movable frame.
- **`Find working seed` is disabled for multi-ship levels.** It scans single-ship bakes only.
- **`Save level` overwrites** `Resources/{seed}_Shape_Planets_Puzzle.asset`. Bake a second level by
  giving it its own seed asset, or copy the saved asset aside first.
- **A rejected bake is not a crash.** `NoSolution` means no zone held the required runs across a
  scale plateau; raise `planetCount`, or try another `seed`.
