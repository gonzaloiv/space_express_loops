using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingMain : MonoBehaviour
    {
        [SerializeField] private SpaceShapingController state;

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
