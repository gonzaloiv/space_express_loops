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
using System;

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
            StartPlanetStoresIfLoopsExist();
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
            ttsHelper.SetInFrontOfCameraOrDefault(false);
            void SayRoundIntro(Action onComplete) => ttsHelper.SayRoundIntro(roundSelector.CurrentRound, onComplete);
            if (roundSelector.IsFirstRound)
            {
                levelContainer.SpaceshipsSpawner.All[0].ShowGrabMePanel();
                SayRoundIntro(() =>
                {
                    ttsHelper.SayAfter(2.5f, "how_to_create_a_route", () => { });
                });
            }
            else
            {
                SayRoundIntro(() => { });
            }
        }

        private void OnLoopChanged(LoopEventArgs args)
        {
            gameSnapshot.SaveLoop(ToLoopData(args));
            ApplyRouteColors();
            StartPlanetStoresIfLoopsExist();
            sessionEventsHelper.Send("loop_created");
        }

        private void StartPlanetStoresIfLoopsExist()
        {
            if (!gameSnapshot.HasAnyLoopWithDestinations)
                return;

            levelContainer.PlanetsSpawner.UnlockPlanetStores();
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
            gameSnapshot.SaveLoop(ToLoopData(args));
            ApplyRouteColors();
        }

        private static LoopData ToLoopData(LoopEventArgs args) => new()
        {
            spaceshipId = args.spaceshipId,
            destinationIds = args.destinationIds != null ? new List<string>(args.destinationIds) : new(),
            colorCode = args.colorCode,
            hubId = args.hubId
        };

        private void ApplyRouteColors()
        {
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
            levelContainer.SpaceshipsSpawner.loopChanged += OnLoopChanged;
            levelContainer.SpaceshipsSpawner.loopComplete += OnLoopComplete;
            levelContainer.SpaceshipsSpawner.loopEditionButtonClicked += OnLoopEditionButtonClicked;
            levelContainer.PlanetsSpawner.planetFull += OnPlanetFull;
        }

        private void UnsubscribeEvents()
        {
            levelContainer.SpaceshipsSpawner.loopChanged -= OnLoopChanged;
            levelContainer.SpaceshipsSpawner.loopComplete -= OnLoopComplete;
            levelContainer.SpaceshipsSpawner.loopEditionButtonClicked -= OnLoopEditionButtonClicked;
            levelContainer.PlanetsSpawner.planetFull -= OnPlanetFull;
        }

        [Button]
        public void Debug_Restart() => OnPlanetFull();
    }
}
