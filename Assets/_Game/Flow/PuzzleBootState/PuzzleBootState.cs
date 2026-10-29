using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class PuzzleBootState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private SpaceShapingState puzzleState;

        public override void Enter() => levelContainer.SetRoomBasedPose(EnterPuzzle);

        public override void Exit() { }

        private void EnterPuzzle() => parent.SetCurrentState(puzzleState.RouteId);
    }
}
