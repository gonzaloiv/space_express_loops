# Done

| Plan | Completed | Notes |
|---|---|---|
| [Two views + interaction unify](universe-views-and-interaction.md) | Phase A4 | Room/Bubble views, content-local routes, pinch-only ships, two-hand gesture with pinch rings + line, node handles removed. |
| [Silent planets](universe-silent-planets.md) | Headset pass | Seeded non-routable scenery, uniform volumetric distribution, occupancy-aware placement, Room culling; persistence validation deferred. |
| [Universe — Bubble map drag](universe-transform.md) | Implementation complete | One-hand map drag, two-hand scale, center marker, recenter, persistence. View/pose model superseded by the [view frame](universe-bubble-window.md); headset validation/tuning carried into that slice. |
| [View frame (bubble cube)](universe-bubble-window.md) | Headset pass | Movable cube frame over a fixed universe (`m = miniBubbleRadius / viewRadius`); cube marker, cube containment, no content reparenting, frame culling band. Prereq for color scope. |
| [Reusable modes + Universe migration](modes-reusable-and-universe.md) | Complete | `ModeController<T>` in FlowControl with EditMode tests; Universe uses one mode source; readers migrated. |
