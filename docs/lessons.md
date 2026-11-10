# Lessons

Short post-mortems, written when a plan closes (rule in [AGENTS.md](../AGENTS.md) §Plans are a
morning). Newest first. Each entry: what worked, what surprised us, the rule for next time. One idea per
bullet; the code and tests are the record of what was built.

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
