# Universe — Bubble Color Scope

**Status:** done - implemented as a **box** mask (`sdBox`, to match the cube view frame) inside
the existing `LitRim` URP-Lit fork, not a new shader and not a sphere. Shader include,
`UniverseView.BubbleBlend`, `UniverseColorScope` (box sized from the content bounds, shrunk by
`boxScale`), world + room-prop materials on `DigitalLove/VFX/LitRim`, and the world-space UI
fader (`UniverseScopeUiVisibility`, four level roots) are all done, wired in `Game.unity`, and
verified on device.

**Related:** [Two views + interaction unify](universe-views-and-interaction.md) · [Transforming the universe](universe-transform.md)

**Wiring rule:** the scope driver and UI faders are authored components with serialized references. No runtime `AddComponent`, `GetComponent`, `FindObjectsByType`, or injected references. Shader globals are the only data channel.

## 1. Decisions from review

| Topic | Decision |
|---|---|
| Gameplay colors | Desaturate routes/ships/planets too. The focus inside the bubble wins over global color information. |
| UI | World-space UI outside the zone is hidden. Screen-space HUD stays. Outlines are white and need no change. |
| Passthrough | Not desaturated. Revisit after the first headset test. |
| Hands | Not part of the game world; leave hands on stock shaders. |
| Room scope | Future feature only. v1 is bubble-only. |
| Number of bubbles | One. The bubble is the player-god view of the single universe. |
| Shader size | One custom shader, as close to default URP Lit as possible. |
| Edge + particles | Use `fwidth` for the edge. If particles join the effect later, use alpha-based particles. |

## 2. Goal

In **Bubble view**, rendered world surfaces inside the bubble sphere are fully colored; everything rendered outside the sphere is desaturated to black and white with a soft edge. The effect fades in/out with the view transition.

In **Room view** nothing changes for now: the whole world stays colored. The room-volume version is a future extension.

The real room is passthrough, so its pixels are not part of this effect. Only rendered surfaces change.

## 3. Non-goals

- Room-volume (MRUK room faces) color scope; future.
- Desaturating passthrough or the real room.
- Multiple simultaneous bubbles.
- Full-screen post-process, second camera, stencil, render texture, or depth texture.
- Migrating unlit/VFX/particle shaders in v1. They are inside the bubble with the content, so they stay colored; phase 4 covers stragglers.
- A literal single material asset for every object (see §7.3).
- Gameplay or spawn changes.

## 4. Chosen approach

Apply a per-fragment sphere mask in the forward pass of one custom Lit shader:

```text
world position
  -> bubble sphere SDF
  -> soft edge with fwidth
  -> lerp(final color, luminance, 1 - mask)
  -> scaled by BubbleBlend
```

| Cost | This approach | Full-screen post |
|---|---|---|
| Extra render passes | 0 | 1 blit + resolve |
| Extra texture bandwidth | 0 | full frame read + write |
| Extra draw calls | 0 | 0-1 |
| Per-pixel work | ~20 ALU on world surfaces | full-screen fetch + ALU |
| MSAA/intermediate texture | unchanged | usually forces a resolve |
| Passthrough | untouched | would desaturate the real room too |

All scope values are globals, so there is no per-material property and no new `UnityPerMaterial` CBUFFER entry. SRP batching stays intact.

## 5. Scope volume and mask

### 5.1 Sphere

```text
center (world) = contentRoot.TransformPoint(contentCenter)
radius (world) = ContentRadius * Scale
```

The center is the **fixed player-view center where the Bubble spawned**, not the camera. `UniverseView` exposes it as `CenterWorld`, and the transform plan uses the same point as the Room reference, so the visual focus and the scale target agree.

In the settled bubble state the radius equals `miniBubbleRadius`; during the transition it shrinks with the content.

### 5.2 Mask

Shared include `ColorScope.hlsl`:

```hlsl
half3 ApplyColorScope(half3 color, float3 positionWS)
{
    if (_ColorScopeStrength <= 0.0)
        return color;

    float d = distance(positionWS, _ColorScopeCenter.xyz) - _ColorScopeRadius;
    float aa = max(fwidth(d), 0.0001);
    float softness = max(_ColorScopeSoftness, aa);
    float mask = 1.0 - smoothstep(-softness, softness, d);
    float keep = lerp(1.0, mask, saturate(_ColorScopeStrength));

    half gray = dot(color, half3(0.299, 0.587, 0.114));
    return lerp(half3(gray, gray, gray), color, keep);
}
```

- `_ColorScopeStrength` is `BubbleBlend`, so room view is a no-op and the transition fades the effect in.
- `fwidth` keeps the edge stable while the bubble or camera moves.
- `_ColorScopeSoftness` defaults to `0.03 m`; at the default `~0.26 m` bubble radius this is a visible but tight edge.
- Rec.601 matches the existing `DigitalLove/Unlit/Grayscale` shader.

### 5.3 Requirement: universe elements outside the frame stay visible and gray out

The scope changes **rendered surfaces outside the frame**. This is a required effect, not an
optional extra: **planets and other universe elements that sit outside the frame must stay
spawned and rendered, and the scope grays them** while the frame interior stays colored.
That is the acceptance criterion.

Consequences:

- **The scope box is sized from the content bounds, shrunk by `boxScale`.** It no longer depends
  on `viewRadius`, so the gray band exists at rest without needing the universe to be larger
  than the frame: the box is `0.8 x content radius`, so the outer shell of the map is already
  outside and grays. More content outside the box (a thicker band, more systems in view) is a
  content-volume question for the sector-field work, not a blocker for this plan.
- **Out-of-frame elements must not be hidden.** The silent-planet bubble-frame hide
  (`bubbleFrameMargin` in `SilentPlanetsSpawner`) was removed for this reason: silent planets
  now stay visible in Bubble view and the scope grays them. Frame culling is otherwise not
  implemented (the frame owns containment math only). Any future culling must skip elements
  that should gray instead of vanish.
- **No shader work is needed for them.** World materials already point at the scope shader,
  so out-of-frame planets gray as soon as they sit outside the box.
- Room props and transition frames also gray, but they are not the requirement.

## 6. Runtime

### 6.1 `UniverseView` additions

Read-only surface; no behavior change:

```text
FrameCenterWorld   = the world point the frame center lands on (content center at rest)
ContentRadiusWorld = content bounds radius * Scale   // the scope box half-size
BubbleRadiusWorld  = viewRadius * Scale              // the frame window (marker, drag limit)
BubbleBlend        = 0 in Room, 1 in Bubble, tweened over transitionSecs
```

`ContentRadiusWorld` is fed from the content bounds (`LevelContentCenter` measures the max
distance from the content center to any hub/planet and calls `UniverseView.SetContentRadius`),
falling back to `viewRadius` before content exists.

**Why the scope uses `ContentRadiusWorld`, not `BubbleRadiusWorld`:** `BubbleRadiusWorld`
always equals `miniBubbleRadius` (`viewRadius * MiniScale`), while the map's world radius is
`contentRadius * MiniScale`. With `viewRadius (1.2) > contentRadius`, the box was larger than
the visible map, so graying only triggered once a drag pushed a planet past the box edge — and
the drag is clamped to `BubbleRadiusWorld * 0.3`, so that only happened at full drag. Sizing the
box from the content bounds makes the box edge coincide with the map edge, so a planet grays as
soon as it leaves the map.

`BubbleBlend` is tweened in `StartTransition` and snapped in `CompleteTransition` and on instant transitions.

### 6.2 `UniverseColorScope`

Serialized `MonoBehaviour` on the `Universe` root:

```text
[SerializeField] UniverseView universeView
[SerializeField] float softnessMeters = 0.03
[SerializeField] float boxScale = 0.8
[SerializeField] float maxStrength = 1
```

`boxScale` shrinks the scope box inside the content bounds, so the gray-out and the UI hide
trigger nearer the center of the cube; the UI fader reads the same `_ColorScopeHalfSize`, so it
follows automatically.

`LateUpdate` (after DOTween moves the content):

```text
strength = universeView.State.HasRoomAnchor ? universeView.BubbleBlend * maxStrength : 0

Shader.SetGlobalVector(_ColorScopeCenter,   universeView.FrameCenterWorld)
Shader.SetGlobalVector(_ColorScopeHalfSize, Vector3.one * (universeView.ContentRadiusWorld * boxScale))
Shader.SetGlobalVector(_ColorScopeRotation, universeView.FrameRotation)
Shader.SetGlobalFloat (_ColorScopeSoftness, softnessMeters)
Shader.SetGlobalFloat (_ColorScopeStrength, strength)
```

- cache every `Shader.PropertyToID`;
- `OnDisable` sets `_ColorScopeStrength` to `0`;
- no per-frame allocations, renderer loops, or material access.

Wiring: add the component to the `Universe` root in `Game.unity` and serialize the existing `UniverseView` (fileID `242500899`).

### 6.3 World-space UI

`UniverseScopeUiVisibility` (`Assets/_Modules/UI/Behaviours/UniverseScopeUiVisibility.cs`) is implemented; it still needs to be added to world-space UI roots that should disappear outside the frame (letters panels, store, TTS, posters, world-space buttons). It only reads shader globals, so it has no asmdef dependency on `_Game`.

- serialized `CanvasGroup`;
- reads the same five shader globals the driver writes (`_ColorScopeCenter`, `_ColorScopeHalfSize`, `_ColorScopeRotation`, `_ColorScopeSoftness`, `_ColorScopeStrength`) - no scope reference, so spawned UI prefabs work;
- if `_ColorScopeStrength` is 0, alpha is 1;
- otherwise computes the same **box** mask (`sdBox` with `_ColorScopeRotation`, matching `ColorScope.hlsl`) from its own `transform.position`;
- keeps the panel visible while the mask is above `0.65`, hides it below `0.35` (hysteresis), and fades `CanvasGroup.alpha` over a short `fadeSecs`;
- `CanvasGroup.blocksRaycasts` follows the visible state;
- do not disable the GameObject; gameplay state stays intact.

Screen-space HUD is not world-positioned and stays visible. Hands and UI use stock shaders and are never desaturated. QuickOutline is white, so it already fits the black-and-white look.

## 7. Shader

### 7.1 One shader

Create one project copy, based on default URP Lit:

- `ColorScope/Lit.shader` (copy of URP `Lit.shader`)
- `ColorScope/LitForwardPass.hlsl` (copy of URP `LitForwardPass.hlsl`)

Changes:

- point the copied shader's forward include at the copied pass;
- add `#include "ColorScope.hlsl"` to the copied forward pass;
- at the end of `LitPassFragment`, after fog and `OutputAlpha`, apply:

```hlsl
color.rgb = ApplyColorScope(color.rgb, input.positionWS);
```

- keep every other pass (ShadowCaster, DepthOnly, DepthNormals, Meta, MotionVectors) pointing at the package includes;
- do not apply the scope in shadow, depth, depth-normals, meta, or motion-vector passes;
- never edit `Library/PackageCache`.

`LitPassFragment` already has `input.positionWS`, so no new interpolator is needed.

Shader name: `DigitalLove/ColorScope/Lit`.

### 7.2 Unlit, particles, custom VFX

Leave them on stock shaders in v1. They travel inside the bubble with the content, so they should stay colored there. If a straggler is ever visible outside the sphere, phase 4 adds a small custom Unlit/Particle shader using the same include. Do not fork the large URP Unlit/Particles shaders for this.

When particles join the effect, switch the relevant particle materials to alpha blending; additive particles would dim when gray. Review trail/star/hit/death materials at that point.

### 7.3 "One material" meaning

This plan uses **one shader** shared by all world material assets. Each object keeps its own material asset for its base map, color, detail map, and keywords; switching the shader preserves those properties.

A literal single material asset for all objects would require per-renderer `MaterialPropertyBlock` overrides or a texture atlas, break batching, and lose the current art workflow. That is not part of this plan.

## 8. Material migration

Migrate all **world URP Lit** materials to `DigitalLove/ColorScope/Lit`:

- planets, including gradient/detail variants;
- spaceship, handles, station, indicator, route edges;
- room props and interactables that are part of the game world (radio, keypad, decor);
- anything else using `Universal Render Pipeline/Lit` outside UI and hands.

Keep on stock shaders:

- TMP/UI, screen-space HUD;
- `OculusRealisticHandLight/Dark.mat`;
- invisible occluders and the EffectMesh material;
- Unlit lines and particles (v1);
- custom VFX shaders that are not world surfaces.

The known `_Game` set is about 12 Lit materials; the room props add more. Inventory by shader GUID, switch the shader, and verify the properties survived. Planet gradient materials use `_DETAIL_MULX2`, which the copied Lit shader preserves.

If materials live in Addressables, rebuild those bundles after the shader swap.

## 9. Performance

- No additional passes, cameras, render textures, depth textures, or stencil.
- Same draw calls and texture bandwidth; only ALU on world surfaces.
- `fwidth` adds a derivative; still cheap.
- `_ColorScopeStrength <= 0` early-outs in Room view.
- Budget: measure on the target headset; expected well under `0.5 ms` at the current 1.6 render scale. Check both eyes and single-pass instanced.
- Verify SRP Batcher and draw calls in the Frame Debugger.

## 10. Phases

### Phase 0 - Shader proof

- Add `ColorScope.hlsl`.
- Fork only `Lit` and its forward pass.
- Repoint one planet material.
- Drive the globals from a temporary inspector/test component.
- Verify the gradient across a planet with a test sphere.
- No scene wiring.

**Exit:** inside the sphere is unchanged; outside fades to gray over the soft edge; no extra passes.

### Phase 1 - Bubble scope runtime

- Add `BubbleCenterWorld`, `BubbleRadiusWorld`, and `BubbleBlend` to `UniverseView`.
- Add `UniverseColorScope` and wire it on the `Universe` root.
- Verify Room → Bubble → Room: effect fades in/out, no per-object pop.
- Room view stays fully colored.

**Exit:** in Bubble view, rendered world content outside the bubble is gray; the bubble content is colored; Room view is unchanged.

### Phase 2 - World content and UI

- Migrate all world URP Lit materials.
- Add `UniverseScopeUiVisibility` to world-space UI roots.
- Verify props outside the bubble gray out and their UI hides.
- Verify hands, screen-space HUD, and white outlines are unaffected.

### Phase 3 - Tuning and device verification

- Tune softness and edge width with `fwidth`.
- Verify edge stability in motion and at both eyes.
- Profile GPU time, SRP Batcher, draw calls, and shader variants.

### Phase 4 - Future extensions

- Room-volume scope using MRUK room bounds (AABB first, wall planes if needed).
- Unlit/VFX/particle scope with alpha-based particles.
- Passthrough desaturation only if the headset test says it is needed; that requires a full-screen pass.

## 11. Test plan

1. Room view: everything stays colored; no effect.
2. Bubble view: rendered content inside the sphere is colored; content outside is gray.
3. A planet/prop crossing the sphere edge fades across its surface, not as a whole.
4. Room → Bubble: the effect fades in with the transition; no pop.
5. Bubble → Room: the effect fades out; full color returns.
6. Instant transitions snap `BubbleBlend` without a stale sphere.
7. World-space UI outside the sphere hides; UI inside stays visible.
8. Screen-space HUD, hands, and QuickOutline are unaffected.
9. Planet gradient/detail materials look unchanged inside the sphere.
10. Shadows, depth, and depth-normals are unaffected.
11. Disabling `UniverseColorScope` restores full color immediately.
12. Before the room anchor is ready, no desaturation is applied.
13. Frame Debugger: no extra passes; draw calls unchanged; SRP Batcher active.
14. Headset: stable frame time in Room and Bubble.
15. Extent requirement: with planets/universe elements outside the frame, confirm they stay visible and gray out; the frame interior stays colored.

## 12. Risks and decisions

| Decision / risk | Choice / mitigation |
|---|---|
| Cost on Quest | Per-fragment ALU only; no blit/resolve/bandwidth. Cheapest option. |
| Hard per-object boundary | Per-fragment sphere SDF with `fwidth` and smoothstep. |
| Content outside the frame | Required: universe elements outside the frame stay rendered and gray out (§5.3). Needs content that extends beyond `viewRadius`; no culling may hide them. |
| One shader vs one material asset | One shader, many material assets. A literal single material asset needs MPBs/atlases and is not planned. |
| Stock URP Lit fork | One shader + one forward-pass copy. Pin URP version and re-apply on upgrade. |
| Unlit/particles | Stock in v1; they are inside the bubble. Add small custom variants later only if stragglers appear. |
| Additive particles | Use alpha-based particles when they join the effect; additive would dim when gray. |
| UI hiding | `CanvasGroup` fader with hysteresis; no GameObject toggling. World-space only. |
| UI prefab references | Read shader globals instead of serializing a scope reference; works for spawned panels. |
| Passthrough | Not desaturated. Decide after the headset test; full-screen post if needed. |
| Multiple bubbles | One scope only; player-god view. |
| Scope center | Bubble view center (`BubbleCenterWorld`), not the camera; shared with the transform Room anchor. |
| SRP Batcher | All scope values are globals; no new CBUFFER entry. Verify. |
| Addressables | Rebuild material bundles after the shader swap. |
| L-shaped room (future) | AABB first; wall planes only if the room scope is implemented and the corners show. |
| Shader variants | One forked shader; verify no unexpected variant explosion. |

## 13. Defaults

| Parameter | Default |
|---|---|
| `softnessMeters` | `0.03 m` |
| `boxScale` | `0.8` |
| `maxStrength` | `1` |
| Edge AA | `fwidth`, minimum `0.0001` |
| Luminance | Rec.601 |
| UI show/hide thresholds | `0.65` / `0.35` |
| Bubble blend | Tweened over `UniverseView.transitionSecs` |

## 14. Files

Done:

- `Assets/_Modules/VFX/Materials/Shaders/ColorScope.hlsl` - shared scope include.
- `Assets/_Modules/VFX/Materials/Shaders/LitRim.shader` + `LitRimForwardPass.hlsl` + `LitRimInput.hlsl` - the URP-Lit fork that applies the scope (the fork is `LitRim`, not a separate `ColorScope/Lit`).
- `Assets/_Game/Features/Universe/View/UniverseColorScope.cs` - the driver (`FrameCenterWorld`, `ContentRadiusWorld`, `FrameRotation`, `BubbleBlend`).
- `Assets/_Game/Features/Universe/View/UniverseScopeUiVisibility.cs` (`_Modules/UI/Behaviours`) - the UI fader, wired with a `CanvasGroup` on the four **level** world-space roots in `Game.unity`: `Level/Hubs/Panel - Store/Canvas`, `Level/Planets/Planet (1)/Panel - Store/Canvas`, `Level/Spaceships/Spaceship (1)/Traveller/Panel/Canvas`, `Level/Spaceships/Spaceship (1)/RoutePanel/Canvas`. Scene-level panels (`TTSPanel`, `HighestScorePoster`, `ReviewPanel`, `GameEndPanel`) are deliberately left alone — only level elements are in scope.
- `Assets/_Game/Game.unity` - `UniverseColorScope` wired on the `Universe` root; world + room-prop materials repointed to `DigitalLove/VFX/LitRim`.

## 15. Future

- **Extent coupling.** When the sector-field extent lands
  ([sector generation](../in-progress/universe-sector-generation.md): `UniverseBounds`,
  `UniverseFocus`, `fieldRadius`), the universe becomes genuinely larger than the window. At
  that point the scope box should be re-derived from the **frame window** (`viewRadius`) instead
  of `ContentRadiusWorld * boxScale` - the content-bounds sizing exists only because the map was
  smaller than the window. Revisit §5.3/§6.2 then.
- Room-volume scope (MRUK bounds, then wall planes).
- Unlit/VFX/particle scope with alpha-based particles.
- Passthrough desaturation via a full-screen pass if the test requires it.
- Multiple scopes only if the game ever shows more than one bubble.

## 16. Next steps (ordered)

1. **See the desaturation (goal 1).** Done - confirmed in the editor: out-of-frame content grays
   and the frame interior stays colored, after the box was sized from the content bounds and
   `boxScale` was added to pull the boundary inward.
2. **World-space UI hiding.** Done: `UniverseScopeUiVisibility` is wired with a `CanvasGroup` on
   the four level world-space roots only (see §14); scene-level panels (TTS, poster, review,
   end panel) are left alone. Watch the interaction with `ScaleOnOff` / `ScalePunch` on those
   canvases, which already animate scale for show/hide.
3. **Tuning and device verification.** Done - tuned and checked on device (edge reads as a fade,
   both eyes, motion stable). Re-run the §11 list if `boxScale` or `softnessMeters` change again.
4. **More universe outside the box** (optional, not a gate). The band currently exists because
   `boxScale` shrinks the box inside the content bounds. A thicker band and more systems in view
   is a content-volume question for the sector-field work
   ([sector generation](../in-progress/universe-sector-generation.md),
   [sector field hub](../backlog/universe/universe-infinite-sector.md)).

