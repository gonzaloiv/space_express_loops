using UnityEngine;
using DigitalLove.FlowControl;
using DigitalLove.DataAccess;
using DigitalLove.Game.Levels;
using Reflex.Attributes;

namespace DigitalLove.Game.Flow
{
    public class NewRoundState : MonoState
    {
        [SerializeField] private MonoState editionState;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private LevelContainer levelContainer;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Enter()
        {
            GameSnapshot gameSnapshot = memoryDataClient.Get<GameSnapshot>();
            gameSnapshot.IncreaseRoundIndex();
            roundSelector.SetCurrentRound(gameSnapshot.roundIndex);

            levelContainer.SpawnRound(roundSelector.CurrentRound, gameSnapshot);

            parent.SetCurrentState(editionState.RouteId);
        }

        public override void Exit()
        {

        }
    }
}