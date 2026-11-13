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

        // The chapter opener is the episode's world beat, its own moment before the job: Jim speaks
        // with no field in the room, once per chapter, then the job starts with its own lesson.
        private bool TryOpenChapter(PuzzleData puzzle)
        {
            ChapterData chapter = levelSelector.GetChapter(puzzle);
            if (!ttsPresenter.HasLines(chapter.IntroKey))
                return false;
            if (!RunProgressCookies.OpenChapter(memoryDataClient.Get<PlayerData>(), chapter.id))
                return false;

            puzzleController.HideAll();
            ttsPresenter.Play(new[] { chapter.IntroKey }, () => BeginJob(puzzle));
            return true;
        }

        private void BeginJob(PuzzleData puzzle)
        {
            if (!active)
                return;

            if (puzzle.HasGoalLine)
            {
                // The tutorial says its goal before the field appears, so nothing covers the scaling.
                puzzleController.HideAll();
                console.PlayMessage(
                    new[] { LocalizationUtil.GetValue(puzzle.goalLineKey) }, StartPuzzle);
                return;
            }

            if (!puzzle.noIntro)
                ttsPresenter.Play(new[] { puzzle.IntroLinesKey });
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
