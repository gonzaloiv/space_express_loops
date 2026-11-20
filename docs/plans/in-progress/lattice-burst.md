---
status: active
artifact_readiness: implementation-ready
execution: code
---

# Lattice on Burst

**Status:** in progress (2026-11-20).

## Goal

Meta rejected the build on VRC.Quest.Performance.1: the frame rate falls under 60 while the player
scales the cube. The headset profiler puts it on one marker: `Shaping.Lattice.Warp` costs ~19 ms per
lattice tick (30 Hz) against a 13.9 ms frame. Move the lattice warp onto Burst jobs so a tick costs
~1–2 ms and the pinch holds 72 Hz.

## Contract

- The lattice looks the same: same zone warp, wells, smoothing, strain and fade.
- The warp stays one piece of math shared by the lattice, the dust and the tests (`SpaceZone`,
  `WellPull`, `LatticeWarp`, `ContentFade`); Burst runs the same code, no shader copy.
- No per-tick managed allocations from the lattice; native buffers are owned and disposed by their view.

## Units

1. **Native field math.** `WellPull`, `LatticeWarp` and `ContentFade` read `NativeArray<T>.ReadOnly`
   instead of managed lists; tests updated. Exit: fast EditMode suite green.
2. **Burst lattice mesh.** `SpacetimeMeshView` takes an `ILatticeField` and runs rest → warp →
   smooth + strain → fade → ring write → bounds as Burst jobs into native vertex/colour buffers.
   Exit: compiles; the field preview draws the same lattice.
3. **Lattice field.** `LatticeField` (Views) fills native zones/sources/baseline/spheres from the
   session and schedules the Burst warp and fade jobs; `SpaceShapingView` owns and disposes it.
   Exit: compiles, tests green.
4. **Headset check.** Dev build, pinch on the heaviest job: `Shaping.Lattice.Warp` under 2 ms and no
   frame over 16.6 ms during the pinch.
