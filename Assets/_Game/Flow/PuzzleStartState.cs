using DigitalLove.FlowControl;
using UnityEngine;
using DigitalLove.XR;
using Reflex.Attributes;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.DataAccess;

namespace DigitalLove.Game.Flow
{
    public class PuzzleStartState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private OVROriginBehaviour ovrOrigin;
        [SerializeField] private SpaceShapingController puzzleController;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            puzzleController.HideAll();
        }

        public override void Enter()
        {
            ovrOrigin.Setup();
            puzzleController.Enter(memoryDataClient.Get<PuzzleData>());
            parent.SetCurrentState(nextState.RouteId);
        }

        public override void Exit()
        {

        }
    }
}