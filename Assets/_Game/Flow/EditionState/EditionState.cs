using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Spaceships;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.UI;
using DigitalLove.Global;
using DigitalLove.Casual.Analytics;
using DigitalLove.Analytics;
using DigitalLove.Game.TTS;

namespace DigitalLove.Game.Flow
{
    public class EditionState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private MonoState newRoundState;
        [SerializeField] private StoreDependentUI storeDependentUI;
        [SerializeField] private TTSHelper ttsHelper;

        [Header("Analytics")]
        [SerializeField] private ProgressionEventsHelper progressionEventsHelper;
        [SerializeField] private SessionEventsHelper sessionEventsHelper;

        [Header("Economy")]
        [SerializeField] private StorePanel storePanel;
        [SerializeField] private IntValue routeEditionCost;
        [SerializeField] private IntValue moneyPerLetter;
        [SerializeField] private IntValue brokenSpaceshipCost;
        [SerializeField] private IntValue planetFullFee;

        [Header("Debug")]
        [SerializeField] private GameSnapshot gameSnapshot;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            storePanel.Hide();
        }

        public override void Enter()
        {
            SubscribeEvents();

            gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            RefreshStoreUI();
            progressionEventsHelper.SendLevelStartedEvent(roundSelector.CurrentRound.id);
            storeDependentUI.DoStart(gameSnapshot);
            ShowFTUIndicators();
        }

        private void RefreshStoreUI()
        {
            storePanel.Show(gameSnapshot.CurrentLetters, roundSelector.TotalLettersToComplete, gameSnapshot.store.money);
        }

        private void ShowFTUIndicators()
        {
            Debug.LogWarning("FormattedCurrentRoundIndex " + roundSelector.FormattedCurrentRoundIndex);
            if (roundSelector.IsFirstRound)
            {
                levelContainer.SpaceshipsSpawner.All[0].ShowGrabMePanel();
                ttsHelper.SetInFrontOfCameraOrDefault(false);
                ttsHelper.SayAfter(2.5f, "the_hub_intro", SayHowToCreateARoute);
                void SayHowToCreateARoute()
                {
                    ttsHelper.SayAfter(2.5f, "how_to_create_a_route", () => { });
                }
            }
        }


        private void OnLoopCreated(LoopEventArgs args)
        {
            LoopData data = new()
            {
                spaceshipId = args.spaceshipId,
                originId = args.originId,
                destinationId = args.destinationId,
                colorCode = args.colorCode
            };
            gameSnapshot.AddLoop(data);

            PlanetRouteColorSync.Apply(
                gameSnapshot,
                levelContainer.PlanetsSpawner,
                levelContainer.SpaceshipsSpawner);

            sessionEventsHelper.Send("loop_created");
        }

        private void OnLoopComplete(LoopCompleteEventArgs args)
        {
            if (args.HasFailed) // ? Broken spaceship
            {
                HandleBrokenSpaceshipLoop(args.value);
                return;
            }

            if (args.IsBaseLoop)
            {
                HandleBaseLoopCompletion(args.value);
                return;
            }

            HandlePlanetLoopCompletion(args.originId, args.value);
        }

        private void HandleBrokenSpaceshipLoop(int loopValue)
        {
            gameSnapshot.SpendMoney(brokenSpaceshipCost.value + moneyPerLetter.value * loopValue);
            RefreshStoreUI();
        }

        private void HandleBaseLoopCompletion(int loopValue)
        {
            gameSnapshot.IncreaseLettersAndMoney(loopValue, moneyPerLetter.value * loopValue);
            RefreshStoreUI();

            if (roundSelector.IsRoundComplete(gameSnapshot.store))
                parent.SetCurrentState(newRoundState.RouteId);
        }

        private void HandlePlanetLoopCompletion(string originId, int loopValue)
        {
            levelContainer.PlanetsSpawner.GetById(originId).PlanetStore.IncreaseLetters(loopValue);
        }

        private void OnLoopEditionButtonClicked(LoopEventArgs args)
        {
            gameSnapshot.ClearLoopDestination(args.spaceshipId, routeEditionCost.value);
            RefreshStoreUI();

            PlanetRouteColorSync.Apply(
                gameSnapshot,
                levelContainer.PlanetsSpawner,
                levelContainer.SpaceshipsSpawner);
        }

        private void OnPlanetFull()
        {
            gameSnapshot.SpendMoney(planetFullFee.value);
            RefreshStoreUI();
        }

        public override void Exit()
        {
            UnsubscribeEvents();
            storeDependentUI.DoStop();
        }

        private void SubscribeEvents()
        {
            levelContainer.SpaceshipsSpawner.loopCreated += OnLoopCreated;
            levelContainer.SpaceshipsSpawner.loopComplete += OnLoopComplete;
            levelContainer.SpaceshipsSpawner.loopEditionButtonClicked += OnLoopEditionButtonClicked;
            levelContainer.PlanetsSpawner.planetFull += OnPlanetFull;
        }

        private void UnsubscribeEvents()
        {
            levelContainer.SpaceshipsSpawner.loopCreated -= OnLoopCreated;
            levelContainer.SpaceshipsSpawner.loopComplete -= OnLoopComplete;
            levelContainer.SpaceshipsSpawner.loopEditionButtonClicked -= OnLoopEditionButtonClicked;
            levelContainer.PlanetsSpawner.planetFull -= OnPlanetFull;
        }
    }
}
