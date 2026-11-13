# Lessons

Short post-mortems, written when a plan closes (rule in [AGENTS.md](../AGENTS.md) §Plans are a
morning). Newest first. Each entry: what worked, what surprised us, the rule for next time. One idea per
bullet; the code and tests are the record of what was built.

## 2026-11-13 — Fleet episode

- **Worked:** the bakes moved into their own chapter folder with their GUIDs and one `ChapterData`,
  so a whole episode joined the run with no map or flow rework.
- **Surprised:** new jobs did not make an episode; the world only came through once the chapter had
  its own opener, separate from the job intro (episode beat vs lesson).
- **Surprised:** the line keys gained Jim's expression suffix in a table sync; the game matched them by
  fragment, but an exact-key test broke and every prebuilt clip went silent.
- **Next time:** a renamed line key means new clips; plan the download with the rename.

## 2026-11-13 — Hand-tracking gate

- **Worked:** a pure gate fed per frame (enabled, two hands, delta) made the steady window, flicker and
  "off never passes" plain EditMode tests.
- **Surprised:** a card panel for the hint fought the boot console; typing it as console lines read as
  one system and needed no new UI.
- **Next time:** a new optional step in a `_Modules` flow stays invisible when unassigned, so other
  projects never see it.

## 2026-11-12 — First minute and meter

- **Worked:** the meter hides the stars and their notches from `PuzzleReadout.NoGrade`, so the
  tutorial jobs show the entropy post without a score they cannot award.
- **Worked:** a nullable miss gap made "warmth only while nothing is at stake" explicit at the
  caller (`TrajectoryLinePool`); `null` is neutral, not a sentinel value.
- **Surprised:** the splash tagline lived only as a scene override of the prefab's empty client
  line, so removing the override was the whole fix.
- **Next time:** when a hint is tutorial-only, gate it where the job kind is known, not in the view
  that draws it.

## 2026-11-11 — The ending

- **Worked:** counting the run from the selector, never a literal, made the ending valid for any number
  of jobs; storing the job count in the seen flag lets a new chapter earn it again.
- **Surprised:** a flow state's exit duties (hide the puzzle, the exit cue) moved between states twice
  across agents; once nobody owned them, the solved puzzle stayed over the map in a build.
- **Surprised:** reaching the 250-line cap by packing statements and deleting blank lines passed the
  count but not the rule; the real fix was splitting the interaction gate off.
- **Next time:** when a state takes over another's duties, move them in one commit with the scene
  wiring, and check the build path, not only the tests.

## 2026-11-11 — Endpoint shapes

- **Worked:** resolving each endpoint to its session source kept a pushed endpoint's ring on its
  planet, and one pool per role kept `FieldPlanetSet` under the cap.
- **Surprised:** re-authoring the endpoint prefabs as rings dropped the planet body, and with it the
  size cue the player reads to predict the pull; the headset caught it, the tests could not.
- **Next time:** a role marker decorates the thing; it never replaces it. Add marks next to the body
  (`NodeBody.marks`) instead of swapping the body.
- **Next time:** edit prefabs in place (load, change, save) so every referenced component keeps its
  file ID; rebuilding one breaks every scene and prefab that points into it.

## 2026-11-11 — Stuck-player skip

- **Worked:** one payload on the existing restart value (signpost index, first open) carried the whole
  feature through the flow without a new seam or a new state.
- **Surprised:** "one fail opens the next" chains: failing an opened job opens the one after. Kept on
  purpose: a stuck judge sees every job, and only solving completes the run.
- **Next time:** test a domain member the views read (`HighestCurrentIndex`) instead of widening an
  internal view for tests.

## 2026-11-11 — Field wells (spike)

- **Worked:** a toggle and a struct behind the lattice warp made the spike cheap to try and cheap to
  turn off.
- **Surprised:** wells from the first frame read as chaos on the headset; the problem was the bent
  start, not wells. Drawing only the change from the start kept the idea.
- **Surprised:** per-source caps do not cap a sum: a strong sun behind a planet dragged nodes through
  it until the summed move was clamped too.
- **Next time:** a field cue should show what the player changed, not everything that is there.

## 2026-11-11 — Run fixes

- **Worked:** reusing the module's menu invoker and its aim flags gave the exit gesture in an hour;
  the left-hand-only scope kept the universal menu from dropping a player out of a job.
- **Worked:** a fail that restarts in place, with a one-shot flag the next state reads and clears, kept
  the retry fast without a new flow state.
- **Surprised:** a skinned mesh in a preview scene keeps its first pose unless told to recalculate, so
  every rendered gesture loop was frozen until a grip check caught it.
- **Surprised:** a debug override (`puzzleToSet`) was committed twice with unrelated scene changes.
  Review the scene diff hunk by hunk before a commit; never stage `Game.unity` whole.
- **Next time:** lines need clips; plan the TTS download with the strings, or the build ships silent.

## 2026-11-11 — Transit routes

- **Worked:** one colour per route reads at a glance; the outcome at the line's end freed the line to
  carry identity instead of result.
- **Surprised:** a faint box around every endpoint while moving was too much with two ships; one red
  cube at the hand on a refused move says the same with less.
- **Surprised:** hand-typed GUIDs in a prefab collided with scripts and broke asset loading. Let Unity
  assign GUIDs; repoint by asset, never by guess.
- **Next time:** split the art-heavy unit (shapes, interchanges) out at the start when the day is a
  code day.

## 2026-11-11 — Judge run

- **Worked:** one constant (`PuzzleGrade`) behind every star limit meant the margin moved the meter,
  the prompt art and the passthrough pulse in one change.
- **Surprised:** tests that hard-code a limit keep passing after it moves; their names lie. Tests read
  the domain constant, not a copy of its value.
- **Next time:** a unit that waits on a playtest is its own small plan, not a tail on a done one.

## 2026-11-11 — The two-ship test

- **Worked:** one test level on the headset settled a question the docs had argued for weeks; the
  fleet bakes played better than the single-ship jobs and went into v1 as their own episode.
- **Worked:** the test lived outside the run (its own assets, no chapter edits), so a "no" would have
  cost nothing to undo.
- **Surprised:** the headset play found the real work, not the bake: route colours, the outcome at the
  line's end and the keep-out became their own plan (transit routes).
- **Surprised:** three ships bake much more slowly than two. Budget the bake time when a job needs a
  third ship.
- **Next time:** when a test says yes, plan the content (ids, lines, clips) at once; the code to play
  the level is the small part.

## 2026-11-10 — The panel bank (rolled back)

- **Rolled back:** the windows on two arms from the grid's corner did not read well in the headset;
  the code went back to the earlier `ConsoleLayout` placement and sizes the same day.
- **Next time:** mock a layout change in the headset (fixed transforms, no code) before building the
  geometry, tests and flow wiring around it.
- **Surprised:** "outside the field's silhouette" was impossible from the eye. The near face projects
  wider than the back corner, so the contract became draw order instead.
- **Surprised:** a line renderer's width is world metres and ignores its parent's scale. The prompt
  leader line, authored at 0.32 under a 0.005-scaled panel, drew 32 cm slabs.
- **Next time:** one root per animated group. Cards left under other parents escaped the console's
  CRT until they moved under the bank.
- **Next time:** check a contract's geometry from the eye position before writing it.

## 2026-11-10 — The field meter

- **Worked:** the field carries the feedback. The bar on the grid edge and a dark map cube over locked
  jobs replaced a floating bar and the words over the selection.
- **Surprised:** the chapter's star square under the selected map field read as that job's result;
  it was removed the same day for the selected job's own stars over its sun. A total drawn on one
  item's frame reads as that item.
- **Surprised:** marks drawn from counts drift from the rules they show: the map ticks were off by one
  (sixths instead of fifths), and the job ticks assumed equal thirds. Draw marks from the domain
  values (`PuzzleGrade`).
- **Surprised:** in the editor the Origin sits at the world origin, so world-axis prompt offsets and a
  world/parent mix in the console layout passed every editor check and broke only once the seat
  moved the Origin.
- **Next time:** anything laid out relative to the field or the map converts through its owner's
  transform, and its test runs under a moved and yawed parent (rule added to AGENTS.md).

## 2026-11-10 — Offline

- **Surprised:** the plan first missed `MemoryDataClient`, the in-session copy every caller reads.
  Only four places saved; routing all reads through the new provider would have been a refactor
  nobody needed.
- **Surprised:** analytics assumed Unity Services was up. Once the boot stopped waiting for the
  services, every early funnel event could throw and break the setup chain.
- **Worked:** one rule for a cloud copy that arrives late: swap it in at the next return to the map,
  and stop mirroring until then, so a local save cannot overwrite richer cloud progress.
- **Next time:** before changing a data path, list its readers and its writers separately; they rarely
  need the same change.

## 2026-11-10 — Soundscape

- **Worked:** one owner per beat. Job entry used to stack a sweep, a glitch blip, a panel blip and
  prompt blips; giving the moment to `PuzzleStartState` alone made it read as one sound.
- **Worked:** ticks and transitions play differently. Transitions cut (`Play`), ticks never do
  (`PlayOver`), so a map tick cannot kill the return sweep.
- **Surprised:** the shared synth carried this game's tuning. `SynthVoice` had cube-only stretch
  methods and the cube's timbre as defaults, so another project could not use it as is.
- **Next time:** a `_Modules` type exposes verbs; the game's tuning lives in data assets the game owns
  (here, `SynthPatch` tones under `Features/Audio/Synth/Tones`).
- **Next time:** measure the first continuous voice on the Quest 3S before adding the second. Each
  synth voice is per-sample C# plus a Meta XR source.

## 2026-11-10 — The first 30 seconds

- **Worked:** the boot is the loader. Showing each setup step as it runs (`LINK`, `SEAT`, `LEDGER`)
  turns the wait into content, and a slow step is visible instead of a frozen screen.
- **Worked:** no-op virtuals on `BaseSplashScreen` let `SetupState` report steps without changing the
  other splashes.
- **Surprised:** placing at setup used wherever the headset was pointing, often at the desk. Placement
  has to wait until the headset is worn and the head pose has settled.
- **Surprised:** putting the headset on restarted the placement wait and dropped setup's callback,
  so the boot hung on `> SEAT`. A step that can be restarted by a system event keeps its caller's
  callback outside the coroutine.
- **Next time:** treat launch as the first moment of play. The judge's clock starts at the app icon,
  not at job 1.

## 2026-11-10 — Pause

- **Surprised:** `Time.timeScale = 0` stops much less than it looks. Input, the map's pinch-to-enter,
  the puzzle preview and the TTS chain all run on unscaled time and kept going behind the menu.
- **Next time:** when adding pause, list every `Time.unscaled*` reader first and decide for each one:
  gate it, or let it run as cosmetic.
- **Next time:** a system that owns a timeline (TTS) handles its own pause and replay; a global pause
  flag cannot rewind a line.
