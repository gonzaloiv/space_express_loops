using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Levels;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class EditionState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;

        [Inject] private MemoryDataClient memoryDataClient;

        private GamePlay gamePlay;

        public override void Enter()
        {
            levelContainer.SpaceshipsSpawner.Current.SetOnLoopComplete(OnLoopComplete);
            gamePlay = memoryDataClient.Get<GamePlay>();
        }

        private void OnLoopComplete(int letters)
        {
            gamePlay.IncreaseLetters(letters);
            levelContainer.PlanetsSpawner.BasePlanet.ShowLetters(gamePlay.CurrentLetters);
        }

        public override void Exit()
        {
            levelContainer.SpaceshipsSpawner.Current.SetOnLoopComplete(null);
        }
    }
}