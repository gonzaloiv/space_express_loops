using System.Collections.Generic;
using DigitalLove.FlowControl;
using DigitalLove.Game.Planets;
using DigitalLove.Game.UI;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipBehaviour : MonoBehaviour
    {
        [SerializeField] private WaitingForRouteState waitingForRouteState;
        [SerializeField] private OnRouteState onRouteState;

        [SerializeField] private GrabbableWrapper grabbableWrapper;
        [SerializeField] private RoutePanel routePanel;
        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private Renderer originZone;

        private StateMachine stateMachine;
        private SpaceshipData data;
        private bool isInitialized;
        private LoopHandlers handlers;

        public bool IsInitialized => isInitialized;
        public DestinationSelector DestinationSelector => destinationSelector;

        public string Id => data.id;
        public string HubId => data.hubId;
        public string ColorCode => data.colorCode;
        public HubBehaviour Hub => destinationSelector.Hub;
        public bool IsActive => gameObject.activeInHierarchy;
        public bool HasRoute => isInitialized && onRouteState.HasDestinations && stateMachine.IsCurrentState<OnRouteState>();

        public void Initialize()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            waitingForRouteState.Bind(StartSelectingDestination);
            onRouteState.Bind(this);

            stateMachine = StateMachineFactory.Create(new MonoState[] { waitingForRouteState, onRouteState });
            stateMachine.SetCurrentState<WaitingForRouteState>();
        }

        public void Configure(LoopHandlers loopHandlers)
        {
            handlers = loopHandlers;
            onRouteState.SetOnLoopComplete(loopHandlers.Complete);
        }

        public void StartSelectingDestination()
        {
            onRouteState.SetEnterSelectingOnEnter(true);
            stateMachine.SetCurrentState<OnRouteState>();
        }

        public void NotifyLoopChanged() => handlers.Changed?.Invoke(BuildLoopEventArgs());

        public void NotifyLoopEditionClicked()
        {
            MoveToHub();
            handlers.EditionClicked?.Invoke(BuildLoopEventArgs());
        }

        public void MoveToHub()
        {
            grabbableWrapper.SetWorldPosition(Hub.SpawnPose.position);
        }

        public LoopEventArgs BuildLoopEventArgs() => new()
        {
            spaceshipId = Id,
            destinationIds = onRouteState.GetDestinationIds(),
            colorCode = ColorCode,
            hubId = HubId
        };

        public void Spawn(SpaceshipData data, Color color, HubBehaviour hub)
        {
            this.data = data;

            grabbableWrapper.SetWorldPosition(hub.SpawnPose.position);
            hub.SetRouteColor(color);
            destinationSelector.Init(hub, color);
            onRouteState.SetRouteColor(color);
            originZone.material.color = color;
            onRouteState.ClearDestinations();

            MoveToHub();

            this.SetActive(true);
            routePanel.Show(Id, color, hub.transform.position);
        }

        public void Hide()
        {
            if (!isInitialized)
            {
                this.SetActive(false);
                return;
            }

            onRouteState.ClearDestinations();
            this.SetActive(false);
        }

        public void SetRoute(IReadOnlyList<PlanetBehaviour> destinations)
        {
            if (!isInitialized)
                Initialize();

            onRouteState.SetEnterSelectingOnEnter(false);
            onRouteState.SetDestinations(destinations);
            stateMachine.SetCurrentState<OnRouteState>();
        }

        public void ShowGrabMePanel() => waitingForRouteState.ShowGrabMePanel();

        #region Debug

        public void Debug_SimulateGrabSelect()
        {
            if (!isInitialized)
                Initialize();

            if (stateMachine.IsCurrentState<WaitingForRouteState>())
                waitingForRouteState.Debug_SimulateGrabSelect();
            else if (stateMachine.IsCurrentState<OnRouteState>())
                onRouteState.Debug_SimulateGrabSelect();
            else
                Debug.LogWarning($"EditorDebug: Grab select ignored in state {stateMachine.CurrentRoute}.");
        }

        public void Debug_SimulateGrabRelease()
        {
            if (!isInitialized)
                return;

            if (stateMachine.IsCurrentState<OnRouteState>())
                onRouteState.Debug_SimulateGrabRelease();
            else
                Debug.LogWarning($"EditorDebug: Grab release ignored in state {stateMachine.CurrentRoute}.");
        }

        public void Debug_ConfirmDestination() => onRouteState.Debug_ConfirmDestination();

        public void Debug_InvokeOnLoopEditionButtonClicked() => onRouteState.Debug_InvokeOnLoopEditionButtonClicked();

        public SpaceshipRoute Debug_Route => onRouteState.Route;

        #endregion
    }
}
