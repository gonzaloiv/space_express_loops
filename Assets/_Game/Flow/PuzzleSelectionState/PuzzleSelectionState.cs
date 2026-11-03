using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Input;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Global;

namespace DigitalLove.Game.Flow
{
    public class PuzzleSelectionState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private LevelSelector levelSelector;
        [SerializeField] private RunCarouselView view;
        [SerializeField] private TwoHandGestureInput gesture;

        [Header("Debug")]
        [SerializeField] private bool skipMapOnFirstLaunch = true;
        [SerializeField] private DebugBool selectRandom;

        [Inject] private MemoryDataClient memoryDataClient;

        private RunCarousel carousel;

        public override void Enter()
        {
            view.FocusRequested += OnFocusRequested;
            view.EnterRequested += OnEnterRequested;
            gesture.StateChanged += OnGestureChanged;

            PlayerData playerData = memoryDataClient.Get<PlayerData>();

            if (skipMapOnFirstLaunch && playerData.GetLevelCompleteCookies().Count == 0)
            {
                EnterJob(levelSelector.GetLevelByGlobalIndex<PuzzleData>(0));
                return;
            }

            if (selectRandom.Value)
            {
                EnterJob(levelSelector.GetLevelByGlobalIndex<PuzzleData>(Random.Range(0, levelSelector.TotalLevelsCount)));
                return;
            }

            RunProgress progress = RunProgressCookies.Build(levelSelector, playerData);
            carousel = RunCarousel.FromLevels(ReadLevels(), progress);
            view.Show(carousel);
        }

        public override void Exit()
        {
            view.FocusRequested -= OnFocusRequested;
            view.EnterRequested -= OnEnterRequested;
            gesture.StateChanged -= OnGestureChanged;

            if (carousel == null)
                return;

            view.Hide();
            carousel = null;
        }

        private void OnGestureChanged(TwoHandGestureState state) => view.ApplyGesture(state);

        private void OnFocusRequested(int index)
        {
            if (carousel.Focus(index))
                view.Show(carousel);
        }

        private void OnEnterRequested()
        {
            if (!carousel.CanEnter)
            {
                view.Refuse();
                return;
            }

            EnterJob(carousel.FocusedPuzzle);
        }

        private void EnterJob(PuzzleData puzzle)
        {
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
