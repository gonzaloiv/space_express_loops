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
        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private GameObject grabMePanel;
        [SerializeField] private AudioSource grabAudioSource;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            grabMePanel.SetActive(false);
        }

        public override void Enter()
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;

            grabbableBody.Show();
            destinationSelector.StartLookingForDestination(false);
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
            grabMePanel.SetActive(false);
            grabAudioSource.Play();
        }

        public void ShowGrabMePanel()
        {
            grabMePanel.SetActive(true);
        }
    }
}