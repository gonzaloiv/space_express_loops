using System;
using System.Collections.Generic;
using DigitalLove.FlowControl;
using DigitalLove.Game.Planets;
using DigitalLove.Global;
using UnityEditor.Presets;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    [RequireComponent(typeof(SpaceshipPresentation))]
    public class SpaceshipBehaviour : MonoBehaviour
    {
        [SerializeField] private WaitingForRouteState waitingForRouteState;
        [SerializeField] private OnRouteState onRouteState;
        [SerializeField] private SpaceshipPresentation presentation;

        [SerializeField] private StationBehaviour station;
        [SerializeField] private RouteContainer splineContainerWrapper;
        [SerializeField] private TravellerBehaviour traveller;
        [SerializeField] private Renderer originZone;
        [SerializeField] private float legDelay = 1f;

        private StateMachine stateMachine;
        private SpaceshipData data;
        private SpaceshipLoop loop;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;
        public SpaceshipLoop Loop => loop;
        public SpaceshipPresentation Presentation => presentation;
        public DestinationSelector DestinationSelector => presentation.DestinationSelector;

        public string Id => data.id;
        public string HubId => data.hubId;
        public string ColorCode => data.colorCode;
        public HubBehaviour Hub => DestinationSelector.Hub;
        public bool IsActive => gameObject.activeInHierarchy;
        public bool HasRoute =>
            isInitialized && loop.HasDestinations && stateMachine.IsCurrentState<OnRouteState>();

        public void Initialize()
        {
            if (isInitialized)
                return;

            isInitialized = true;
            presentation ??= GetComponent<SpaceshipPresentation>();

            loop = new SpaceshipLoop(
                this,
                transform,
                presentation.DestinationSelector,
                station,
                splineContainerWrapper,
                traveller,
                legDelay);
            presentation.Reset(loop);

            waitingForRouteState.Bind(this);
            onRouteState.Bind(this);

            traveller.Hide();

            stateMachine = StateMachineFactory.Create(new MonoState[] { waitingForRouteState, onRouteState });
            stateMachine.SetCurrentState<WaitingForRouteState>();
        }

        public void StartSelectingDestination()
        {
            onRouteState.SetEnterSelectingOnEnter(true);
            stateMachine.SetCurrentState<OnRouteState>();
        }

        public void SetOnLoopChanged(Action<LoopEventArgs> onLoopChanged) => onLoopChangedHandler = onLoopChanged;

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> onLoopComplete) =>
            loop.SetOnLoopComplete(onLoopComplete);

        public void SetOnLoopEditionButtonClicked(Action<LoopEventArgs> onLoopEditionButtonClicked) =>
            onLoopEditionButtonClickedHandler = onLoopEditionButtonClicked;

        private Action<LoopEventArgs> onLoopChangedHandler = _ => { };
        private Action<LoopEventArgs> onLoopEditionButtonClickedHandler = _ => { };

        public void NotifyLoopChanged() => onLoopChangedHandler(BuildLoopEventArgs());

        public void NotifyLoopEditionClicked()
        {
            loop.ClearDestinations();
            loop.MoveShipToHub();
            onLoopEditionButtonClickedHandler(BuildLoopEventArgs());
        }

        public LoopEventArgs BuildLoopEventArgs() => new()
        {
            spaceshipId = Id,
            destinationIds = loop.GetDestinationIds(),
            colorCode = ColorCode,
            hubId = HubId
        };

        public void Spawn(SpaceshipData data, Color color, HubBehaviour basePlanet)
        {
            this.data = data;

            transform.SetWorldPose(basePlanet.SpawnPose);

            basePlanet.SetRouteColor(color);
            DestinationSelector.Init(basePlanet, color);
            loop.SetVisuals(data.id, color);
            loop.SetRoutePanelData(presentation.RoutePanel, data.id, color);
            originZone.material.color = color;
            loop.ClearDestinations();
            loop.MoveShipToHub();

            this.SetActive(true);
        }

        public void Hide()
        {
            if (!isInitialized)
            {
                this.SetActive(false);
                return;
            }

            loop.ClearDestinations();
            this.SetActive(false);
        }

        public void SetRoute(IReadOnlyList<PlanetBehaviour> destinations)
        {
            if (destinations == null || destinations.Count == 0)
                return;

            if (!isInitialized)
                Initialize();

            if (Hub == null)
            {
                Debug.LogWarning($"{nameof(SpaceshipBehaviour)}: Cannot set route without a hub. Spawn the ship first.");
                return;
            }

            onRouteState.SetEnterSelectingOnEnter(false);
            loop.SetDestinations(destinations);
            stateMachine.SetCurrentState<OnRouteState>();
        }

        public void ShowGrabMePanel() => waitingForRouteState.ShowGrabMePanel();

        // ! DEBUG

        public void Debug_ConfirmDestination() => onRouteState.Debug_ConfirmDestination();

        public void Debug_InvokeOnLoopEditionButtonClicked() => onRouteState.Debug_InvokeOnLoopEditionButtonClicked();
    }
}
