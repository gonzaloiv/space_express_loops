# Lessons

Short post-mortems, written when a plan closes (rule in [AGENTS.md](../AGENTS.md) §Plans are a
morning). Newest first. Each entry: what worked, what surprised us, the rule for next time. One idea per
bullet; the code and tests are the record of what was built.

## 2026-11-27 — The answer-first generator

- **Worked:** the single-ship constructor met its bakes (Clear 12/20, Curve 11/20), and the search was
  never chapter 1's bottleneck. The generator was a second path, not the next speed-up.
- **Surprised:** the fleet recipe never converged. The lesson placed the blockers but every bend offset
  failed on a crowded crossing; bend-first moved the failure to the start curve (310/474) and the
  scale (155/474). Clear was 1/20, Curve 0/20.
- **Next time:** test a new fleet idea by measuring the search's passing seeds first (02_06 already had
  33/200). A recipe that has not passed one seed is not a shortcut.
- **Next time:** measure the speed-up before building the second path. Two bake paths double every rule
  (`Generator`, scratch folder, pins) for an unmeasured gain.

## 2026-11-27 — Bake answer and bake parallel

- **Worked:** storing each job's answer turned the shipped check from a re-solve into a replay, and a
  settings change now names every job whose answer stopped landing.
- **Worked:** the fleet search already ran on a plain snapshot, so parallel scans were scheduling work,
  not a rewrite; one checkout on every core retired the second worktree.
- **Surprised:** a backfill that re-solves stores a stand-in, not the baked answer: it passed bounds by
  luck and could even pick a cube a fixed job's player can never place. A stored answer must be checked
  against the contract and, for a fixed job, sit on its frame.
- **Surprised:** reviews caught three answer bugs (stand-in zones, stale kind, wrong seed) before the
  backfill ran; running it first would have stored them.
- **Next time:** review a data migration before running it, and give derived numbers (wire count,
  fleet ids) a test that names the value to set instead of a hand-kept constant.

## 2026-11-27 — Chapter 2 bakes

- **Worked:** one list for every chapter 2 scan across two checkouts, each job taken from its own
  branch through git, kept nine bakes and their pins straight.
- **Worked:** stopping the long scans and rescanning only their candidate seeds saved hours (02_08,
  then 02_07, came from seed 65 of a 200-seed run).
- **Surprised:** both checkouts rewrite `Chapters/02.asset` and `PuzzleJobContracts.cs`, so every merge
  conflicted and the chapter order had to be set by hand afterwards.
- **Surprised:** renumbering to play order after the bakes touched ids, assets, contracts, lines and
  clips at once, and left every open plan's notes on the old ids.
- **Next time:** a bake writes only its own job (bake answer U4) and scans on every core in one
  checkout (bake parallel); fix ids to play order before the first bake.

## 2026-11-27 — Edge sun

- **Worked:** making the validator check where the sun is caught 02_06's above-field sun, committed
  under a `Centre` contract that nothing checked.
- **Surprised:** a height-only check passes a centred sun (y = 0 sits inside most bands); the edge needs
  the height band and a horizontal distance beyond the rim.
- **Surprised:** the map anchored on the sun in four places (focus, slide, route line, zoom pivot);
  moving the star off the centre needed one field-centre point instead.
- **Surprised:** the only mention of the edge sun was a card's fine print, and the playtest missed it;
  a change to the picture of a system needs Jim to say it.

## 2026-11-27 — Fleet rebake

- **Worked:** fleet contracts plus a stored-ships re-solve turned "won by shrinking the sun" from a
  headset surprise into a failing check.
- **Surprised:** at the centre the sun sits on every fleet corridor, so `NoSunAnswer` cannot be met
  there (48 + 24 seeds); the sun's place flipped centre → above → centre → edge in one day.
- **Surprised:** `Separate` is the relation enum's default, so an unset relation silently bakes the
  wrong lesson.
- **Next time:** settle the picture (where the star sits) on the headset before scanning seeds
  against it.

## 2026-11-27 — Chapter 2 intro

- **Worked:** scramble bounds as zero-default contract fields kept every existing contract passing.
- **Surprised:** fixed fleets failed a check written for the movable frame (the bounding-box centre
  sat on the shared origin); the fixed frame had no check of its own until then.
- **Surprised:** `MaxScaleDelta = 0.5` was set to admit the widest baked answer, so it screens nothing.
- **Next time:** a bound set from the answer it should screen is a record, not a check; set bounds by
  playing first.

## 2026-11-27 — Fleet capstone and fleet Clear

- **Worked:** classifying the confirmed fleet solution and rejecting `WrongRole` let a fleet ask for
  Clear; trying the probe's own 13 scales first made the re-solve exact by construction.
- **Surprised:** "two shared blockers" baked on none of 17 seeds, and the two Clear jobs that shipped
  share one contract, so the playtest read them (and the `Any` capstone after them) as one solution.
- **Surprised:** three-ship seeds are rare and slow, and a shipped three-ship job (02_06) stopped
  solving under a settings change with nothing to say so.
- **Next time:** two jobs need two different contracts; a ramp only the headset can tell apart
  ("decoys judged by playing") is one job twice. A capstone names the mechanism it must be solved by.

## 2026-11-25 — Fleet search

- **Worked:** splitting the bake at a plain snapshot let the probe run on a Task while the field and the
  variant stayed on the main thread; the synchronous bake and the background search share one
  implementation.
- **Surprised:** the editor dropping mid-scan was a recompile, not a cancel: the run must cancel, wait
  briefly for the worker and release its assets on `beforeAssemblyReload`, or the one-search gate stays
  locked.
- **Surprised:** a test that compares two constructors of one type proves nothing once one delegates to
  the other; assert the walk, the progress and the cancellation instead.
- **Surprised:** the shipped three-ship fleet (02-4) no longer solves in the bake's candidate space
  after the settings changed, so it cannot be a validating fixture; the headset check owns it.
- **Next time:** give a background run its release hook from the start — `onDone` is never called on a
  reload, so anything it would destroy leaks.

## 2026-11-25 — Story arc

- **Worked:** putting the arc on the seams — job 1's receipt, job 7's close, chapter 2's opener,
  2-4's close — changed five beats instead of rewriting eleven jobs.
- **Worked:** naming the damage as bodies (anchored, repulsor) gave chapter 3 a visible tell and the
  arc a concrete hand-off; the "eleven years" note moved to the final manual entry.
- **Surprised:** the sector wire is keyed to solved count, so inserting chapter 2's jobs shifts every
  later line; the 24-key tail is only right while the 23-job count holds.
- **Surprised:** changing a spoken line in place keeps its key and leaves the old mp3; the three clips
  had to be regenerated after the text changed.
- **Next time:** write the clip keys and the wire re-check into the plan before editing, not after.

## 2026-11-20 — Fleet sun answer

- **Worked:** diagnosing the red smoke test first turned the mystery into a seed problem: seed 2's field
  has no two-run rescue plateau, so the test bakes seed 13 and the bake is untouched.
- **Surprised:** at the centre the sun sits on every candidate corridor, so `NoSunAnswer` cannot be met
  by filtering runs; the field has to move the sun clear of the planet-pair chords.
- **Surprised:** the first off-corridor field that bakes is seed 17, not the no-sun seed 13 — a sun at
  the rim still perturbs the runs enough to change the answer.
- **Next time:** run `[Explicit]` Creator tests by name in a batchmode Unity run, and check the connected
  Unity project before trusting a zero-test bridge run.

## 2026-11-20 — Seeds retune

- **Worked:** running the failing job set before touching a seed turned a suspected retune into a
  one-line test fix: `PuzzleJobBakeTests.Seed` dropped `GrowOnly`, so 01_01 was the test's bug.
- **Surprised:** shipped jobs store neither their answer nor their kind, so an asset check has to
  re-solve; `PuzzleSolutionSearch` with a brief built from the contract was enough, and all seven
  shipped assets passed untouched.
- **Surprised:** once the settings changed, no seed reproduces the shipped planets. The retune pins a
  seed whose bake has the same shape (mechanism, zones, routes, sun and start rules), not the same
  field — the asset stays the content, the contract is a proxy.
- **Next time:** keep asset checks in the Creator suite; the validator and the contracts are
  Editor-only, so "runs in the fast suite" only relocates the cost.
- **Next time:** never pass `-quit` with `-runTests` in the Unity CLI; it quits after import and
  reports zero tests. A batch window runs the suite reliably.

## 2026-11-20 — Lattice on Burst

- **Worked:** profiler markers first, then reading the headset capture straight from the editor:
  one marker (`Shaping.Lattice.Warp`, ~19 ms) settled the cause and ruled out the re-sim and upload.
- **Worked:** porting the warp math to native read-only views kept one copy shared by Burst, the
  dust and the tests; warp 19 ms → ~2 ms across four workers, pinch holds 72/72 FPS.
- **Surprised:** with the profiler attached, frames still read 18–24 ms (mostly `XRUpdate` wait);
  the headset's own VrApi line showed a steady 72 FPS. Profiler frame time is not the delivered rate.
- **Surprised:** Burst jobs refuse unassigned containers, so "no wells" / "no baseline" are empty
  arrays, not default fields.
- **Next time:** judge Quest frame rate from `adb logcat -s VrApi` (FPS, Stale, GPU%, CPU%), and use
  the profiler only to find what costs the time.

## 2026-11-15 — Freeze day

- **Worked:** closing plans moved their headset checks into one list, so the pass was a single
  sitting instead of a check per feature.
- **Worked:** "no new features" held; the day's fixes were the pass's findings (map centering) and
  the store cover, nothing new.
- **Surprised:** the freeze slipped two days past its 2026-11-13 date; the 2026-11-18 deadline
  absorbed it.
- **Next time:** set the internal freeze with a buffer day for the headset pass itself.

## 2026-11-15 — No sun answer

- **Worked:** checking the rule against every answer, not just the chosen one; `OutOfZone` had let
  the sun win 1-4 through a second answer.
- **Surprised:** a one-ship check named 2-4 the sun job; requiring every ship to arrive showed the
  opposite (2-1 to 2-3 are sun wins, 2-4 is not).
- **Surprised:** in the two-ship layouts the sun is the only cube that rescues both ships, so no seed
  scan can fix chapter 2. A locked sun read as a bug.
- **Next time:** a field rule the generator does not plan for is a generator change, not a seed scan.
- **Next time:** long bakes run as cancellable background jobs, never through the editor bridge.

## 2026-11-15 — Sector wire

- **Worked:** one ordered wire keyed by the solved count told the story with no speaker, no TTS and no
  new UI; the count comes from the run, so chapter 2 grew the total for free.
- **Worked:** one "last seen count" cookie decides typed-once vs instant on every map return.
- **Surprised:** two chapters on one map needed a layout per chapter and its own centring, not a
  bigger spiral.
- **Next time:** a layout experiment lands with its revert path and its headset decision written down,
  so the plan can close before the read.

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

## 2026-11-25 — Dropped lessons: multi-zone fleets and Mixed

- **Surprised:** two planned chapter-3 lessons were not lessons. With a movable cube, "one zone per
  ship" is what the player already does in 2-1, and Mixed is what players already try when a job has
  more than one route. Both lived only in the validator.
- **Next time:** before planning a bake for a new lesson, ask what the player does differently. If
  the answer is "nothing, but the validator checks more", it is a harder version of an old job, not a
  chapter's idea.
- **Next time:** look for a lesson in what the cube can already do but has not been taught (a fixed
  cube with a fleet, resize) before inventing a combination.

## 2026-11-26 — Story arc and the job card

- **Worked:** putting the arc on four seams (job 1's receipt, job 7's close, chapter 2's opener, 2-4's
  close) instead of every job. Eleven jobs kept their lines and only three clips changed.
- **Worked:** measuring the card before cutting it. Characters typed per card and the typing speed
  gave the 6–8 second figure, and the fix (contract at once, one short manual line) followed from it.
- **Surprised:** anything derived from a job id breaks once jobs are inserted before older ids. The
  manual version read v2.0.9 on chapter 2's first card until it followed play order.
- **Surprised:** a key-stable line edit keeps the old mp3, so the headset plays the old line under the
  new subtitle until the clip is regenerated.
- **Next time:** derive anything the player reads in order (versions, wire lines) from play order or
  solved count, never from the id; and give each derived value one source, not one per view.

## 2026-11-26 — Chapter 2 lines

- **Worked:** owning the wire by chapter seams (7 + 10 + 6 keys) and pinning the seams in
  `SectorWireTests`. Inserting ten chapter 2 jobs moved lines once, with a test that catches the next
  shift.
- **Worked:** writing cards before the jobs baked. Missing keys show as empty text, so a job that
  lands later already has its card.
- **Surprised:** the first job's manual entry decides what every later entry may repeat. Once 02_09
  opened the chapter, 02_02's "bend near the start" and 02_01's "two ships, one budget" became repeats.
- **Next time:** keep the clip list where the batch reads it (the downloader scene's keys), not in a
  side file in the plans folder; and close a lines plan only with its clips or with the clips as a
  named open tail.
