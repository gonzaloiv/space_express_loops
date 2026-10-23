# Done

| Plan | Completed | Notes |
|---|---|---|
| [Two views + interaction unify](universe-views-and-interaction.md) | Phase A4 | Room/Bubble views, content-local routes, pinch-only ships, two-hand gesture with pinch rings + line, node handles removed. |
| [Silent planets](universe-silent-planets.md) | Headset pass | Seeded non-routable scenery, uniform volumetric distribution, occupancy-aware placement, Room culling; persistence validation deferred. |
| [Universe — Bubble map drag](universe-transform.md) | Implementation complete | One-hand map drag, two-hand scale, center marker, recenter, persistence. View/pose model superseded by the [view frame](universe-bubble-window.md); headset validation/tuning carried into that slice. |
| [View frame (bubble cube)](universe-bubble-window.md) | Headset pass | Movable cube frame over a fixed universe (`m = miniBubbleRadius / viewRadius`); no content reparenting. The frame-culling band was reversed by the [color scope](universe-color-scope.md) (out-of-frame content grays instead of hiding), and the center marker now draws the scope ellipse rather than a cube. |
| [Reusable modes + Universe migration](modes-reusable-and-universe.md) | Complete | `ModeController<T>` in FlowControl with EditMode tests; Universe uses one mode source; readers migrated. |
| [Universe — Bubble color scope](universe-color-scope.md) | Device pass | Ellipsoid scope in the `LitRim` fork; world + room-prop materials migrated, level UI fader wired. Grays/hides outside a content-sized ellipsoid (`scopeScale` / `scopeRadii`), and the pan clamp shares the shape. Bigger universe extent deferred to the sector field. |
| [Space shaping — POC](space-shaping.md) | Validated on device | One ship, one destination; sources -> field -> grid -> trajectory; bounded moves and grab/scale work on device; the lab runs on the target architecture. Carried forward: ghost preview, move-range viz, tuning, lattice presentation (see the architecture + integration plans). |
| [Space shaping — spike](space-shaping-spike.md) | Passed | Smallest testable version: Tier 0 math + a 3D debug scene. Found the launch-to-curvature ratio band and chose **coast**. |
| [Space shaping — architecture refactor](space-shaping-architecture.md) | First pass complete | The lab runs on `Domain/` / `Data/` / `Views/` / `Flow/`; the POC driver and the LineRenderer grid are gone. Game integration tracked separately. |
