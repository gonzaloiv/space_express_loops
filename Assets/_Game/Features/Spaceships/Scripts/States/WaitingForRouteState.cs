using System;
using DigitalLove.FlowControl;
using DigitalLove.Global;
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
            grabbableWrapper.EnablePointerHandling();
            grabbableWrapper.selected += OnSelect;
            grabbableWrapper.Show();
            destinationSelector.StartLookingForDestination(false);
        }

        public override void Exit()
        {
            grabbableWrapper.selected -= OnSelect;
            grabbableWrapper.Hide();
        }

        [Button]
        private void OnSelect()
        {
            grabMePanel.SetActive(false);
            grabAudioSource.Play();
            startSelectingDestination();
        }

        public void ShowGrabMePanel() => grabMePanel.SetActive(true);

        #region Debug

        public void Debug_SimulateGrabSelect() => OnSelect();

        #endregion
    }
}
