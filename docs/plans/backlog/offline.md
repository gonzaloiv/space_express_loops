# Offline — the game runs with no internet

**Status:** next · flow · updated 2026-11-09 — progress kept locally and mirrored to the cloud when
online; no internet gate on launch. The judges' one-bus-stop test (fast cold start). Split from
[pause](../in-progress/pause.md), which goes first. Checked against the code 2026-11-09.

**Related:** [pause](../in-progress/pause.md) · [the first 30 seconds](../in-progress/first-30-seconds.md) ·
[plans board](../README.md) (Road step 8, judge tests)

## Goal

`SetupState`'s internet check waits for Unity Services, then shows "internet required" and quits; saves
go only to Cloud Save. A judge without a connection loses the game before playing it.

## Contract

- **Local is the source of truth.** Progress is always written to the device first; the cloud is a
  mirror when it is reachable. Losing the connection never loses a solve.
- **No internet panel on the critical path.** Offline, the game starts as fast as online.
- **One client for the callers.** The flow and the prompt recorder keep one injected `IDataProvider`;
  which store it hits is the provider's business.

## Units

### U1 — the local client and the wrapper

- One client contract: the existing `IDataProvider` (`Get<T>` / `Put<T>`). Today it is implemented only
  by the unused `MemoryDataProvider`, and its RSG `Promise` returns are not awaitable while the player
  data callers already `await` Tasks. Make it Task-based (`Task<T> Get<T>()`, `Task Put<T>(T)`), have the
  local player-data provider and the wrapper implement it, and inject it in `PuzzleCompleteState`,
  `GestureLearningRecorder` (and `GestureLearning.Save`, which takes the concrete client today), `Main`
  and the remaining `UnityPlayerDataClient` callers (`InternetChecker`, `ReviewPanel.cs:29`,
  `CookieHelper.cs:11`). Move the session-snapshot rule onto `PlayerData` (`AddSessionSnapshot`) so it
  is not a client-specific method; delete `MemoryDataProvider` with the old Promise shape.
- A PlayerPrefs provider (JSON per type, `PlayerPrefs.Save()` after each write, not only on quit).
- A wrapper provider: reads and writes local first; when online, mirrors each `Put` to Cloud Save without
  failing or blocking the local write (do not `await` the cloud on the caller's path). On launch, take
  the cloud copy only when it holds more completed jobs than local; otherwise push local up. Compare
  completed-job cookies, not all cookies, and keep the comparator out of `DataAccess.Remote` — that
  assembly cannot reference `Casual.Levels`/`_Game`, so compare the progress key or use a count defined
  in `DataAccess`.
- `Main` registers the wrapper and hydrates local data before the flow starts; the session snapshot is
  written locally at launch so a first offline launch has `sessionCount != 0`.
- **Exit:** solve offline, relaunch offline — the progress is there; go online and relaunch — the local
  progress reaches the cloud.

### U2 — launch without internet

- Hydrate local PlayerData synchronously in `Main.InstallDataProvider` and start the Flow without
  awaiting Unity Services; run initialization, sign-in and the cloud reconcile in the background.
  Today `Main.InitializeServicesAsync` is awaited before the first state (`Main.cs:102-113`), so the
  internet check alone changes nothing: offline the boot still waits on the services timeout.
- `SetupState` stops referencing `InternetChecker`: it goes straight to the device/CXR check and the
  splash. Add an explicit launch option instead — `Main` gets a serialized `requireInternet` (default
  off). When on, the services result gates the flow with a bounded wait and shows the "internet
  required" panel and quits; when off, offline is the normal path. Retire `InternetChecker` and move its
  funnel event to `Main`/`SetupState`.
- The cloud pull can land after the flow starts: replace the in-memory copy only when the cloud has more
  completed jobs, and never mid-job (defer the swap to the next launch if it arrives late).
- **Exit:** airplane mode → launch → the boot plays and job 1 opens with no panel and no wait; with
  `requireInternet` on, the same launch shows the panel.

## Manual check (headset)

1. Airplane mode, erase the save, launch: no panel, the boot plays, job 1 opens.
2. Solve job 1 offline, quit, relaunch offline: job 1 is solved on the map.
3. Back online: the progress reaches the cloud (the next launch online keeps it).
4. With `requireInternet` on and airplane mode, launch: the panel shows and the game quits.
