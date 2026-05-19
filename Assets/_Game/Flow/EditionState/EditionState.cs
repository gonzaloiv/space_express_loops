using System.Collections.Generic;
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
        [SerializeField] private MonoState gameEndState;
        [SerializeField] private TTSHelper ttsHelper;

        [Header("Analytics")]
        [SerializeField] private ProgressionEventsHelper progressionEventsHelper;
        [SerializeField] private SessionEventsHelper sessionEventsHelper;

        [Header("Economy")]
        [SerializeField] private StorePanel storePanel;
        [SerializeField] private IntValue moneyPerLetter;

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
            PlanetRouteColorSync.Apply(
                gameSnapshot,
                levelContainer.PlanetsSpawner,
                levelContainer.HubsSpawner,
                levelContainer.SpaceshipsSpawner);
            progressionEventsHelper.SendLevelStartedEvent(roundSelector.CurrentRound.id);
            ShowFTUIndicators();
        }

        private void RefreshStoreUI()
        {
            storePanel.Show(gameSnapshot.CurrentLetters, gameSnapshot.lettersRequiredForRoundCompletion, gameSnapshot.store.money);
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
                destinationIds = args.destinationIds != null ? new List<string>(args.destinationIds) : new(),
                colorCode = args.colorCode,
                hubId = args.hubId
            };
            gameSnapshot.AddLoop(data);

            PlanetRouteColorSync.Apply(
                gameSnapshot,
                levelContainer.PlanetsSpawner,
                levelContainer.HubsSpawner,
                levelContainer.SpaceshipsSpawner);

            sessionEventsHelper.Send("loop_created");
        }

        private void OnLoopComplete(LoopCompleteEventArgs args)
        {
            if (args.HasFailed)
                return;

            HandleBaseLoopCompletion(args.value);
        }

        private void HandleBaseLoopCompletion(int loopValue)
        {
            gameSnapshot.IncreaseLettersAndMoney(loopValue, moneyPerLetter.value * loopValue);
            RefreshStoreUI();

            if (gameSnapshot.IsCurrentRoundLetterGoalMet)
                parent.SetCurrentState(newRoundState.RouteId);
        }

        private void OnLoopEditionButtonClicked(LoopEventArgs args)
        {
            gameSnapshot.ClearLoopDestinations(args.spaceshipId);

            PlanetRouteColorSync.Apply(
                gameSnapshot,
                levelContainer.PlanetsSpawner,
                levelContainer.HubsSpawner,
                levelContainer.SpaceshipsSpawner);
        }

        private void OnPlanetFull()
        {
            parent.SetCurrentState(gameEndState.RouteId);
        }

        public override void Exit()
        {
            UnsubscribeEvents();
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

        [Button]
        public void Debug_Restart() => OnPlanetFull();
    }
}
