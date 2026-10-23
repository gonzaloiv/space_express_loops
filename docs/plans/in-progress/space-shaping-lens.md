# Space Shaping — The Lens (Testing the Mechanic)

**Status:** in progress - new. Tests whether the **lens** — the cube scale (L0; the content zoom
is L0b) — is a game. Supersedes the [test slice](../parked/space-shaping-test-slice.md)
(planet-drag verb); **inherits its harness, not its verb**.

**Related:** [project direction](../../design/project-direction.md) · [relativity as gameplay](../../design/relativity-as-gameplay.md) · [transport company](../../design/transport-company.md) · [parked test slice](../parked/space-shaping-test-slice.md) · [POC](../done/space-shaping.md)

**Wiring rule:** authored objects + serialized refs only; no runtime component lookup, mesh,
or asset creation. New readouts are authored objects in the lab scene.

## 1. Goal

Answer: **is the lens a game?**

The lens has **two gestures** (the design — [project direction](../../design/project-direction.md) §5):

| Gesture | Effect | Changes |
|---|---|---|
| **Cube scale** — resize the cube | stretches the local **space**: a shrunk zone is a **shortcut**, a magnified one a longer journey | travel time |
| **Content zoom** — grab inside *(L0b)* | zooms the zone's content: the planets **spread** and **grow** (mass ∝ radius) → a deeper well | the ship's clock |

**This POC tests the cube scale only** (L0); the content zoom is deferred to **L0b**. Both change
**time** — the game's hard resource — and the question is whether that loop has a goal, a cost,
and a legible read.

## 2. Harness inherited, verb not

- **Inherited (landed):** the determinism gate (`TrajectoryDeterminismTests`), the ship clock
  (`TrajectoryResult.ShipTime`), the deadline (`PuzzleData.deadline`), the HUD harness, the
  field/domain (`SpacetimeField`, `TrajectoryIntegrator`), the mesh read (`SpacetimeMeshView`).
- **Not inherited: the puzzles.** They were verified under the *move* verb, with deadlines tuned
  to it; author fresh in L2.
- **Superseded:** the planet-grab interaction and its polish (parked).

The parked test slice keeps the tuning data and the verification record.

## 3. Scope

### L0 — the lens (cube scale only)

The POC tests **only the lens** — the cube scale. The planets are **fixed** (the field); the
cube is the only interactive object. No planet drag and no content zoom: mixing them felt messy.

1. **Cube scale = a metric stretch.** Inside the cube the ship's step is scaled
   (`TrajectoryIntegrator.cs:24-25`), so the journey through the zone lengthens or shortens.
   (A content group-move was rejected: it bends the field like the old verb and never stretches
   the space.)
2. **Build the cube + its scale session.** The lab has **no** two-hand gesture — it carries an
   XR rig and the planet `Grabbable` only, and `UniverseGhostSession` is coupled to the bubble.
   This is build, not reuse.
3. **The mesh must warp with the zone scale** (`SpacetimeMeshView` warps only by the field
   today), or the stretch will not read. The cube's edge blends **softly** — a hard edge tears
   the mesh.
4. **Remove the planet drag** — the planets become fixed. Delete the `Grabbable`s and the
   move-range viz from the lab scene; `SpaceSourceView` keeps only its placement.
5. **The prediction on/off toggle** — Gate A's predict-then-reveal probe needs it, so it lands
   in **L0**, not L2.

**Deferred to L0b — the content zoom.** Grabbing inside to **spread + grow** the planets, and
its constraints (`Home`/`MoveRange`/`MaxScale`, `Separate`). Not in this POC.

### L1 — the game-feel pass

6. **Launch / commit** + a flying ship (authored, follows the computed path).
7. **A grade** — ship-time bucketed to stars; no move counts.
8. **Job name** + per-attempt telemetry.

### L2 — content

9. **3–5 puzzles**, authored fresh as *solved → scramble* (see
   [project direction](../../design/project-direction.md) §10), with a rising difficulty.

## 4. Phases

- **L0 (next):** the lens — the **cube scale**.
  **Exit:** scaling the cube changes the predicted path and ship-time; the mesh reads the
  stretch; deadlines re-tuned.
- **L0b:** the **content zoom**.
  **Exit:** grabbing inside spreads and grows the planets; the field and the clock respond.
- **L1:** the game-feel pass.
  **Exit:** a puzzle can be committed, watched, graded, and re-attempted.
- **L2:** content + legibility.
  **Exit:** 3–5 puzzles with a real spread; the predict-then-reveal probe works.

## 5. Test plan

Run seated, on device, hand-tracked, with a tester who has not read the docs. **Gate A** tests
the puzzle; **Gate B** tests the game (needs L1). The **prediction toggle must land in L0** —
Gate A needs it.

### Puzzle metrics (Gate A)

1. **Solve rate** per puzzle.
2. **Time-to-solve and edit count** before success.
3. **The lens probe:** can the player predict how the cube scale changes the path and the clock?
4. **Is the deadline binding?** Do they plan against it, or ignore it?
5. **Qualitative:** can they explain *why* their solution worked?

### Game metrics (Gate B, needs L1)

6. **Retry after a non-perfect grade.**
7. **Hesitation at commit.**
8. **Does the clock land** (noticed slowing near mass)?
9. **Wants another job.**

### The A/B that isolates the grade

Run the same puzzle **twice**: live (no commit) and commit + flight + grade. If engagement does
not change, cut the grade.

## 6. Decision gates

**Gate A — mechanic (puzzle).** Pass: testers solve, **predict how the cube scale changes the
run**, and explain why → build L1 and run Gate B. Fail: they fiddle blindly and cannot predict →
revisit the interaction.

**Gate B — game (fun).** Requires L1. Pass: testers retry after a grade, hesitate at commit, and
want the next job → **only Gate B justifies integration.** Fail: they solve once, mash Launch,
and ignore the grade → fix the stakes, not the presentation.

## 7. Defaults

| Parameter | Default |
|---|---|
| scene | `SpaceShapingPOC.unity` (lab) |
| posture | seated, on device, hand tracking |
| gestures | **cube scale only** (the lens); content zoom is L0b |
| dilation model | depth-only: `dτ = dt · clamp(1 + kΦ·Φ)` |
| score | ship-time → stars (1–3); **no fuel** (not in the code) |
| puzzles | 3–5, rising difficulty |
| success | Arrive **and** `shipTime ≤ deadline` |
| deferred | rotate, black holes, one-view polish, integration, sectors |

## 8. Risks

| Risk | Mitigation |
|---|---|
| The two scales are confusable | *L0b* — deferred with the content zoom; L0 has one gesture |
| Scale = time is illegible | show the clock and the path before commit; the mesh must read the stretch |
| The cube is "just a group drag" | the metric stretch (L0.1) — not a content move |
| Constraints cancel the zoom | exempt cube-scaled sources from `Separate`; scale `Home`/`MoveRange` (L0.5) |
| Deadlines stop binding | re-tune them under the new verb — an explicit L0/L2 exit item |
| The transform is untested | add determinism coverage for the zone transform |
| Scope creep into rotate / black holes | §7 deferred list; the gate must pass first |

## 9. Decisions

| Decision | Call |
|---|---|
| Step-count cap (magnified zone) | **Raise `maxSteps` with the zone** — a sim-budget limit, not the score. |
| Tunnelling (shrunk zone) | **Sweep the segment.** |
| Content-zoom spread | *L0b* — the spread is expected; scale `Home`/`MoveRange`. |
| Falloff width | Tune in the lab (test before fixing). |
| Star calibration | The puzzle creator's job (the authored par). |
| Zone read | The planets visibly growing/shrinking is enough. |

## 10. What's deferred (not this slice)

- **Rotate (frame dragging)** — the variety beat, act 2
  ([relativity as gameplay](../../design/relativity-as-gameplay.md) §3.2).
- **Black holes (event horizon)** — the stake, after the gate
  ([relativity as gameplay](../../design/relativity-as-gameplay.md) §3.3).
- One-view polish, integration, sector generation.
- `PuzzleData.solutionMoves` (dead) — remove it, consistent with "no move counts".

## 11. Readiness to start (2026-10-23)

The design decisions are settled, the review's stale lines are fixed (the gestures table §1;
"build the cube" §3 L0.2; "no fuel" §7; the prediction toggle scoped into L0 §3 L0.5), and the
zone model is defined in §12 with its safety cases. L0's build list is unambiguous and the plan
is ready to implement.

## 12. The zone model

L0 needs a **zone** — the region whose space is scaled. In the lab there is one cube = one zone.

| Field | Meaning |
|---|---|
| `centre` | zone centre (content space) |
| `halfExtents` | the zone's box half-size |
| `scale` | the metric multiplier (the cube's resize): > 1 magnified, < 1 shrunk |
| `falloff` | the edge blend width (content units); `0` = hard edge (tears the mesh) |

- **Inside-ness:** `t(p) = clamp01( min_axis( (halfExtents − |p − centre|) / falloff ) )` — 1 deep
  inside, 0 outside the band. **`falloff <= 0` is a step function** (1 inside, 0 outside), never a
  division.
- **Rule:** the ship's **position step** is **divided** by `lerp(1, scale, t)` — a magnified zone
  is a **longer journey** (more ship-time), a shrunk one a **shortcut**. Only the position step is
  scaled, not the velocity update (`TrajectoryIntegrator.cs:24-25`) — the field acts in coordinate
  time inside the zone; state that in code.
- **Step-count safety:** a magnified zone raises the step count to cross it, and the integrator
  caps at `maxSteps`, returning `Miss` on overflow (`TrajectoryIntegrator.cs:17-33`; parked §12
  A1). **Decided: raise `maxSteps` with the zone** so a magnified run is not truncated.
- **Tunnelling:** a shrunk zone multiplies the step, and crash detection is a per-step
  point-in-sphere (`TrajectoryIntegrator.cs:39-49`), so a shortcut can jump past a planet.
  **Decided: sweep the segment.**
- **Consumers:** `TrajectorySettings` gains the zone (L0.1); `SpacetimeMeshView` warps by the zone
  (L0.4).
- **Determinism:** `t(p)` is a pure function of position → the trajectory stays deterministic;
  add a test.

## 13. Zone model review — resolved (2026-10-23)

The four defects and two notes are folded into §12: the `falloff <= 0` branch, the `maxSteps`
cap, the tunnelling clamp, and the position-step-only statement. §11's stale blocker paragraph is
removed.
