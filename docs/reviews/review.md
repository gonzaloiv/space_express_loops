# UX & progression notes

**Status:** review · UX and progression.
**Date:** 2026-11-12.

**Related:** [vision](../design/vision.md) · [core loop](../design/core-loop.md) ·
[progression](../design/systems/progression.md) · [jobs](../design/jobs.md) · [chapters](../design/chapters.md) ·
[lore](../design/world/lore.md) · [plans board](../plans/README.md)

A game-design pass over the docs and the implementation, written for the 2026-11-13 competition
freeze. It records what the build teaches, where the progression feedback is missing, and which doc
promises the code does not keep. **Freeze** = worth doing before the deadline; **Store** = after the
freeze. This is a review note, not the intended design — the design docs remain the target. Code
references are file names under `Assets/`; most are under `Assets/_Game/`.

## Verdict

The fiction, the instrument and the in-field entropy post are strong. The build currently reads as a
convincing *tool* more than a taught *game*: a first-time player meets a failing ship with one gesture
prompt and learns the goal by discovery, but the manual and exit arrive late; the stars they earn
vanish on the map; and the live preview plus the warmer/colder end dot turns the puzzles into a
hot/cold sweep. Fix the hand gate first, then the first minute and the score's visibility; protect
the "read the field" lesson in the store build.

## What already works

- **The entropy post.** Spend, pending cost, last edit and star bands sit where the decision happens
  (`FieldEdgeMeter.cs:104-120`).
- **The contract card.** The manual revision and the console clock sell the fiction cheaply
  (`JobCardPanel.cs:97-105`); prompts are earned and persisted (`GestureLearning.cs:8-34`).
- **The solve beat.** Arrival, stamp and Jim's receipt are complete (`PuzzleCompleteState.cs:74-98`);
  the ending lights the whole map (`RunEndingView.cs:76-94`).
- **The map.** It is a chain, not a menu (`MapRouteLine.cs:86-87`), and the card previews on cube
  move (`MapCubeSelection.cs:23-33`).
- **Best score.** Replaying a job cannot lower its stars (`RunProgressCookies.cs:92-106`).

## Findings

Findings are in priority order; the [Before the 13th](#before-the-13th) list is the smallest-ship subset in the same order.

### 1. The hands-only entry has no hand-tracking gate — **Freeze; plan in progress**

- The scene requests scene permission at startup (`Game.unity` `requestScenePermissionOnStartup: 1`)
  although the game uses no MRUK/scene anchors, and sets `controllerDrivenHandPosesType: 1`
  (ConformingToController) for a hands-only game (`Game.unity`).
- `SetupState` checks AR support but not hand tracking (`SetupState.cs:51-63`); `TwoHandGestureInput`
  simply gets no pinch when the hand is not connected/tracked (`TwoHandGestureInput.cs:206-223`), and
  `SeatPlacement` silently falls back to the default drop (`SeatPlacement.cs:109-115`).
- A judge with hand tracking off or controllers connected sees a permission dialog and an inputless
  universe in the first 30 seconds.

**Fix:** set controller-driven poses to None, turn scene permission off, launch simultaneous
hands/controllers, add a hand-tracking availability panel/gate, and test with controllers on the desk
([lessons](../lessons.md)). The gate holds while hand tracking is off and
requires both hands for the core's two-hand pinch; the panel stays up through flaky tracking, so a
one-hand judge does not reach an unpinchable job. The panel stays up until both hands track; it does
not pass the step.

**Decision (2026-11-12):** both hands are the gate. A one-hand judge keeps the panel until the second
hand tracks; a controller fallback stays out of scope for the freeze.

### 2. The first minute is a scale-only tutorial, but the manual and exit arrive late — **Freeze**

- A fresh save auto-enters job 1 (`PuzzleSelectionState.cs:68-74`).
- Job 1 is `noIntro` (`Job_01_01_Puzzle.asset`), so no Jim line (`PuzzleStartState.cs:45-48`) and no
  in-run contract card (`PuzzleEditionState.cs:97-101`).
- The only teaching text is "PINCH TO SCALE" (`Global_en.asset`, `gesture_prompt_scale`).
- The job-1 manual — *"Welcome. Your Wrap does one thing. Do that one thing."* — is only seen on the
  map **after** the job ([jobs](../design/jobs.md) §The run — the lore skin). The exit is not taught on job 1
  either (`PuzzleEditionState.cs:91-92`).

**Since this pass (2026-11-12):** job 1 now teaches the goal by discovery rather than a line. The
cube only grows there (`growOnly`, `PuzzleData.MinScaleFactor`), so the one decision is "how much",
and the pending end dot warms as the run nears the destination (finding 9).

**Decision (2026-11-12):** keep it this way — a pre-job beat, then a scale-only job, then the game
(Jim's receipt and the map). Scaling is the only in-run gesture; nothing else overlays it.

**Fix:** play the job-1 manual as the opening console beat **before the job starts**, keeping
`noIntro` so no card or Jim line plays over the job; teach the exit in that same pre-job beat or on
the first map card, **not** over the scaling surface. The job starts after the beat. The scale is the
main gesture, so once the job starts the Scale prompt is the only overlay: no goal line, exit prompt,
card, Spend or Stars prompt covers the in-run scaling surface. This is the cheapest judge-facing win.
**Done 2026-11-12:** job 1 is a pure tutorial, so the console types `> MAKE THE SHIP REACH THE RING`
(`PuzzleData.goalLineKey`) before its field appears; a restart skips it. In the job, a `Goal` prompt
panel (`> GET THE SHIP HERE`) stands over the destination with its leader on the planet until the
job ends — clear of the cube, so the scaling surface stays free.

### 3. The stars you earn vanish when you return to the map — **Freeze**

- Only the selected node shows stars (`RunSlotView.ShowMapState` gates them on `selected`).
  `JobEntry.ShowsStars` now shows unlit stars on every graded job not yet solved, locked ones too, and the map
  stars are ~1.2 cm and flat with the map — but still only on the selected node.
- The map selects the job after the last solve (`RunProgressCookies.cs:115-120` →
  `LevelSelector.GetFollowingLevel`), so the just-solved node is a neighbour with no stars.
- `PuzzleMap.TotalStars` is used only by the ending (`RunEndingView.cs:82`), although
  [progression](../design/systems/progression.md) §Feedback promises "stars per job; a running total".

**Fix:** show solved nodes' stars within the fade radius, add a console line
`CONTRACTS n/m · STARS x/y`, and put `BEST ★★` on the selected card.

### 4. The meter promises a score on the two jobs that award none — **Done 2026-11-12**

- `FieldEdgeMeter` always draws and tints the 3/2/1 star bands (`FieldEdgeMeter.cs:16-17, 115-120`).
- Jobs 1–2 are `noGrade` (assets; [jobs](../design/jobs.md) §The pressures fade in) and the stamp is skipped
  (`PuzzleCompleteState.cs:77-78`).
- Job 1 is `noFail`, so stars can dim but never be won; job 2 can fail but never be graded.
- Job 2's Stars prompt is suppressed (`PuzzleEditionState.cs:93-94`) while its stars remain drawn.
- Job 3 is then both the first graded job **and** the first clear-role puzzle.

**Fix:** hide the stars for `noGrade` (or label `TRAINING`), label `noFail` as `NO LIMIT`, and turn
stars on for job 2 (fixed centre, one verb) so job 3 is only "the other direction".

**Done 2026-11-12 (the `noGrade` part):** `FieldEdgeMeter` hides the stars and their notches on
`noGrade` jobs; the `NO LIMIT` label and stars on job 2 stay open.

### 5. The 2ft/airplane-seat reach is not measured — **Freeze**

- Since this pass the field sits 12 cm lower (2026-11-12: `defaultDrop` 0.27, `aboveHands` 0.13,
  `minDrop` 0.17), which eases the desk case; the lap case below is unchanged (the 0.4 m clamp).
- The field origin is placed 0.45 m from the head (`SeatSettings.cs:32-37`), then job 1's content sits
  0.11–0.26 m forward of it (`Job_01_01_Puzzle.asset:17-18`); the far endpoint is ~0.71 m from the eyes
  and the far lattice edge ~1.0 m.
- The seated hand-reach max is 0.7 m (`SeatSettings.cs:37`). On a plane with hands in the lap the
  field drops to the 0.4 m clamp (`SeatPose.cs:29`; `maxDrop` at `SeatSettings.cs:36`), so the judge
  looks down ~40° and leans.
- The pitch claims "the bubble and the cube sit at seated arm's reach" and "nothing needs standing or
  turning around" (`pitch.md:48-49`).

**Fix:** measure in a chair with no desk and a 2ft tape; reduce the seat distance/field scale or add a
raise control; record the comfort result in the pitch.

### 6. There is no in-run reset, even though the loop is designed around it — **Freeze**

- [Core loop](../design/core-loop.md) §3 says reset/retry is free and is the only way back.
- `ResetPuzzle` exists only on the lab panel (`PuzzleLabPanel.cs:51-55`); neither the panel nor its
  button is referenced by `Game.unity` or `Puzzle.prefab`.
- The only practical reset is a fail or exit → map → re-enter.
- A fail replays the reset console plus the full failing start check
  (`PuzzleStartState.cs:36-49`, `SpaceShapingController.cs:84-94`).

**Fix:** wire a free reset (console button or frame hold), name it in the exit prompt, and shorten
the start check on a retry.

### 7. After a fail, the map has two "current" nodes and the cube may sit on neither — **Freeze**

- `JobState` only has Locked/Current/Solved (`JobState.cs:3-8`).
- Every opened-unsolved node is `Current` (`RunJobs.cs:26-34`).
- The map selects the job after the last solve, while the pulse is the furthest current
  (`MapFieldRenderer.cs:74-104`).
- Fail job 2 (which opens 3): the cube is on 2, job 3 pulses, and the route ends at 3.

**Fix:** add a `Skipped`/`Opened` state (dim amber) and keep one `Current` (first unsolved), or select
the furthest opened. The skip is a judge aid; it should read as one.

### 8. The fleet jobs are single-zone bakes, so chapter 2's "several fixes" lesson is not real — **Freeze for lines; Store for bake**

- `PuzzleMultiShipBake` finds one zone that rescues every ship and stamps `puzzle.zones = 1`
  (`PuzzleMultiShipBake.cs:7-9, 36-47, 147`).
- All four chapter 2 assets read `zones: 1, routes: 1`.
- The docs promise "two fixes" and "three fixes on one budget" ([jobs](../design/jobs.md) §Chapter 2 — the
  fleet; [chapters](../design/chapters.md) §Chapter 2).
- The budget is 3× one edit, so "the budget did not grow" is literally true but the intended skill is
  absent.

**Fix:** for v1, reframe the episode as "one Wrap can carry several ships" (a shared-path edit) and
adjust the manual/Jim lines. For the store, add a per-ship zone search and real `zones = ships`
bakes.

### 9. The live preview and the warmer/colder end dot solve the puzzle for the player — **Done 2026-11-12 for the `noGrade` rule; Store for the rest**

- Every pinch previews the exact path (`SpaceShapingController.cs:129-153`).
- The pending end dot warms, grows and turns success as the run nears the destination
  (`TrajectoryEndMarker.cs:20-41`).
- A single-zone pinch is free to probe until release ([core loop](../design/core-loop.md) §3), so jobs 1–4
  become "sweep until the dot is warm"; jobs 3–4 then award 3★ for the sweep.
- Job 6 asks the player to read a deep well; the dot tells them when they are close without reading
  anything.

The warmth was added on purpose for job 1's onboarding (finding 2); the critique is that it stays on
for every job.

**Fix:** keep the warmth only on `noGrade` jobs (1–2): a one-line condition that keeps the teaching
and takes the hint away once stars are at stake. Longer term, make the pending marker neutral
(outcome only on release) or coarsen the line. Validate against [vision](../design/vision.md) §7 ("can they
say *why*"). The `noGrade` rule is small enough for the freeze; the rest is store work.

**Done 2026-11-12 (the `noGrade` rule):** the pending end dot warms only on `noGrade` jobs; the live
preview and the rest stay store work.

### 10. The map hides its depth axis and its chapter break — **Store**

- Numbers show only on the selected node and its ±1 route neighbours (`MapFieldRenderer.cs:98,
  107-109`); stars only on the selected node. The number limit is deliberate (2026-11-12, to quiet a
  cluttered map), so the fix keeps it at rest.
- There is no chapter concept: four new nodes appear with no episode beat
  ([chapters](../design/chapters.md) §Open questions leaves the break open).
- `ManualRevision` resets to r1 for chapter 2 (`PuzzleData.cs:52-53`), which breaks the "manual keeps
  being rewritten" fiction.

**Fix:** an `EPISODE 2 — THE FLEET` console beat plus a chapter-tinted node ring or route segment;
show numbers for all opened nodes while the cube is held; offset the manual revision by chapter.

### 11. Job 5, the multi-zone intro, is a single-route spike — **Store**

- `Job_01_05_Puzzle` has `routes: 1` with two zones and a 3.77 budget, directly after job 4's 3
  routes.
- [Jobs](../design/jobs.md) §The run — archetypes says each archetype should offer more than one route where
  the brief allows.

**Fix:** give the first two-zone job at least a second valid pair; save the one-answer puzzle for the
capstone.

## Lore in the game lines

The world is **named**, not explained. The player meets the vocabulary — Our Space Corp, the console,
the new employee, the Wrap, entropy, the ledger — and the tone; the economic frame is not in any line.
The lore's own "In the game — show, don't tell" table ([lore](../design/world/lore.md) §In the game) commits to
only six carriers, so part of the gap is deliberate.

| Concept | In the game lines? | Evidence |
|---|---|---|
| **Spacetime wrapping / the Wrap** | Named, not explained | `level_01_01_init_manual_entry` "Your Wrap does one thing"; `level_01_03_init_manual_entry` "The Wrap takes as well as gives"; jobs 4–5 "The Wrap is yours to place"; job 6 "The field is free. The Wrap is not." "Spacetime" never appears; no line says it deforms space, is new, or could destroy the universe. |
| **Entropy** | Named as the budget, not explained | `field_meter_entropy`, `gesture_prompt_spend`, `field_reset_spent`, `field_reset_full`; job 6 manual. No line says what it is. |
| **Eco-greed** | Implied, not stated | `gesture_prompt_stars`; the "ECO" stamp; "The ledger is happy" after every solve; `level_02_01_complete` "Same cost. Nobody asked how."; `level_02_04_complete` "Three for the price of one..."; `level_02_04_init_manual_entry` "Efficiency is a virtue until it is a cost." The words "green" and "greed" never appear. |
| **Stationary-state society** | Absent | No society/population/growth line. Closest: job 7 manual "There has not been a new employee in eleven years." Client names hint at a tired economy ("The Last Two Customers", "Nobody & Co."). |
| **Fuel is expensive** | Absent | "Fuel" never appears. Closest: "The field is free. The Wrap is not." — field vs Wrap, not fuel. The launch is free and unexplained; the lore says fuel is not a lever. |
| **Economy / fixed pie** | Absent | No market/pie/growth line. Closest: "The ledger is happy", "Same cost...", "Efficiency is a virtue until it is a cost." |
| **No office / remote** | Present | Boot header "OUR SPACE CORP · DISPATCH CONSOLE · REMOTE SEAT 2"; job 1 manual "there is no office"; job 7 manual "There is still no office." |
| **New employee / eleven years** | Present | Boot (fresh save) "WELCOME, NEW EMPLOYEE."; job 7 manual. |
| **Jim / company** | Present | Card header "OUR SPACE CORP."; job 1 complete "I'm Jim. This is Our Space Corp."; boot header. The two-kitchen detail is planned art, not a line. |
| **Stars as margin** | Implied | Stars + "The ledger is happy"; the word "margin" never appears. |
| **"Revenue is relative"** | Removed (2026-11-12) | The `Game.unity` override of the boot card's client line is gone; the prefab's empty line shows, matching [lore](../design/world/lore.md)'s cut. |

**The order on the default path.**

1. **Boot:** the company/console/remote seat header and "WELCOME, NEW EMPLOYEE."; the tagline leftover.
2. **Job 1:** the "ENTROPY" meter and "PINCH TO SCALE"; no Spend prompt (`noFail`), no card and no Jim
   intro (`noIntro`). The "Your Wrap / there is no office" manual is on job 1's map card, but the map
   opens on job 2, so the default path skips it (finding 2 moves it to the pre-job beat).
3. **Job 1 complete:** the first Jim line, "I'm Jim. This is Our Space Corp.", and "The ledger is
   happy."
4. **Job 2:** the first "EACH PINCH SPENDS ENTROPY" prompt; the manual is the star lesson, still no
   "Wrap".
5. **Job 3:** the first graded job; the first "ECO" stamp and Stars prompt; the first "Wrap" manual on
   the default path.
6. **Job 6:** "The field is free. The Wrap is not."
7. **Job 7:** "eleven years", "There is still no office", "Today the space. Tomorrow the time."
8. **Ending:** "The tech team says more are coming."

**What this means.** The player learns who they are and what the tool costs before learning what the
tool is, and the economic frame never arrives. This is consistent with the lore's "backdrop, not
exposition" rule, but the bridge from the mechanic (entropy/stars) to the fiction (margin/fixed pie)
is thin.

**Fix (cheap, in the lore's voice):**

1. Show job 1's manual before entry, or put its opening on the boot card. It is the only line that
   names the Wrap and says "there is no office".
2. Remove the "Revenue is relative" leftover, or move it to a moment after the first cheap solve
   (the lore's own rule). **Done 2026-11-12** (removed).
3. If the economics should be understood, put it in the ledger/manual, not Jim: a boot
   `> LEDGER .......... FIXED`, and keep "The ledger is happy" as the bridge from stars to margin.
   For fuel, either state "The launch is paid. The Wrap is not." once or leave fuel out; it is not a
   lever.
4. Leave the stationary state as tone; "eleven years", "no office" and the client names are enough.

## Before the 13th

1. A hand-tracking gate (both hands) plus the scene/controller-pose config (finding 1;
   [lessons](../lessons.md)).
2. Job 1's pre-job beat (manual + exit), then the clean scale-only job; the goal line only if a fresh
   player flails (finding 2).
3. Earned stars and a running total on the map, plus `BEST` on the card (finding 3).
4. One clear meaning for the `noGrade`/`noFail` meter, and stars on at job 2 (finding 4). **Done 2026-11-12** for the meter (stars hidden on `noGrade`); stars on job 2 cut for the freeze.
5. The 2ft/desk/lap placement test and tuning (finding 5).
6. The chapter 2 line reframe (finding 8): "one Wrap can carry several ships"; no new bake before the
   freeze.
7. Warmth on the pending dot only for `noGrade` jobs (finding 9): a one-line condition. **Done 2026-11-12.**
8. Jim over the music: the planned duck is not implemented; verify the mix. The chapter 2 and ending
   clips are present in `StreamingAssets`; the old missing-clips tail is closed.
9. Remove or confirm the `Revenue is relative` boot leftover; it contradicts the lore's own cut. **Done 2026-11-12** (removed).

**If the morning only holds three content fixes:** onboarding (finding 2), stars (finding 3) and
meter (finding 4); the hand-tracking gate is the first setup gate and has its own plan. The tagline
removal and the `noGrade` warmth are ten-minute changes.

## Docs that no longer match the build

- [Core loop](../design/core-loop.md) §3 — "reset/retry is free", but there is no shipped reset.
- [Progression](../design/systems/progression.md) §Feedback — "a running total", but the total only appears at
  the ending.
- [Progression](../design/systems/progression.md) §The map — "each node carries a number", but only three
  numbers exist at a time (`MapFieldRenderer.cs:98, 107-109`). That limit is deliberate; update the
  doc to "the selected job and its route neighbours carry their number".
- [Jobs](../design/jobs.md) §Chapter 2 — "two fixes" / "three fixes", while the bake is `zones = 1`.
- Job 1's `growOnly` cube (contract → seed → brief → bake, checked by the job validator) is not in
  [jobs](../design/jobs.md) §The run — the mechanics; add it to job 1's row.

## Open questions

- If the preview keeps its warmth, does the §7 validation still show players can say *why* a route
  works, or only that they found it?
- Is the fleet's one-zone bake a happy accident worth keeping as "one Wrap, many ships", or does the
  store need true per-ship fixes to make chapter 2 a progression axis?
- Does the map need a chapter break at all, or is one continuous galaxy the better read at 11 nodes?
