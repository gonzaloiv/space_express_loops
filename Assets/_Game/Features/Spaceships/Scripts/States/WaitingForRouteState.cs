using System;
using DigitalLove.FlowControl;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class WaitingForRouteState : MonoState
    {
        [SerializeField] private GameObject grabMePanel;
        [SerializeField] private AudioSource grabAudioSource;
        [SerializeField] private GrabbableWrapper grabbableWrapper;
        [SerializeField] private DestinationSelector destinationSelector;

        private Action startSelectingDestination;

        public void Bind(Action startSelectingDestination) =>
            this.startSelectingDestination = startSelectingDestination;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            grabMePanel.SetActive(false);
        }

        public override void Enter()
        {
            grabbableWrapper.SubscribePointerEvents(OnPointerEvent);
            grabbableWrapper.Show();
            destinationSelector.StartLookingForDestination(false);
        }

        public override void Exit()
        {
            grabbableWrapper.UnsubscribePointerEvents(OnPointerEvent);
            grabbableWrapper.Hide();
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (pointer.Type == PointerEventType.Select)
                OnSelect();
        }

        [Button]
        private void OnSelect()
        {
            grabMePanel.SetActive(false);
            grabAudioSource.Play();
            startSelectingDestination();
        }

        public void ShowGrabMePanel() => grabMePanel.SetActive(true);
    }
}
