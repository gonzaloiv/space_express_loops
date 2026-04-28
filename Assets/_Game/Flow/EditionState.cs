using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Spaceships;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class EditionState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;

        [Header("Debug")]
        [SerializeField] private GameSnapshot gameSnapshot;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Enter()
        {
            levelContainer.SpaceshipsSpawner.Current.SetOnLoopCreated(OnLoopCreated);
            levelContainer.SpaceshipsSpawner.Current.SetOnLoopComplete(OnLoopComplete);

            gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            
            levelContainer.PlanetsSpawner.BasePlanet.ShowLetters(gameSnapshot.CurrentLetters);
        }

        private void OnLoopCreated(LoopData data)
        {
            gameSnapshot.AddLoop(data);
        }

        private void OnLoopComplete(int letters)
        {
            gameSnapshot.IncreaseLetters(letters);
            levelContainer.PlanetsSpawner.BasePlanet.ShowLetters(gameSnapshot.CurrentLetters);
        }

        public override void Exit()
        {
            levelContainer.SpaceshipsSpawner.Current.SetOnLoopCreated(null);
            levelContainer.SpaceshipsSpawner.Current.SetOnLoopComplete(null);
        }
    }
}