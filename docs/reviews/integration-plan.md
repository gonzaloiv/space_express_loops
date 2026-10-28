# Review — space-shaping integration plan

**Date:** 2026-10-27.
**Scope:** the integration plan (now split into six: [in the game](../plans/backlog/space-shaping-in-game.md),
[campaign map](../plans/backlog/space-shaping-campaign-map.md),
[HUD + validation 2](../plans/backlog/space-shaping-hud-validation.md),
[progression](../plans/backlog/space-shaping-progression.md),
[legacy cut](../plans/backlog/space-shaping-legacy-cut.md),
[catalogue](../plans/backlog/space-shaping-catalogue.md)) against the design
docs and the code (`Assets/_Game`).
**Verdict:** the direction is right, but the plan was **stale in five places**, **oversized for one plan**,
and **most of its open questions were already settled** by [variations](../ideas/variations.md),
[progression](../design/systems/progression.md) and [vision](../design/vision.md). Close those first; then
split the plan at the validation gate.

**Applied 2026-10-27:** full replacement was ratified; the scope/phases, the field-source and loop-route
corrections, the selector/HUD/cut-depth notes and the validation-gate split are now in the plan. Navigation
(direct pick only), cube placement (authored) and launch (global) were also decided. The remaining §6
contracts are the campaign shape, progression record, runtime puzzle contract, run/readout and ownership.

**Applied 2026-10-28:** the POC now spawns the shared planet view (`FieldPlanetBehaviour`) from the shared
prefab; `SpaceSourceView` and the POC-only `PuzzlePlanet.prefab` are removed, so the in-game plan reuses the
view as-is.

**Applied 2026-10-28 (split):** the oversized plan was replaced by six one-morning plans — in the game,
campaign map, HUD + validation 2, progression, legacy cut, catalogue + acceptance. The gate moved to the end
of the HUD plan; each contract (§6) now lives in the plan that consumes it.

**Applied 2026-10-28 (reduce):** the job card moved from [in the game](../plans/backlog/space-shaping-in-game.md)
to [campaign map](../plans/backlog/space-shaping-campaign-map.md) M3; the in-the-game plan is the puzzle
flow and result seam only.

## 1. Close now (settled elsewhere)

| Item | Decision | Evidence |
|---|---|---|
| Replacement vs mode | **Full replacement.** A mode branch doubles the flow and save surface; the campaign map is the progression spine. | vision §5, §8; progression “the company is lore only” |
| What survives | Setup/splash, XR rig + hands, bubble placement, planet prefab, cube gesture, the sector field as selector substrate, TTS/analytics framework. | Current scene wiring; plan §3.3 |
| Snapshot policy | **Player progression only** (solved jobs + stars). Never mid-level state; the field is baked. | progression §Persistence; plan §3.5 |
| Level selector role | **Selection only** for the slice; a demand/economy layer is deferred. Navigation is direct pick only. | progression §Open; element pack (station = future demand layer) |
| Sequencing | **Single-leg jobs only.** Multi-ship stays out; the sequential loop route is a later variation. | jobs.md; vision §5; variations §2.2 |
| Puzzle field source | **The puzzle generates its own field from its seed.** The campaign field places cubes. | variations §3; progression §3 |
| Aperture trigger | Raise `viewRadius`/`fieldRadius` when the **selector map** needs the wide field. | variations §3 |
| Silent-planet migration | **Already done in substance.** `SectorFieldSpawner` is the field’s ambient-sector spawner. | `SectorFieldSpawner.cs:38-72` |
| Loop behaviour | No commit: every completed pinch flies the ship; a miss is a wrong check; entropy zero is the only hard fail. | core-loop §2; resources §Failure |

## 2. Fix (contradictions and stale statements)

1. **I2 contradicted §3.5.** I2 said “puzzle snapshot”; §3.5 and progression say progression-only. Replaced
   with progression save. The same stale “snapshot = scale commands” line in `variations` §4 is fixed.
2. **I4 duplicated the creator plan.** Replaced with the campaign catalogue (3–5 first, 8–10 stretch).
3. **§3.3 implied the level selector exists.** `LevelSelector`/`LevelsPanel` are generic `_Modules/Casual`
   code, not wired in `_Game`; the cube selector is new work.
4. **§3.6 HUD is not built.** `SpaceShapingState` exposes no readout and the POC has no HUD. The plan now
   names the surface; the separate lab readout landed 2026-10-28.
5. **§4 contradicted the run.** The loop route is deferred; “Map View” is a retired concept.
6. **I3 was too shallow for a delete step.** `LevelContainer` owns both `SectorFieldSpawner` and bubble
   restore; `GameStartState` owns mid-level resume. The keepers move first.
7. **Dropping `GameSnapshot` also drops two keepers:** bubble placement and `silentPlanetSeed`. The plan now
   carries them into a placement/campaign snapshot.
8. **§3.7 priority was inverted.** Seed + brief is primary; fixed sources are the fallback.

## 3. Missing scope

- **Flow/mode contract** — map as the Bubble view, puzzle as Flow, result as a panel (`modes.md`).
- **Progression plumbing** — reuse `LevelCompleteCookie` (`hasBeenCompleted` + stars), `LevelSelector`,
  `PlayerData`; ids `XX_YY`; no new schema unless needed.
- **Legacy reset** — ignore `GameSnapshot`, keep bubble placement, first boot writes a fresh save.
- **Result/fail UX** — window, entropy-zero fail, grade, retry/map.
- **Performance gate** — 72 fps on Quest 3S, map/puzzle budgets, device capture.
- **Content dependency** — the brief’s `moves = 2+` work is now the
  [multi-zone plan](../plans/in-progress/space-shaping-multizone.md); physics must land before bakes.
- **Test/device pass** — boot → map → pick → solve → back → reload; no legacy route/hub/round content.

## 4. Plan shape

This was a project, not a morning. It is now six plans, each one sitting, with the gate kept:

- **In the game** — one puzzle in `Game.unity`, the flow, the result seam.
- **Campaign map** — cube map, pick → brief → scale up → play → back; keeper moves.
- **HUD + validation 2** — entropy, clock window, result/fail. **Its exit is the gate.**
- **Progression, legacy cut, catalogue + acceptance** — only after the gate.

No legacy deletion or progression work before validation 2 passes.

## 5. Decisions that actually remain

1. **Replacement vs mode — decided:** full replacement.
2. **Pick directly or only “next”? — decided:** direct pick only; the map is the only navigation.
3. **Is the map a new mode? — recommendation:** no new Flow state; it is the Bubble view showing cubes,
   and the scale-up is a presentation transition. Flow would be `CampaignState` → `SpaceShapingState`.

## 6. Safe prep before replacement

These are contracts and seams, not flow or scene work:

1. **Stable puzzle identity + campaign order.** `PuzzleData.id` in `XX_YY` form; one `ChapterData` order.
2. **Progression record.** Stars on the completion cookie; reset rule; no in-level snapshot.
3. **Runtime puzzle contract.** Per-puzzle vs global values; brief locks; `solution` creator-only.
4. **Domain seam (`PuzzleRun`).** Load/reset/preview/commit/undo/readout/result; when solve/fail fires.
5. **Readout + HUD surface.** Read-only snapshot and the result/fail beat.
6. **Map/flow contract.** Map as Bubble view, puzzle as Flow, scale-up transition, back/next.
7. **Carve-out manifest.** Field spawner and bubble placement out of `LevelContainer`; resume out of
   `GameStartState`.
8. **Performance budget.** 72 fps per-frame budget and the device pass.

Each contract now lives in the plan that consumes it: runtime puzzle + run/readout in
[in the game](../plans/backlog/space-shaping-in-game.md) §3–§4, map/flow/ownership in
[campaign map](../plans/backlog/space-shaping-campaign-map.md) §3, the record in
[progression](../plans/backlog/space-shaping-progression.md) §3.
