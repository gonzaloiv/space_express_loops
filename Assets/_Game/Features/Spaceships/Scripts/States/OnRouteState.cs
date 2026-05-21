using System;
using System.Collections.Generic;
using DigitalLove.FlowControl;
using DigitalLove.Game.Planets;
using DigitalLove.Game.UI;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class OnRouteState : MonoState
    {
        [SerializeField] private GrabbableWrapper grabbableWrapper;
        [SerializeField] private GhostBehaviour ghost;
        [SerializeField] private RoutePanel routePanel;
        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private RouteContainer splineContainerWrapper;
        [SerializeField] private TravellerBehaviour traveller;
        [SerializeField] private float legDelay = 1f;

        private SpaceshipRoute route;
        private LoopDestinationSelection destinationSelection;
        private TravellerLoopRunner travellerLoop;
        private Func<string> getSpaceshipId;
        private Func<LoopEventArgs> buildLoopEventArgs;
        private Action onLoopChanged;
        private Action onLoopEditionClicked;

        private bool enterSelectingOnEnter;
        private bool isSelectingDestination;

        public bool HasDestinations => route.HasDestinations;
        public SpaceshipRoute Route => route;

        public void Bind(
            Func<string> getSpaceshipId,
            Func<LoopEventArgs> buildLoopEventArgs,
            Action onLoopChanged,
            Action onLoopEditionClicked)
        {
            EnsureRouteSystems();
            this.getSpaceshipId = getSpaceshipId;
            this.buildLoopEventArgs = buildLoopEventArgs;
            this.onLoopChanged = onLoopChanged;
            this.onLoopEditionClicked = onLoopEditionClicked;
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> onLoopComplete)
        {
            EnsureRouteSystems();
            travellerLoop.SetOnLoopIterationComplete(onLoopComplete);
        }

        public void SetDestinations(IReadOnlyList<PlanetBehaviour> destinations)
        {
            EnsureRouteSystems();
            route.SetDestinations(destinations);
        }

        public void ClearDestinations()
        {
            if (route == null)
                return;

            route.ClearDestinations();
        }

        public void SetRouteColor(Color color)
        {
            EnsureRouteSystems();
            route.SetColor(color);
        }

        public List<string> GetDestinationIds()
        {
            EnsureRouteSystems();
            return route.GetDestinationIds();
        }

        public void SetEnterSelectingOnEnter(bool value) => enterSelectingOnEnter = value;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            EnsureRouteSystems();
            travellerLoop.Stop();
            route.SetLineRendererActive(false);
            ResetRouteChrome();
        }

        public override void Enter()
        {
            routePanel.editButtonClicked += OnEditButtonClick;
            grabbableWrapper.SubscribePointerEvents(OnPointerEvent);

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
            routePanel.editButtonClicked -= OnEditButtonClick;
            grabbableWrapper.UnsubscribePointerEvents(OnPointerEvent);

            isSelectingDestination = false;
            travellerLoop.Stop();
            ResetRouteChrome();
        }

        private void EnsureRouteSystems()
        {
            if (route != null)
                return;

            route = new SpaceshipRoute(splineContainerWrapper, () => destinationSelector.Hub);
            destinationSelection = new LoopDestinationSelection(grabbableWrapper, destinationSelector, route);
            travellerLoop = new TravellerLoopRunner(this, splineContainerWrapper, traveller, legDelay);
            traveller.Hide();
        }

        private void BeginLoop()
        {
            isSelectingDestination = false;
            destinationSelection.End();
            route.RebuildRoute();
            ShowLoopChrome();
            travellerLoop.StartLoop(getSpaceshipId(), buildLoopEventArgs);
        }

        private void BeginSelecting()
        {
            isSelectingDestination = true;
            destinationSelection.Begin();
            ghost.SetActive(true);
            grabbableWrapper.SetInteractionActive(true);
        }

        private void EndSelecting()
        {
            isSelectingDestination = false;
            destinationSelection.End();
            ghost.SetActive(false);
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
            if (destinationSelector.HasDestinationBeenSelected)
            {
                OnDestinationConfirmed();
                return;
            }

            EndSelecting();

            if (route.HasDestinations)
                ShowStationGrab();
            else
                parent.SetCurrentState<WaitingForRouteState>();
        }

        private void OnDestinationConfirmed()
        {
            SelectionConfirmResult result = destinationSelection.Confirm(
                destinationSelector.Destination,
                onLoopChanged);

            switch (result)
            {
                case SelectionConfirmResult.StartedLoop:
                    BeginLoop();
                    break;
                case SelectionConfirmResult.ExtendedLoop:
                    EndSelecting();
                    ShowStationGrab();
                    break;
            }
        }

        private void OnEditButtonClick()
        {
            onLoopEditionClicked();
            parent.SetCurrentState<WaitingForRouteState>();
        }

        private void ResetRouteChrome()
        {
            ghost.SetActive(false);
            grabbableWrapper.Hide();
            routePanel.Hide();
            route.SetLineRendererActive(false);
            destinationSelector.StartLookingForDestination(false);
        }

        private void ShowLoopChrome()
        {
            ghost.SetActive(false);
            destinationSelector.StartLookingForDestination(false);

            routePanel.SetPosition(route.Hub.transform.position);
            routePanel.Show();

            grabbableWrapper.Show();
            MoveShipToActiveStation();
        }

        private void ShowStationGrab()
        {
            grabbableWrapper.Show();
            MoveShipToActiveStation();
        }

        private void MoveShipToActiveStation()
        {
            grabbableWrapper.ActivateGameObject();
            grabbableWrapper.SetWorldPosition(route.HasDestinations && route.Destinations.Count > 1
                ? route.LastLegEndPosition
                : route.FirstLegEndPosition);
        }

        // ! DEBUG

        public void Debug_ConfirmDestination()
        {
            if (destinationSelector.Destination != null)
                OnDestinationConfirmed();
        }

        public void Debug_InvokeOnLoopEditionButtonClicked() => OnEditButtonClick();
    }
}
