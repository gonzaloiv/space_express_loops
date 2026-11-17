using DigitalLove.FlowControl;
using UnityEngine;
using Reflex.Attributes;
using DigitalLove.Audio;
using DigitalLove.Casual.Levels;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Game.UI.Console;
using DigitalLove.DataAccess;
using DigitalLove.Localization;
using DigitalLove.TTS;

namespace DigitalLove.Game.Flow
{
    public class PuzzleStartState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private SpaceShapingController puzzleController;
        [SerializeField] private LevelSelector levelSelector;
        [SerializeField] private ConsoleScreen console;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch enterCue;

        [Inject] private MemoryDataClient memoryDataClient;

        private bool active;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            puzzleController.HideAll();
            ttsPresenter.Stop();
        }

        public override void Enter()
        {
            active = true;
            PuzzleRestart restart = memoryDataClient.Get<PuzzleRestart>();
            if (restart.Value)
            {
                // A real restart: the old field goes, the console reports the reset, then the field
                // comes back fresh instead of respawning over the previous attempt.
                puzzleController.HideAll();
                console.PlayMessage(restart.ConsoleLines(), StartPuzzle);
                return;
            }

            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            if (!TryOpenChapter(puzzle))
                BeginJob(puzzle);
        }

        public override void Exit() => active = false;

        // The chapter opener is the episode's world beat, once per chapter: Jim presents the chapter
        // over its first job's field, then that job's own intro follows. A tutorial job keeps its goal
        // line before the field, so its opener still plays with no field in the room.
        private bool TryOpenChapter(PuzzleData puzzle)
        {
            ChapterData chapter = levelSelector.GetChapter(puzzle);
            if (!ttsPresenter.HasLines(chapter.IntroKey))
                return false;
            if (!RunProgressCookies.OpenChapter(memoryDataClient.Get<PlayerData>(), chapter.id))
                return false;

            if (puzzle.HasGoalLine)
            {
                puzzleController.HideAll();
                ttsPresenter.Play(new[] { chapter.IntroKey }, () => BeginJob(puzzle));
                return true;
            }

            string[] lines = puzzle.noIntro
                ? new[] { chapter.IntroKey }
                : new[] { chapter.IntroKey, puzzle.IntroLinesKey };
            ttsPresenter.Play(lines, ttsPresenter.HidePanel);
            StartPuzzle();
            return true;
        }

        private void BeginJob(PuzzleData puzzle)
        {
            if (!active)
                return;

            // Jim's panel leaves once he has spoken, so the job plays without it.
            if (puzzle.HasGoalLine)
            {
                // The tutorial says its goal before the field appears, so nothing covers the scaling.
                ttsPresenter.HidePanel();
                puzzleController.HideAll();
                console.PlayMessage(
                    new[] { LocalizationUtil.GetValue(puzzle.goalLineKey) }, StartPuzzle);
                return;
            }

            if (puzzle.noIntro)
                ttsPresenter.HidePanel();
            else
                ttsPresenter.Play(new[] { puzzle.IntroLinesKey }, ttsPresenter.HidePanel);
            StartPuzzle();
        }

        private void StartPuzzle()
        {
            if (!active)
                return;

            systemPlayer.Play(enterCue);
            puzzleController.Enter(memoryDataClient.Get<PuzzleData>());
            puzzleController.Glitch();
            parent.SetCurrentState(nextState.RouteId);
        }
    }
}
