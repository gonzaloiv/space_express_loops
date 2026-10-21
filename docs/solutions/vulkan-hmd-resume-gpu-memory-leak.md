---
title: Vulkan retains ~1.5 GB of GPU memory per HMD sleep/wake until LMK kills the app
date: 2026-10-21
category: performance-issues
module: XR/URP
problem_type: performance_issue
component: development_workflow
symptoms:
  - "FPS collapses after taking the headset off and putting it back on"
  - "Room/anchor re-localization glitches, then the app drops to the Meta menu a few seconds later"
  - "No managed exception; process death is SIGKILL"
  - "Quest 3S dumpsys shows GL mtrack climbing ~1.0-1.6 GB per resume"
root_cause: wrong_api
resolution_type: workaround
severity: high
tags:
  - vulkan
  - opengles3
  - adreno
  - quest
  - gpu-memory
  - swapchain
  - application-pause
  - low-memory-killer
  - occlusion
  - selective-passthrough
---

# Vulkan retains ~1.5 GB of GPU memory per HMD sleep/wake until LMK kills the app

## Problem

Taking the headset off (proximity sensor → `HMDUnmounted` → app pause) and putting it back on caused
a frame-rate collapse, visible room/anchor glitching, and then the app being killed to the Meta menu a
few seconds later.

The FPS drop was **not** a rendering-performance regression. It was the visible stage of memory
pressure that ended in an OS kill:

- Vulkan on the Adreno GPU retained roughly **1.0-1.6 GB of GPU memory per sleep/wake cycle**.
- `GL mtrack` climbed from a ~1.9 GB baseline to a ~5.1 GB plateau.
- Android's low-memory killer began direct reclaim / swap thrashing (`am_low_memory`, lmkd
  `direct reclaim and thrashing`), which is the stutter, then killed the process:
  `ApplicationExitInfo reason=2 (SIGNALED) status=9` (SIGKILL). No `FATAL EXCEPTION`, empty crash
  buffer.

So the sequence is: GPU memory step on resume → system thrash → FPS collapse → LMK kill.

## Symptoms

- Stable FPS until the first unmount/remount, then progressively worse with each cycle.
- Focus loss through the Quest system menu is fine (HMD stays worn); only the headset-off path
  triggers it.
- Logcat around remount shows `TrackingLost`, repeated `OnAppSpaceChange`, `Recenter event detected`,
  repeated `RenderTexture::Create`, and anchor re-localization spam before the app dies.
- Death reason is always SIGKILL under memory pressure, never a script error.

## Evidence

Measured from `dumpsys meminfo` and `ApplicationExitInfo` on a Quest 3S
(Unity 6000.6.0f1, OpenXR 1.18.0, Meta XR Core SDK):

| Build | Baseline `GL mtrack` | Per-resume behaviour | Result |
| --- | --- | --- | --- |
| Vulkan (default) | ~1.94 GB, PSS ~2.60 GB | +1.0-1.6 GB per cycle | killed at ~5.1 GB PSS |
| Vulkan + dynamic resolution off | 4.59 GB | higher floor | worse, reverted |
| GLES3 | ~1.44 GB, PSS ~2.25 GB | flat, no step over 3+ cycles | held at ~2.3-3.0 GB |

Ruled out on Vulkan with on-device measurement, not guesses:

- Dynamic resolution (`_enableDynamicResolution`) — disabling it made GPU memory ~2.4x worse because
  the URP asset's `m_RenderScale: 1.5` stopped being downscaled.
- MSAA / render scale (4 / 1.5) — a smaller render target shrank the step but did not remove it.
- Fixed foveation (`MetaXRFoveationFeature`, `XR_FB_foveation_vulkan`) — disabled in a build verified
  by an in-app probe (`gfx=Vulkan foveation=Off`); the step persisted.
- OpenXR plugin version — project is on 1.18.0, past the 1.16.1 offscreen-rendering fix.
- URP render graph extras — no SSAO/TAA in the renderer; only `ARBackgroundRendererFeature`.
- GPU Resident Drawer (`m_GPUResidentDrawerMode: 0`), audio middleware (no FMOD/Wwise), HDR (off).
- Focus/pause handling — `OVRSessionHelper` only fired analytics and `hasUnpaused` had no subscribers;
  hardening it changed nothing.
- Room-mesh churn — a transient `MRUKUtil` guard that called `EffectMesh.DestroyMesh()` on unmount and
  `CreateMesh()` on mount bought **exactly one extra cycle** (it freed CPU-side room geometry, not GPU
  memory), then the app was killed again. Reverted.
- Recenter/app-space churn, dev vs release builds.

The only configuration that held flat was **OpenGLES3**, which isolates the retention to the Vulkan
swapchain path (the Adreno driver failing to reclaim the old swapchain surface across suspend/resume).

## Solution

Switch the Android graphics API from Vulkan to OpenGLES3 and fix the one thing GLES3 broke.

1. `ProjectSettings/ProjectSettings.asset` → `m_BuildTargetGraphicsAPIs` → `AndroidPlayer`:
   `m_APIs: 0b000000i` (OpenGLES3) instead of `15000000i` (Vulkan), `m_Automatic: 0`.
   Commit `7ba9ebbe`.
2. The occluder broke under GLES3. It was a built-in-pipeline `CGPROGRAM` shader
   (`OcclusionCutoutURP` from `com.meta.xr.depthapi.urp`) whose effect lives in the framebuffer alpha
   with premultiplied-alpha stereo — a path where GLES3 and Vulkan differ. Port it to a URP HLSL shader
   with the same blend/`ZTest` contract (`DigitalLove/SelectivePassthrough`, in
   `Assets/_Modules/XR/Prefabs/MRUK/EffectMesh/Occlusion/SelectivePassthrough.shader`), repoint the
   occluder material, and move `VHSTape` to URP Lit. The Meta depth-API URP shader package was removed.
3. Keep URP quality as authored: `m_MSAA: 4`, `m_RenderScale: 1.5`, 4K main-light shadows.
   See `urp-quest-gpu-memory-bloat.md` — do not "fix" memory by cutting these.

## Cost of the workaround

- GLES3 is legacy on Quest. Higher CPU overhead per draw call, no Vulkan-only paths (notably
  `XR_FB_foveation_vulkan`, so foveation is inert and fill-rate/thermal headroom is lower), and Meta
  steers toward Vulkan.
- Long-term options: device/driver-gate Vulkan with a GLES3 fallback, or file the engine/driver bug
  (Quest 3S, Adreno, Unity 6000.6.0f1, OpenXR 1.18.0, Vulkan: `GL mtrack` +1.0-1.6 GB per wake, bounded
  ~5.1 GB plateau, LMK kill, absent on GLES3).

## How to verify

```powershell
adb -s <device> shell dumpsys meminfo com.digitallove.marble | Select-String "GL mtrack|TOTAL"
adb -s <device> shell dumpsys activity exit-info com.digitallove.marble
adb -s <device> logcat -d -v time | Select-String "MarbleDiag|OnAppSpaceChange|RenderTexture::Create"
```

Do 3 sleep/wake cycles. GLES3 should keep `GL mtrack` and total PSS flat; a per-resume step means the
build is not actually GLES3 (check Project Settings → Player → Android → Graphics APIs) or a second
leak was introduced.

## Prevention

- When FPS degrades only after pause/resume in VR, check `dumpsys meminfo` / `ApplicationExitInfo`
  before touching rendering settings. An LMK kill looks like a crash and is not one.
- Do not trust a "fix" that only shifts the cycle count; measure `GL mtrack` across at least three
  cycles.
- Attribute one change at a time. Several rounds here were confounded by a quality change landing
  alongside an API change (and by settings assets re-serializing back to their old values).
- The Android graphics API is a build-defining setting; changing it requires a fresh build, and GLES3
  compatibility of custom shaders must be checked.
