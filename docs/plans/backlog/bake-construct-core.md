# Bake — construct the answer first (one ship)

**Status:** next · puzzle creator · updated 2026-11-27 — a generator that places the route, the
obstacle and the answer first and fills the field around them, then verifies with today's validator;
one ship, Clear and Curve. Not started.

**Bakes:** off the editor — U4's yield comparison.

**Related:** [plans board](../README.md) · [bake answer](../in-progress/bake-answer.md) (stores what this builds) ·
[bake construct fleet](bake-construct-fleet.md) (the same for fleets)

## Why

Today a job is random field → full zone × scale search → validator → usually reject, and every rule
(edge sun, one shared blocker, no sun answer) is one more filter, so pass rates fall with each rule:
200–400 seeds for a three-ship job. Constructing the answer makes the rules steps of the build, and the
search becomes a single verify plus a short 1-D tune.

## How the physics shapes the build

- A ship launches straight at its destination (`ShipLaunch.BuildLaunch`), so the unedited path is the
  chord bent by the field. A job fails at start because something on or near the chord bends or blocks
  it.
- `PuzzleSolutionClassifier`: **Clear** = a planet on or near the path shrinks; **Curve** = a planet
  grows. `fixedMass` sources never scale, so a fixed obstacle cannot be cleared (it rules out the
  unwanted kind by construction).

## Units

### U1 — the constructor skeleton

- New `PuzzleConstruct` (Creator domain, plain data in and out, no Unity objects, so it runs off the
  main thread like the fleet search). Input: the brief (`PuzzleBrief`), the seed. Output: a
  `PuzzleData` field plus the intended answer (`PuzzleZone[]`, kind).
- Steps, each its own small type: pick origin and destination (spread and length from the field shape);
  place the sun first with `PuzzleSunPlacementChoice` (edge or centre per contract); place the lesson's
  planets (U2, U3); fill decoys (U1b); verify (U1c).
- **U1b, decoys:** the field generator's placement for the rest of `PlanetCount`, rejecting any spot
  within a corridor margin of the start path and the answered path; on a failed verify, move only the
  decoy nearest the path and retry a few times.
- **U1c, verify:** run the existing `PuzzleJobValidator.Validate` with the intended answer, plus the
  solve window (`PuzzleSolveWindow`) so the answer is not a knife edge. Nothing new decides pass/fail.
- Exit: unit tests for each step on fixed seeds (fast suite where they need no simulation).

### U2 — Clear

- Place the blocker on the chord, a third to two thirds of the way along, sized so its full body
  deflects the start path past the capture radius. The answer is a cube around it; tune its shrink
  scale by bisection between the reachable minimum and 1 until the ship lands, then take the middle of
  the landing range.
- Exit: on 20 seeds of a chapter 1 Clear contract (01_03-like), most construct and pass the validator;
  record the rate here.

### U3 — Curve

- Place a fixed obstacle (`fixedMass`) on the chord, so the straight shot fails and shrinking cannot
  help, and a bend planet beside the chord on the side the route should pass. The answer grows the bend
  planet; tune its scale by bisection and its offset from the chord by a short 1-D sweep.
- Check how a fixed obstacle reads on the headset (it must look like a planet the cube refuses to
  scale; today's refusal pulse may already say that).
- Exit: on 20 seeds of a chapter 1 Curve contract, most construct and pass; rate recorded.

### U4 — wire it in and compare

- `PuzzleJobContract.Generator` (`Search` default, `Construct`). `PuzzleSeedScanner` calls the
  constructor for `Construct`; the result flows through the same export and (after
  [bake answer](../in-progress/bake-answer.md)) the answer asset.
- Time and yield, search vs construct, on two chapter 1 contracts in the bake checkout; write both here.
- Exit: a `Construct` contract bakes through `docs/tools/bake.ps1` with no other change; the numbers
  decide whether chapter 3 and freelance use it by default.
