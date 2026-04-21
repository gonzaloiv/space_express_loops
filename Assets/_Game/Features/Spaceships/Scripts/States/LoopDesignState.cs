using DigitalLove.FlowControl;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class LoopDesignState : MonoState
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private GhostBehaviour ghost;

        public override void Enter()
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }

        public override void Exit()
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (pointer.Type == PointerEventType.Unselect)
                OnUnselect();
        }

        private void OnUnselect()
        {
            ghost.SetActive(false);
            parent.SetCurrentState<OnRouteState>();
        }
    }
}