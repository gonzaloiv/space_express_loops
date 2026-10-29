using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingFlow : MonoBehaviour
    {
        [SerializeField] private SpaceShapingState state;

        private StateMachine machine;

        private void Start()
        {
            machine = new StateMachine();
            state.Init(machine);
            machine.Register(state);
            machine.SetCurrentState(state.RouteId);
        }
    }
}
