# Space Shaping — Campaign Map

**Status:** next · space shaping · updated 2026-10-29 — the cube map over the sector field: pick a job,
play its brief in the same Bubble view, then return to the map. Part of validation 2 (with the
[HUD plan](space-shaping-hud-validation.md)); after the [puzzle boots](../in-progress/space-shaping-in-game.md) and before
[progression](space-shaping-progression.md).

**Related:** [in the game](../in-progress/space-shaping-in-game.md) · [HUD + validation 2](space-shaping-hud-validation.md) ·
[progression](space-shaping-progression.md) · [legacy cut](space-shaping-legacy-cut.md) ·
[catalogue](space-shaping-catalogue.md) · [variations](../../ideas/variations.md) ·
[progression](../../design/systems/progression.md) · [modes](../../../Assets/_Modules/docs/standards/modes.md)

**Wiring rule:** cubes, the field and the views are authored in `Game.unity` with serialized refs. One
hand-placed cube per job — no generated cube positions, no runtime hierarchy creation.

## 1. Context

The campaign map is a **field of cubes** over the bounded sector field; the field is the map substrate, not
scenery. This plan lands the map and the handoff, and moves the keepers out of the legacy level container so
the later cut is deletion only.

## 2. Slices

### M1 — Carve the keepers out

- `SectorFieldSpawner` spawn/reset and the bubble-placement restore leave `LevelContainer` (today
  `LevelContainer.cs:16` owns the spawner, the placement restore and the room pose) and move to a campaign
  owner: `CampaignView` + `PlacementStore`, both authored in `Game.unity`.
- `UniverseView` stays and is not deleted with the level container. `GameStartState`'s mid-level resume
  path is no longer used by boot.
- The legacy flow code stays in the project, unreachable, until the [cut](space-shaping-legacy-cut.md).
- **Exit:** the campaign owner spawns and resets the sector field and restores bubble offset/yaw; boot no
  longer needs `LevelContainer` or `GameStartState`.

### M2 — Cube map

- `CampaignView` renders one numbered cube per job from the ordered `ChapterData` list. Number = global
  index + 1; `PuzzleData.id` keeps the `XX_YY` shape the level cookies parse.
- States: **solved** (lit + stars), **current** (first unsolved, highlighted), **locked** (dark, refuses
  visibly). Until [progression](space-shaping-progression.md) lands, derive them from order alone — nothing
  solved, first cube current.
- **Exit:** the map shows the authored cubes with correct visual states; a locked cube refuses visibly; the
  number matches the catalogue order.

### M3 — Pick, brief, play, back

- Pick a numbered job cube → `SpaceShapingState` loads that job's `PuzzleData` and brief while the same
  Bubble view and map remain visible; there is no Map/Sculpt split or scale-up transition ([vision §3](../../design/vision.md#3-mixed-reality-why-the-bubble-is-the-point),
  [cube](../../design/systems/cube.md)). On result, return to the map at the same bubble placement.
- The **job card** plays the job's manual entry and tech-voice line as two chained TTS entries on entering
  the puzzle (`TTSHelper.Say(key, onComplete)`); author the test job's keys here and fall back to the
  generic text if a key is missing.
- Flow: `CampaignState` (boot + map) → `SpaceShapingState` (play + result) → back to the map. Setup/splash
  stay. The map is `UniverseViewMode.Bubble`; **no new map mode**.
- Aperture for the map: `miniBubbleRadius 0.45`, `bubbleDistance 0.75`, `viewRadius 12.5`,
  `fieldRadius 12.5`, `cellSize 5`, `softSectorBudget 32`, `softPlanetBudget 64`, `hideRadiusFactor 1.5`,
  MiniScale ≈ 0.036 — keeps ~5 cm hand targets for 8–10 cubes; tune on device.
- The puzzle's own field is generated from its seed and is independent of the map field; the aperture is
  raised for the map, not for puzzle content. Both fields spawn the same pooled `FieldPlanetBehaviour` —
  no puzzle-only planet view.
- Derived grid stays flat until the player scales.
- **Exit:** pick → brief → shape/fly/check → solve → back to the map; the map field and the bubble placement
  survive the round trip; 72 fps target held in the map (measured with FrameTiming, device pass is
  [later](space-shaping-catalogue.md)).

## 3. Map contract

- **Navigation — decided (2026-10-27): direct pick only.** The map is the only way in; the result offers
  Map/Retry, not Next. `LevelSelector` supplies the current id, but the player always chooses the cube.
- **Cube placement — decided (2026-10-27): authored**, one position per job for legible spacing. The
  generated sector field supplies the backdrop, never the cube positions.
- **Ownership:** `CampaignView` owns the map cubes and `SectorFieldSpawner`; `SpaceShapingView` owns the
  puzzle; `PlacementStore` owns bubble offset/yaw. All authored in `Game.unity`.
- **Aperture and color scope:** color scope does not use `viewRadius`; out-of-frame content is grayed by the
  `hideRadiusFactor 1.5` band. Raise `fieldRadius` and `softSectorBudget` together.
- **Budget:** map ≤ 32 sectors / 64 planets + 10 cubes; puzzle ≤ 10 planets + lattice; no per-frame
  allocations.

## 4. Tests

- Map state derivation (solved/current/locked) in EditMode against the catalogue order.
- Cube pick → job id mapping (number ↔ `PuzzleData.id`) — no off-by-one between index and id.
- Session load from a pick leaves the POC's one-puzzle behaviour unchanged.

## 5. Out of scope

- Entropy bar, clock band, result/fail panel — [HUD](space-shaping-hud-validation.md).
- Stars, persistence, reset semantics — [progression](space-shaping-progression.md).
- Deleting the legacy route/hub/economy content — [legacy cut](space-shaping-legacy-cut.md).
- A demand/economy layer on the map ([element pack](../../ideas/element-pack.md)).
- Authoring the full catalogue — [catalogue](space-shaping-catalogue.md).
