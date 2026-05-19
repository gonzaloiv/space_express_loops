using DigitalLove.FlowControl;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class OnRouteState : MonoState
    {
        private SpaceshipBehaviour spaceship;
        private SpaceshipLoop loop;
        private SpaceshipPresentation presentation;
        private bool enterSelectingOnEnter;
        private bool isSelectingDestination;

        public void Bind(SpaceshipBehaviour spaceship)
        {
            this.spaceship = spaceship;
            loop = spaceship.Loop;
            presentation = spaceship.Presentation;
        }

        public void SetEnterSelectingOnEnter(bool value) => enterSelectingOnEnter = value;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            loop.ResetVisuals();
            presentation.Reset(loop);
        }

        public override void Enter()
        {
            presentation.RoutePanel.editButtonClicked += OnEditButtonClick;
            presentation.Grabbable.WhenPointerEventRaised += OnPointerEvent;

            if (enterSelectingOnEnter)
            {
                enterSelectingOnEnter = false;
                BeginSelecting();
                return;
            }

            BeginLoop();
        }

        public override void Exit()
        {
            presentation.RoutePanel.editButtonClicked -= OnEditButtonClick;
            presentation.Grabbable.WhenPointerEventRaised -= OnPointerEvent;

            isSelectingDestination = false;
            loop.StopTraveller();
            presentation.Reset(loop);
        }

        private void BeginLoop()
        {
            isSelectingDestination = false;
            loop.EndSelection();
            loop.RebuildRoute();
            presentation.ShowLoopChrome(loop);
            loop.StartTraveller(spaceship.Id, spaceship.BuildLoopEventArgs);
        }

        private void BeginSelecting()
        {
            isSelectingDestination = true;
            loop.BeginSelection();
            presentation.ShowSelectingChrome();
            presentation.Grabbable.SetActive(true);
        }

        private void EndSelecting()
        {
            isSelectingDestination = false;
            loop.EndSelection();
            presentation.HideSelectingChrome();
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (isSelectingDestination)
            {
                if (pointer.Type == PointerEventType.Unselect)
                    OnUnselectWhileSelecting();
            }
            else if (pointer.Type == PointerEventType.Select)
            {
                BeginSelecting();
            }
        }

        private void OnUnselectWhileSelecting()
        {
            if (spaceship.DestinationSelector.HasDestinationBeenSelected)
            {
                OnDestinationConfirmed();
                return;
            }

            EndSelecting();

            if (loop.HasDestinations)
                presentation.ShowStationGrab(loop);
            else
                parent.SetCurrentState<WaitingForRouteState>();
        }

        private void OnDestinationConfirmed()
        {
            SelectionConfirmResult result = loop.ConfirmSelection(
                spaceship.DestinationSelector.Destination,
                spaceship.NotifyLoopChanged);

            switch (result)
            {
                case SelectionConfirmResult.StartedLoop:
                    BeginLoop();
                    break;
                case SelectionConfirmResult.ExtendedLoop:
                    EndSelecting();
                    presentation.ShowStationGrab(loop);
                    break;
            }
        }

        private void OnEditButtonClick()
        {
            spaceship.NotifyLoopEditionClicked();
            parent.SetCurrentState<WaitingForRouteState>();
        }

        // ! DEBUG

        public void Debug_ConfirmDestination()
        {
            if (spaceship.DestinationSelector.Destination != null)
                OnDestinationConfirmed();
        }

        public void Debug_InvokeOnLoopEditionButtonClicked() => OnEditButtonClick();
    }
}
