using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipBehaviour : MonoBehaviour
    {
        [SerializeField] private MonoState[] states;

        private StateMachine stateMachine;

        private void Awake()
        {
            stateMachine = StateMachineFactory.Create(states);
            stateMachine.SetCurrentState<WaitingForRouteState>();
        }
    }
}