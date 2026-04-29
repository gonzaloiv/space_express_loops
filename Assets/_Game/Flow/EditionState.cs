using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Spaceships;
using Reflex.Attributes;
using UnityEngine;
using DigitalLove.Game.Persistence;

namespace DigitalLove.Game.Flow
{
    public class EditionState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private MonoState newRoundState;

        [Header("Debug")]
        [SerializeField] private GameSnapshot gameSnapshot;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Enter()
        {
            levelContainer.SpaceshipsSpawner.SetOnLoopCreated(OnLoopCreated);
            levelContainer.SpaceshipsSpawner.SetOnLoopComplete(OnLoopComplete);

            gameSnapshot = memoryDataClient.Get<GameSnapshot>();
        }

        private void OnLoopCreated(LoopCreatedEventArgs args)
        {
            LoopData data = new()
            {
                spaceshipId = args.spaceshipId,

                destinationId = args.destinationId
            };
            gameSnapshot.AddLoop(data);
        }

        private void OnLoopComplete(int value)
        {
            gameSnapshot.IncreaseLetters(value);
            levelContainer.PlanetsSpawner.SpawnBase(gameSnapshot.CurrentLetters, roundSelector.CurrentRound.lettersToComplete);
            if (roundSelector.IsRoundComplete(gameSnapshot.store))
                parent.SetCurrentState(newRoundState.RouteId);
        }

        public override void Exit()
        {
            levelContainer.SpaceshipsSpawner.SetOnLoopCreated(null);
            levelContainer.SpaceshipsSpawner.SetOnLoopComplete(null);
        }
    }
}