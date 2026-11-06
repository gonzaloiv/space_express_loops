using DigitalLove.FlowControl;
using UnityEngine;
using DigitalLove.XR;
using Reflex.Attributes;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.DataAccess;
using DigitalLove.TTS;

namespace DigitalLove.Game.Flow
{
    public class PuzzleStartState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private OVROriginBehaviour ovrOrigin;
        [SerializeField] private SpaceShapingController puzzleController;
        [SerializeField] private TtsPresenter ttsPresenter;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            puzzleController.HideAll();
            ttsPresenter.Stop();
        }

        public override void Enter()
        {
            ovrOrigin.Setup();
            PuzzleData puzzle = memoryDataClient.Get<PuzzleData>();
            puzzleController.Enter(puzzle);
            ttsPresenter.Play(new[]
            {
                puzzle.IntroKey + "_manual_entry",
                puzzle.IntroKey + "_01",
            });
            parent.SetCurrentState(nextState.RouteId);
        }

        public override void Exit()
        {

        }
    }
}