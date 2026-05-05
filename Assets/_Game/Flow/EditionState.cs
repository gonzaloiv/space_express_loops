using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Spaceships;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.UI;
using DigitalLove.Global;

namespace DigitalLove.Game.Flow
{
    public class EditionState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private MonoState newRoundState;

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
            gameSnapshot.SpendMoney(routeEditionCost.value);
            gameSnapshot.RemoveLoopBySpaceshipId(args.spaceshipId);
        }

        private void OnPlanetSetColorButtonClicked(string id)
        {
            gameSnapshot.SpendMoney(planetColorCost.value);
            levelContainer.PlanetsSpawner.GetById(id).PlanetBody.SetRandomColor();
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