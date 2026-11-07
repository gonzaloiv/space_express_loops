using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.SpaceShaping;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Global;
using DigitalLove.TTS;
using DigitalLove.Game.GesturePrompts;

namespace DigitalLove.Game.Flow
{
    public class PuzzleSelectionState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private LevelSelector levelSelector;
        [SerializeField] private PuzzleMapView view;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private GesturePromptPresenter prompts;

        [Header("Debug")]
        [SerializeField] private DebugBool selectRandom;
        [SerializeField] private PuzzleData puzzleToSet;

        [Inject] private MemoryDataClient memoryDataClient;

        private PuzzleMap map;
        private bool autoEntryUsed;

        public override void Enter()
        {
            ttsPresenter.Stop();
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
            prompts.HideAll();

            if (map == null)
                return;

            view.Hide();
            map = null;
        }

        private void OnEnterRequested()
        {
            Debug.Log(
                $"[PuzzleSelection] enter can={map.CanEnter} index={map.SelectedIndex} " +
                $"state={map.Selected.State} puzzle={(map.SelectedPuzzle == null ? "null" : map.SelectedPuzzle.name)} " +
                $"next={nextState.RouteId}",
                this);

            if (!map.CanEnter)
            {
                view.Refuse();
                return;
            }

            EnterJob(map.SelectedPuzzle);
        }

        private void EnterJob(PuzzleData puzzle)
        {
            Debug.Log(
                $"[PuzzleSelection] route {nextState.RouteId} puzzle={(puzzle == null ? "null" : puzzle.name)}",
                this);
            memoryDataClient.Put(puzzle);
            parent.SetCurrentState(nextState.RouteId);
        }

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
