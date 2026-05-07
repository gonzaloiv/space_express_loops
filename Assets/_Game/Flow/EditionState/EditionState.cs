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
using DigitalLove.Game.Planets;

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
        [SerializeField] private IntValue planetColorCost;

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
            levelContainer.SpaceshipsSpawner.loopCreated += OnLoopCreated;
            levelContainer.SpaceshipsSpawner.loopComplete += OnLoopComplete;
            levelContainer.SpaceshipsSpawner.loopEditionButtonClicked += OnLoopEditionButtonClicked;
            levelContainer.PlanetsSpawner.planetSetColorButtonClicked += OnPlanetSetColorButtonClicked;

            gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            storePanel.Show(gameSnapshot.CurrentLetters, roundSelector.TotalLettersToComplete, gameSnapshot.store.money);
            progressionEventsHelper.SendLevelStartedEvent(roundSelector.CurrentRound.id);

            ShowFTUIndicators();
        }

        private void OnLoopCreated(LoopEventArgs args)
        {
            LoopData data = new()
            {
                spaceshipId = args.spaceshipId,
                originId = args.originId,
                destinationId = args.destinationId
            };
            gameSnapshot.AddLoop(data);

            sessionEventsHelper.Send("loop_created");
        }

        private void OnLoopComplete(LoopCompleteEventArgs args)
        {
            if (string.IsNullOrEmpty(args.destinationId)) // ? Broken spaceship
            {
                gameSnapshot.SpendMoney(brokenSpaceshipCost.value);
            }
            else
            {
                if (args.IsBaseLoop)
                {
                    gameSnapshot.IncreaseLettersAndMoney(args.value, moneyPerLetter.value * args.value);
                    storePanel.Show(gameSnapshot.CurrentLetters, roundSelector.TotalLettersToComplete, gameSnapshot.store.money);
                    if (roundSelector.IsRoundComplete(gameSnapshot.store))
                        parent.SetCurrentState(newRoundState.RouteId);
                }
                else
                {
                    levelContainer.PlanetsSpawner.GetById(args.originId).PlanetStore.IncreaseLetters(args.value);
                }
            }
        }

        private void OnLoopEditionButtonClicked(LoopEventArgs args)
        {
            gameSnapshot.RemoveLoopBySpaceshipId(args.spaceshipId, routeEditionCost.value);
        }

        private void OnPlanetSetColorButtonClicked(string id)
        {
            PlanetBody planetBody = levelContainer.PlanetsSpawner.GetById(id).PlanetBody;
            planetBody.SetRandomTextureOffset();
            SerializableVector2 color = SerializableVector2.FromVector2(planetBody.TextureOffset);
            gameSnapshot.SetPlanetColor(id, color, planetColorCost.value);
        }

        private void ShowFTUIndicators()
        {
            if (roundSelector.IsFirstRound)
            {
                levelContainer.SpaceshipsSpawner.All[0].ShowGrabMePanel();
                ttsHelper.SayAfter(4, "the_hub_intro", SayHowToCreateARoute);
                void SayHowToCreateARoute() => ttsHelper.SayAfter(4, "how_to_create_a_route", () => { });
            }
        }

        public override void Exit()
        {
            levelContainer.SpaceshipsSpawner.loopCreated -= OnLoopCreated;
            levelContainer.SpaceshipsSpawner.loopComplete -= OnLoopComplete;
            levelContainer.SpaceshipsSpawner.loopEditionButtonClicked -= OnLoopEditionButtonClicked;
            levelContainer.PlanetsSpawner.planetSetColorButtonClicked -= OnPlanetSetColorButtonClicked;
        }
    }
}