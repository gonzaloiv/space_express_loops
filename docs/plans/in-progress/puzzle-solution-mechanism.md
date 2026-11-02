# Puzzle solution mechanism — curve or clear

**Status:** next · space shaping · updated 2026-11-02 — the creator classifies and requests a solution's
role: curve the path with a well, or clear the obstacle out of it.

**Related:** [the cube](../../design/systems/cube.md) · [jobs](../../design/jobs.md) ·
[core loop](../../design/core-loop.md) · [first-minute read](../in-progress/first-minute-read.md)

## 1. Why

The bake only knows that the run arrives. Scaling a source changes **both** its collision radius and its
well (`SpaceShapingSession.cs:127-137`), so a solve that shrinks a blocker out of the chord is
indistinguishable from a well that bends the path (`PuzzleVerifier.cs:25-38`). The run is mostly about
curves; the obstacle is a later variation, and a multi-zone job can mix the two roles. The creator has to
name the role so a job teaches the intended one.

## 2. Slices

### S1 — classify

- Find the source the **start run crashes into**. The integrator already knows the hit
  (`TrajectoryIntegrator.cs:55-64`); expose it on `TrajectoryResult`.
- Add `PuzzleSolutionKind` (`Curve`, `Clear`, `Mixed`) and classify:
  - **Clear** — the solution covers the start-crash source and **shrinks** it (`scale < 1`);
  - **Curve** — the solution covers another body, or grows the covered one;
  - **Mixed** — several zones do both.
- Record the solved path's max deviation from the chord as a secondary tag (cheap, no new simulation).
- **Exit:** a shrink-the-blocker bake classifies `Clear`; a grow-a-well bake classifies `Curve`; a
  two-zone clear+curve classifies `Mixed`.

### S2 — request

- Add `PuzzleMechanism` (`Any`, `Curve`, `Clear`) to `PuzzleSeed` and carry it through `PuzzleBrief`.
  Default `Any` so existing seeds keep baking; the jobs set their role.
- `PuzzleFieldBake`/`PuzzleSeedScanner` reject a mismatch with a new `PuzzleBakeStatus` (`NotCurved` /
  `NotCleared`) and try the next seed. `PuzzleCreatorWindow` shows the classified kind and a role filter.
- **Exit:** a `Curve` request rejects a `Clear` solution and vice versa; the creator says which role came
  out; the seed scan retries.

### S3 — content

- Job 1 requests **Curve**: the cube covers a **run planet**, and the blocker is only why the start fails.
- The obstacle is a later variation (`Clear`); a multi-zone job can mix one `Clear` and one `Curve`.
- Re-bake the set, tune attempts, and record each job's role in [jobs](../../design/jobs.md).
- **Exit:** the tutorial bakes `Curve`, the obstacle level bakes `Clear`, and the mixed level bakes
  `Mixed`; the job table states the role.

## 3. Tests

- Classifier: hand-built Clear, Curve and Mixed cases return the right kind.
- A requested role rejects the wrong kind and the creator reports it; the seed scan continues.
- The tutorial's bake is `Curve`; the obstacle level's is `Clear`; the mixed level's is `Mixed`.
- The deviation tag is stable for a fixed seed (determinism).

## 4. Out of scope

- The full well/metric ablation; it is a follow-up only if the cheap classifier mislabels edge cases.
- Runtime detection — this is authoring only.
- Automatic role inference from the field without a requested role.

## 5. Risks

- **Edge cases.** A grown blocker curves, a moved source can clear without shrinking, and a zone can cover
  both. Classify by the start-crash source and tag `Mixed` rather than pretending one label fits.
- **Endpoint bodies.** Skip a start-crash source sitting at the origin or destination; it is not an
  obstacle.
- **Yield.** Requesting a role drops seeds that bake the other kind; tune attempts and the seed scan, and
  expect the tutorial to need a new seed.
- **Re-bake.** The tutorial's field changes when job 1 requests `Curve`; re-verify it with
  `PuzzleVerifier` before the first-minute pass.
