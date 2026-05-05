using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Spaceships;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.UI;

namespace DigitalLove.Game.Flow
{
    public class EditionState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private MonoState newRoundState;
        [SerializeField] private LettersPanel lettersPanel;

        [Header("Debug")]
        [SerializeField] private GameSnapshot gameSnapshot;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            lettersPanel.Hide();
        }

        public override void Enter()
        {
            levelContainer.SpaceshipsSpawner.loopCreated += OnLoopCreated;
            levelContainer.SpaceshipsSpawner.loopComplete += OnLoopComplete;
            levelContainer.SpaceshipsSpawner.loopEditionButtonClicked += OnLoopEditionButtonClicked;

            gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            lettersPanel.ShowLetters(gameSnapshot.CurrentLetters, roundSelector.TotalLettersToComplete);
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
            if (args.IsBaseLoop)
            {
                gameSnapshot.IncreaseLetters(args.value);
                lettersPanel.ShowLetters(gameSnapshot.CurrentLetters, roundSelector.TotalLettersToComplete);
                if (roundSelector.IsRoundComplete(gameSnapshot.store))
                    parent.SetCurrentState(newRoundState.RouteId);
            }
            else
            {
                levelContainer.PlanetsSpawner.GetById(args.originId).PlanetStore.IncreaseLetters(args.value);
            }
        }

        private void OnLoopEditionButtonClicked(LoopEventArgs args)
        {
            gameSnapshot.RemoveLoopBySpaceshipId(args.spaceshipId);
        }

        public override void Exit()
        {
            levelContainer.SpaceshipsSpawner.loopCreated -= OnLoopCreated;
            levelContainer.SpaceshipsSpawner.loopComplete -= OnLoopComplete;
            levelContainer.SpaceshipsSpawner.loopEditionButtonClicked -= OnLoopEditionButtonClicked;
        }
    }
}