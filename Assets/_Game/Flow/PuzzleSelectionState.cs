using DigitalLove.Audio;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.GesturePrompts;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Global;
using DigitalLove.TTS;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class PuzzleSelectionState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private LevelSelector levelSelector;
        [SerializeField] private PuzzleMapView view;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private GesturePromptPresenter prompts;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch selectCue;
        [SerializeField] private SynthPatch openCue;
        [SerializeField] private SynthPatch typeCue;
        [SerializeField] private JobCardPanel card;
        [SerializeField, Min(0f)] private float tickCooldownSecs = 0.06f;

        [Header("Debug")]
        [SerializeField] private DebugBool selectRandom;
        [SerializeField] private PuzzleData puzzleToSet;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private PlayerDataProvider playerDataProvider;

        private PuzzleMap map;
        private bool autoEntryUsed;

        public override void Enter()
        {
            ttsPresenter.Stop();
            view.SelectionChanged += OnSelectionChanged;
            card.Opened += OnCardOpened;
            card.LineTyped += OnLineTyped;
            ApplyPendingCloud();
            PlayerData playerData = memoryDataClient.Get<PlayerData>();

            if (selectRandom.Value)
            {
                EnterJob(levelSelector.GetLevelByGlobalIndex<PuzzleData>(
                    Random.Range(0, levelSelector.TotalLevelsCount)));
                return;
            }

            if (puzzleToSet != null)
            {
                EnterJob(puzzleToSet);
                return;
            }

            RunProgress progress = RunProgressCookies.Build(levelSelector, playerData);
            if (progress.IsFresh && !autoEntryUsed)
            {
                autoEntryUsed = true;
                EnterJob(levelSelector.GetLevelByGlobalIndex<PuzzleData>(0));
                return;
            }

            ShowMap(progress);
        }

        private void ApplyPendingCloud()
        {
            PlayerData pending = playerDataProvider.ApplyPendingCloud();
            if (pending != null)
                memoryDataClient.Put(pending);
        }

        private void ShowMap(RunProgress progress)
        {
            view.EnterRequested += OnEnterRequested;

            PuzzleData[] levels = ReadLevels();
            map = PuzzleMap.FromLevels(
                levels, progress, PuzzleMapLayout.Galaxy(levels.Length));
            view.Show(map);
            prompts.Show(GestureKind.Enter);
        }

        public override void Exit()
        {
            view.EnterRequested -= OnEnterRequested;
            view.SelectionChanged -= OnSelectionChanged;
            card.Opened -= OnCardOpened;
            card.LineTyped -= OnLineTyped;
            prompts.HideAll();

            if (map == null)
                return;

            view.Hide();
            map = null;
        }

        private void OnEnterRequested()
        {
            if (!map.CanEnter)
            {
                view.Refuse();
                return;
            }

            EnterJob(map.SelectedPuzzle);
        }

        private void EnterJob(PuzzleData puzzle)
        {
            memoryDataClient.Put(puzzle);
            parent.SetCurrentState(nextState.RouteId);
        }

        private void OnSelectionChanged() => Tick(selectCue);

        private void OnCardOpened() => Tick(openCue);

        private void OnLineTyped() => Tick(typeCue);

        private void Tick(SynthPatch patch) => systemPlayer.PlayTick(patch, tickCooldownSecs);

        private PuzzleData[] ReadLevels()
        {
            int count = levelSelector.TotalLevelsCount;
            PuzzleData[] levels = new PuzzleData[count];
            for (int i = 0; i < count; i++)
                levels[i] = levelSelector.GetLevelByGlobalIndex<PuzzleData>(i);

            return levels;
        }
    }
}
