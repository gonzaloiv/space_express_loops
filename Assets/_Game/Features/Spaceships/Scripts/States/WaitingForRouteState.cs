using DigitalLove.FlowControl;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class WaitingForRouteState : MonoState
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private GrabbableBody grabbableBody;
        [SerializeField] private BezierRay bezierRay;

        public override void Enter()
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;

            grabbableBody.Show();
            bezierRay.SetActive(false);
            grabbable.SetActive(true);
        }

        public override void Exit()
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;

            grabbableBody.Hide();
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (pointer.Type == PointerEventType.Select)
                OnSelect();
        }

        [Button]
        private void OnSelect()
        {
            parent.SetCurrentState<DestinationSelectionState>();
        }
    }
}