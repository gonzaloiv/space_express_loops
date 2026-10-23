# Space Shaping — The Lens (Testing the Mechanic)

**Status:** in progress - new. Tests whether the **lens** (the cube) — *scale the space,
scale the content* — is a game. Supersedes the [test slice](../parked/space-shaping-test-slice.md)
(planet-drag verb); **inherits its harness, not its verb**.

**Related:** [project direction](../../design/project-direction.md) · [relativity as gameplay](../../design/relativity-as-gameplay.md) · [transport company](../../design/transport-company.md) · [parked test slice](../parked/space-shaping-test-slice.md) · [POC](../done/space-shaping.md)

**Wiring rule:** authored objects + serialized refs only; no runtime component lookup, mesh,
or asset creation. New readouts are authored objects in the lab scene.

## 1. Goal

Answer: **is the lens a game?**

The lens has **two gestures**, both two-hand scales:

| Gesture | Effect | Changes |
|---|---|---|
| **Cube scale** — resize the cube | stretches the local **space**: a shrunk zone is a **shortcut**, a magnified one a longer journey | travel time |
| **Content zoom** — grab inside | scales the **planets** (mass ∝ radius) → a deeper well | the ship's clock |

Both change **time** — the game's hard resource. The question is whether that loop has a goal,
a cost, and a legible read.

## 2. Harness inherited, verb not

- **Inherited (landed):** the determinism gate (`TrajectoryDeterminismTests`), the ship clock
  (`TrajectoryResult.ShipTime`), the deadline (`PuzzleData.deadline`), the HUD harness, the
  field/domain (`SpacetimeField`, `TrajectoryIntegrator`), the mesh read (`SpacetimeMeshView`).
- **Not inherited: the puzzles.** They were verified under the *move* verb, with deadlines tuned
  to it; author fresh in L2.
- **Superseded:** the planet-grab interaction and its polish (parked).

The parked test slice keeps the tuning data and the verification record.

## 3. Scope

### L0 — the lens interaction

1. **Cube scale = a metric stretch.** Inside the cube the ship's step is scaled
   (`TrajectoryIntegrator.cs:24-25`), so the journey through the zone lengthens or shortens.
   (A content group-move was rejected: it bends the field like the old verb and never stretches
   the space.)
2. **Content zoom = mass.** Grabbing inside scales the planets (`mass ∝ radius`) → the well and
   the clock.
3. **Build the cube + its scale session.** The lab has **no** two-hand gesture — it carries an
   XR rig and the planet `Grabbable` only, and `UniverseGhostSession` is coupled to the bubble.
   This is build, not reuse.
4. **The mesh must warp with the zone scale** (`SpacetimeMeshView` warps only by the field
   today), or the stretch will not read. The cube's edge blends **softly** — a hard edge tears
   the mesh.
5. **Constraints:** exempt cube-scaled sources from `SourceConstraints.Separate`, and scale
   `Home`/`MoveRange` with the zone — otherwise the move-range clamp cancels the zoom.
6. **The prediction on/off toggle** — Gate A's predict-then-reveal probe needs it, so it lands
   in **L0**, not L2.

### L1 — the game-feel pass

7. **Launch / commit** + a flying ship (authored, follows the computed path).
8. **A grade** — ship-time bucketed to stars; no move counts.
9. **Job name** + per-attempt telemetry.

### L2 — content

10. **3–5 puzzles**, authored fresh as *solved → scramble* (see
    [project direction](../../design/project-direction.md) §10), with a rising difficulty.

## 4. Phases

- **L0 (next):** the lens interaction.
  **Exit:** scaling the cube and the content changes the field, the predicted path and
  ship-time; the mesh reads the stretch; deadlines re-tuned.
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
3. **The lens probe:** can the player tell whether they are scaling the **space** or the
   **content**? And can they predict the curve?
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

**Gate A — mechanic (puzzle).** Pass: testers solve, **distinguish the two scales**, predict the
curve, and explain why → build L1 and run Gate B. Fail: they cannot tell the two scales apart,
fiddle blindly, and cannot predict → revisit the interaction.

**Gate B — game (fun).** Requires L1. Pass: testers retry after a grade, hesitate at commit, and
want the next job → **only Gate B justifies integration.** Fail: they solve once, mash Launch,
and ignore the grade → fix the stakes, not the presentation.

## 7. Defaults

| Parameter | Default |
|---|---|
| scene | `SpaceShapingPOC.unity` (lab) |
| posture | seated, on device, hand tracking |
| gestures | cube scale = space (metric); content zoom = mass |
| dilation model | depth-only: `dτ = dt · clamp(1 + kΦ·Φ)` |
| score | ship-time → stars (1–3); **no fuel** (not in the code) |
| puzzles | 3–5, rising difficulty |
| success | Arrive **and** `shipTime ≤ deadline` |
| deferred | rotate, black holes, one-view polish, integration, sectors |

## 8. Risks

| Risk | Mitigation |
|---|---|
| The two scales are confusable | distinct feedback (the cube visibly stretches the space; the planets visibly grow); the §5 probe |
| Scale = time is illegible | show the clock and the path before commit; the mesh must read the stretch |
| The cube is "just a group drag" | the metric stretch (L0.1) — not a content move |
| Constraints cancel the zoom | exempt cube-scaled sources from `Separate`; scale `Home`/`MoveRange` (L0.5) |
| Deadlines stop binding | re-tune them under the new verb — an explicit L0/L2 exit item |
| The transform is untested | add determinism coverage for the zone transform |
| Scope creep into rotate / black holes | §7 deferred list; the gate must pass first |

## 9. Open questions

1. Is the content zoom **mass-only**, or mass + the local metric?
2. **Soft-brush falloff** — how far does a zone's influence blend into its neighbours?
3. **Star calibration** — the verified solution's score, or a margin above it?
4. Does a magnified zone need a stronger read than the mesh warp to be legible?

## 10. What's deferred (not this slice)

- **Rotate (frame dragging)** — the variety beat, act 2
  ([relativity as gameplay](../../design/relativity-as-gameplay.md) §3.2).
- **Black holes (event horizon)** — the stake, after the gate
  ([relativity as gameplay](../../design/relativity-as-gameplay.md) §3.3).
- One-view polish, integration, sector generation.
- `PuzzleData.solutionMoves` (dead) — remove it, consistent with "no move counts".

## 11. Readiness to start (2026-10-23)

The design decisions are settled, and the review's stale lines are fixed in this revision (the
gestures table §1; "build the cube" §3 L0.3; "no fuel" §7; the prediction toggle scoped into
L0 §3 L0.6). One item still blocks a clean L0 start.

**The zone model does not exist.** L0.1 commits the integrator to *knowing the cube's zone*
("inside the cube the ship's step is scaled"), but the code has no zone type, no zone bounds,
and no zone→step-scale rule: `TrajectoryIntegrator.Integrate` takes only field, start, launch
and settings (`TrajectoryIntegrator.cs:8-9`). §9 Q2 (falloff) and Q4 (read) are the same
undefined object seen from two sides.

**Resolved:** the zone is defined in §12 (centre, halfExtents, scale, falloff) — L0's build list
is now unambiguous and the plan is ready to implement.

## 12. The zone model

L0 needs a **zone** — the region whose space is scaled. In the lab there is one cube = one zone.

| Field | Meaning |
|---|---|
| `centre` | zone centre (content space) |
| `halfExtents` | the zone's box half-size |
| `scale` | the metric multiplier (the cube's resize): > 1 magnified, < 1 shrunk |
| `falloff` | the edge blend width (content units); `0` = hard edge (tears the mesh) |

- **Inside-ness:** `t(p) = clamp01( min_axis( (halfExtents − |p − centre|) / falloff ) )` — 1 deep
  inside, 0 outside the band.
- **Rule:** the ship's step is **divided** by `lerp(1, scale, t)` — a magnified zone is a **longer
  journey** (more ship-time), a shrunk one a **shortcut**.
- **Consumers:** `TrajectorySettings` gains the zone (L0.1, `TrajectoryIntegrator.cs:24-25`);
  `SpacetimeMeshView` warps by the zone (L0.4).
- **Determinism:** `t(p)` is a pure function of position → the trajectory stays deterministic;
  add a test (S2).

## 13. Zone model review (2026-10-23)

§12 defines the zone, so L0 is startable — but four defects to fix before coding.

1. **`falloff = 0` divides by zero.** The inside-ness is `… / falloff`, but the table says
   `0 = hard edge`. Add a `falloff <= 0` branch (a step function at the edge).
2. **A magnified zone fights `maxSteps`.** Dividing the step raises the step count to cross the
   zone; the integrator caps at `maxSteps` and returns `Miss` on overflow
   (`TrajectoryIntegrator.cs:17-33`; parked §12 A1). A large `scale` can truncate a run that
   should arrive — decide the cap (raise `maxSteps`, or clamp the per-zone step).
3. **A shrunk zone can tunnel planets.** `scale < 1` multiplies the step, and crash detection is
   a per-step point-in-sphere (`TrajectoryIntegrator.cs:39-49`), so a shortcut can jump past a
   planet it should hit. Needs a swept check or a step-size clamp.
4. **§11 contradicts itself.** It still opens "the zone model does not exist … one item still
   blocks a clean L0 start", then says "Resolved: the zone is defined in §12". Drop the blocker
   paragraph now that §12 exists.

**Minor:** §12's "add a test (S2)" cites `S2`, a label from the review section that was deleted —
drop the reference.

**Note:** the rule scales only the position step, not the velocity/acceleration update
(`TrajectoryIntegrator.cs:24-25`). That may be intended for a stylized metric, but state it, since
it changes how the field acts inside the zone.
