using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Levels;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class LevelInitState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;

        [Inject] private MemoryDataClient memoryDataClient;

        private GamePlay gamePlay;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            levelContainer.HideAll();
        }

        public override void Enter()
        {
            if (gamePlay == null)
            {
                gamePlay = new GamePlay();
                memoryDataClient.Put(gamePlay);
            }
            gamePlay.Reset();
            levelContainer.Spawn();
            parent.SetCurrentState<EditionState>();
        }

        public override void Exit()
        {

        }
    }
}