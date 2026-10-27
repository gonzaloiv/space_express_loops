# Hologram Look — Bubble view, then Edition view (Recorded)

**Status:** idea · presentation · updated 2026-10-23 — recorded, design only. Not started, and independent of the current work
streams. This is a self-contained idea captured for future context; nothing here is wired into
other plans or in-progress work.

**Wiring rule:** all visuals stay authored in the scene/prefabs with serialized references. No
runtime `AddComponent`, `GetComponent`, `FindObjectsByType`, or injected references. Shader
globals are the only data channel for the effect, matching the existing scope driver. Follow the
SerializeField wiring rules in the project `AGENTS.md`.

## 1. The idea

Give the game a **hologram** look — tinted, semi-transparent, fresnel-rimmed, faintly scanned —
and apply it in two places, in order:

1. **Bubble view** (the mini map / player-god view of the universe).
2. **Edition view** (the interactive loop-building gameplay).

It is the same visual language in both, but the two are very different in cost and risk, so they
are separate phases with separate gates.

## 2. Verdict up front

| Target | Effort | Risk | Recommendation |
|---|---|---|---|
| Bubble view | Low-medium | Low | Do it. The substrate already exists. |
| Edition view | High | High | Do it **selectively**, not as a whole-frame pass. |

The bubble view is cheap because the project already drives a global, per-fragment effect from the
bubble transition. The edition view is expensive because it is the interactive gameplay, in a
mixed-reality (passthrough) app, on a mobile "Performant" URP config.

## 3. Why the bubble view is cheap

The **color scope** already does exactly the shape of thing a hologram needs:

- `Assets/_Modules/VFX/Materials/Shaders/ColorScope.hlsl` — `ApplyColorScope(color, positionWS)`,
  a per-fragment effect applied inside the forward pass of the project's URP-Lit fork.
- `Assets/_Game/Features/Universe/View/UniverseColorScope.cs` — a driver that writes **shader
  globals** each `LateUpdate` from `UniverseView.BubbleBlend` (0 in Room, 1 in Bubble), so the
  effect fades in/out with the view transition and costs nothing in Room view.
- The look shader is `DigitalLove/VFX/LitRim` (`Assets/_Modules/VFX/Materials/Shaders/LitRim.shader`),
  which already has `_RimColor` / `_RimPower` / `_RimIntensity` to build a hologram rim on.
- Globals keep SRP batching intact — no per-material property, no new `UnityPerMaterial` entry.

A hologram is the **same architecture**: one more global strength, one more branch in the scope
include, one more term in the forward pass. It is a sibling of the grayscale scope, not a new
system.

### 3.1 The hologram recipe (fragment-level)

Inside the scope include, alongside or replacing the desaturation:

```text
hologram(color, positionWS, normalWS, viewDirWS)
  base   = tint(color, _HoloTint)            // push toward a palette (e.g. cyan/teal)
  rim    = pow(1 - saturate(dot(normalWS, viewDirWS)), _HoloRimPower) * _HoloRimIntensity
  scan   = 0.5 + 0.5 * sin(positionWS.y * _HoloScanFreq + _Time.y * _HoloScanSpeed)
  flicker= lerp(1, noise, _HoloFlicker)      // subtle, optional
  alpha  = saturate(base.a * _HoloOpacity + rim)
  out    = lerp(base.rgb + rim, scan, _HoloScanMix) * strength * flicker
```

`strength` is `BubbleBlend * maxStrength`, exactly as the grayscale scope uses it.

### 3.2 Design decision: replace or coexist with the grayscale scope?

Two options; decide when the phase starts.

- **Replace inside the bubble:** the bubble content goes hologram instead of full color, and the
  out-of-frame content stays gray. Reads as "the map is a projection."
- **Coexist:** keep full color inside, add only a light hologram rim/scan overlay. Lower risk,
  keeps art legibility.

Recommendation: start with **coexist** (light overlay), tune toward **replace** only if it reads
better on device.

### 3.3 Transparency and ordering

A hologram wants some see-through. Transparency in the forward pass has real costs and ordering
issues (sorting, overdraw, depth writes). Start **opaque with a strong rim + tint + scan**, add
alpha only if the look demands it. If alpha is added, keep `ZWrite` on where possible and treat it
as a look decision to be verified on device.

## 4. Why the edition view is expensive

- **It is the whole interactive gameplay**, not a framed map: planets, spaceships, store panels,
  keypad, TTS panel, routes, world-space UI. There is no single blend parameter to ride, and no
  natural spatial scope.
- **Legibility / comfort.** Hologram shimmer over text and UI is hard to read and, in a headset,
  fatiguing. Text and store UI must stay clean.
- **Mixed reality.** This is an MR app (MRUK, passthrough, `SelectivePassthrough.shader`). A
  full-frame hologram fights the passthrough background, which is not a rendered surface.
- **Mobile renderer.** `Assets/Settings/Project Configuration/Performant URP Renderer Config.asset`
  is a mobile forward config with `postProcessData: {fileID: 0}`. A **full-screen post pass** is the
  most expensive option here: an extra full-screen read+write per eye, and it would tint the real
  room too.
- **Cost on Quest.** A stereo full-screen blit in a tiled GPU is the thing to avoid.

## 5. Recommended shape for the edition view

Do **not** apply a whole-frame hologram. Scope it to **select objects**:

- The **universe content** (planets, hubs, spaceships) — the game world.
- The **loop routes** and route edges.
- The **ghost preview** (`Assets/_Game/Features/Universe/Gesture/UniverseGhostView.cs`) — a
  hologram ghost is a natural fit and probably the best first candidate.
- Leave **UI panels, TTS, store, keypad, and hands** on stock shaders and fully legible.

Mechanically this is the same recipe as the bubble, driven by a **separate strength global** that
is on during edition and off elsewhere, rather than a full-screen pass. Objects that should not be
holograms simply keep a shader that does not include the hologram branch.

## 6. Non-goals

- A full-screen post-process pass (rejected: cost, stereo, would affect passthrough).
- A second camera, render texture, or depth texture.
- Hologram over text, UI, store, or hands.
- New runtime-created materials, meshes, or hierarchies.
- Changing gameplay, spawning, or the round/economy flow.

## 7. Runtime shape (if built)

Reuse the existing pattern; do not invent a new channel.

- A small driver MonoBehaviour that writes the hologram globals each `LateUpdate`:
  `_HoloStrength`, `_HoloTint`, `_HoloRimPower`, `_HoloRimIntensity`, `_HoloScanFreq`,
  `_HoloScanSpeed`, `_HoloScanMix`, `_HoloOpacity`.
- Cache every `Shader.PropertyToID`; no per-frame allocations, renderer loops, or material access.
- `OnDisable` sets `_HoloStrength` to `0`.
- Bubble driver reads `UniverseView.BubbleBlend` (as the grayscale scope does). Edition driver is
  driven by the edition state / a serialized strength.
- No new interpolators needed: `LitRim`'s forward pass already carries `positionWS` and the
  normals/view direction needed for a rim.

## 8. Phases (proposed)

### Phase 0 — Bubble hologram proof
- Add the hologram branch to the scope include (or a sibling include) and to the `LitRim` forward
  pass, gated by `_HoloStrength`.
- Drive globals from a temporary inspector/test component.
- Verify on a test sphere: tint, rim, scan; no extra passes; SRP batching intact.

**Exit:** bubble content reads as a hologram; Room view unchanged; no extra passes.

### Phase 1 — Bubble runtime
- Add a driver reading `BubbleBlend`; wire it on the `Universe` root in `Game.unity`.
- Verify Room -> Bubble -> Room fades the hologram in/out with no pop.
- Tune: replace vs coexist, opacity, scan density, rim strength.

**Exit:** the bubble view looks holographic and fades with the transition; Room view is untouched.

### Phase 2 — Edition view, scoped (only if Phase 1 lands well)
- Add a second, independent strength global for edition.
- Apply only to the chosen objects (universe content, routes, ghost preview).
- Verify UI, TTS, store, keypad, and hands are unaffected and fully legible.

**Exit:** the game world reads as a hologram during edition; UI stays clean and readable.

### Phase 3 — Device verification
- Profile GPU time both eyes, single-pass instanced, in Room/Bubble/edition.
- Check edge stability in motion, comfort, and text legibility.

## 9. Test plan

1. Room view: no hologram; everything normal.
2. Bubble view: bubble content reads as a hologram; fades in with the transition.
3. Bubble -> Room: the hologram fades out; full normal look returns.
4. Instant bubble transitions snap the strength with no stale frame.
5. Disabling the driver restores the normal look immediately.
6. Room props / out-of-frame content behave as designed (gray, or normal — per the §3.2 decision).
7. Frame Debugger: no extra passes; draw calls unchanged; SRP Batcher active.
8. Edition view (if built): only the scoped objects are holograms; UI, TTS, store, keypad, hands
   are clean.
9. Text legibility check in a headset during edition.
10. Headset: stable frame time in Room, Bubble, and edition.
11. Passthrough unaffected (the real room is never tinted).

## 10. Risks and decisions

| Decision / risk | Choice / mitigation |
|---|---|
| Cost on Quest | Per-fragment ALU only; no blit, no resolve, no bandwidth. |
| Full-screen post | Rejected: cost, stereo, would tint passthrough. |
| Transparency cost | Start opaque with rim + tint + scan; add alpha only if the look needs it. |
| Replace vs coexist | Start coexist (light overlay); tune toward replace on device. |
| Text/UI legibility | Never hologram UI, TTS, store, keypad, or hands. |
| Edition scope | Object-scoped (content, routes, ghost), not whole-frame. |
| Shader fork | Reuse `LitRim` + the scope include; pin URP version and re-apply on upgrade. |
| SRP batching | All values are globals; no new CBUFFER entry. Verify. |
| Comfort | Subtle flicker/scan only; verify in a headset. |
| Passthrough | Untouched; only rendered surfaces change. |

## 11. Open questions

1. **Replace or coexist** inside the bubble (§3.2)?
2. **Palette** — one tint, or per-context tints (map vs edition)?
3. **Transparency** — opaque rim-only, or true see-through? (device cost)
4. **Edition first target** — the ghost preview alone, or the whole universe content?
5. **Interaction with the grayscale scope** — does the hologram sit before or after the
   desaturation in the fragment chain?

## 12. Files (for whoever picks this up)

Substrate that already exists:

- `Assets/_Modules/VFX/Materials/Shaders/ColorScope.hlsl` — the scope include; add the hologram
  branch here or in a sibling include.
- `Assets/_Modules/VFX/Materials/Shaders/LitRim.shader` + `LitRimForwardPass.hlsl` +
  `LitRimInput.hlsl` — the URP-Lit fork that applies the scope; the hologram goes in its forward
  pass.
- `Assets/_Game/Features/Universe/View/UniverseColorScope.cs` — the driver pattern to copy.
- `Assets/_Game/Features/Universe/View/UniverseView.cs` — `BubbleBlend`, `FrameCenterWorld`,
  `FrameRotation`.
- `Assets/_Game/Features/Universe/Gesture/UniverseGhostView.cs` — natural edition-view candidate.
- `Assets/Settings/Project Configuration/Performant URP Renderer Config.asset` — the mobile config;
  note `postProcessData: {fileID: 0}` (no post-processing wired).
- `Assets/_Game/Game.unity` — where the drivers would be wired.
