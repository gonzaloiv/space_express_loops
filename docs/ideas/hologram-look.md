# Hologram Look (Recorded)

**Status:** idea · presentation · updated 2026-10-30 — recorded, design only. Not scheduled.

**Related:** [presentation](../design/presentation.md)

## The idea

Give the game a **hologram** look — tinted, semi-transparent, fresnel-rimmed, faintly scanned — on the
**bubble** first. Same visual language elsewhere later, but the bubble is the cheap place to start.

## Why the bubble is cheap

The **color scope** is already the right shape: `ColorScope.hlsl` applies a per-fragment effect inside
the project's URP-Lit fork, and `UniverseColorScope.cs` drives it from **shader globals** off
`UniverseView.BubbleBlend` (0 outside, 1 in the bubble). A hologram is one more global and one more
branch in that include — a sibling of the grayscale scope, not a new system. The rim shader
(`DigitalLove/VFX/LitRim`) already carries `_RimColor` / `_RimPower` / `_RimIntensity`.

Recipe (fragment level): tint → fresnel rim → scan → optional flicker, multiplied by a global
`strength` (`BubbleBlend * maxStrength`), exactly as the grayscale scope uses it.

## Scope rule

**Never hologram text, UI, TTS, store, keypad, or hands** — shimmer over text is unreadable and
fatiguing in a headset. Apply it to rendered game surfaces only, driven by one global strength.

## Non-goals

- A full-screen post pass (cost, stereo, would tint passthrough).
- A second camera / render texture / depth texture.
- Runtime-created materials, meshes, or hierarchies.

## Phases

1. **Bubble proof** — add the branch, drive it from a test component, verify on a sphere: tint, rim,
   scan, no extra passes, SRP batching intact.
2. **Bubble runtime** — drive it from `BubbleBlend` on the `Universe` root; tune replace-vs-coexist,
   opacity, scan density, rim strength.
3. **Device** — profile GPU both eyes; check edge stability, comfort, and text legibility.

## Open questions

- Replace the grayscale scope inside the bubble, or coexist as a light overlay? (start coexist)
- One tint, or per-context tints?
- Opaque rim-only, or true see-through? (device cost)

## Substrate (all verified present)

`Assets/_Modules/VFX/Materials/Shaders/ColorScope.hlsl` · `LitRim.shader` (+ its forward/input
includes) · `Assets/_Game/Features/Universe/View/UniverseColorScope.cs` (the driver pattern) ·
`UniverseView.cs` (`BubbleBlend`) · `Assets/Settings/Project Configuration/Performant URP Renderer
Config.asset` (no post-processing wired) · `Assets/_Game/Game.unity` (wiring).
