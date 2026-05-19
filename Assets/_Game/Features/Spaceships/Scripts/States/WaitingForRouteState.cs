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

        private SpaceshipBehaviour spaceship;
        private SpaceshipPresentation presentation;

        public void Bind(SpaceshipBehaviour spaceship)
        {
            this.spaceship = spaceship;
            presentation = spaceship.Presentation;
        }

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            grabMePanel.SetActive(false);
        }

        public override void Enter()
        {
            presentation.Grabbable.WhenPointerEventRaised += OnPointerEvent;
            presentation.ShowHubIdle();
        }

        public override void Exit()
        {
            presentation.Grabbable.WhenPointerEventRaised -= OnPointerEvent;
            presentation.HideHubIdle();
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
            spaceship.StartSelectingDestination();
        }

        public void ShowGrabMePanel() => grabMePanel.SetActive(true);
    }
}
