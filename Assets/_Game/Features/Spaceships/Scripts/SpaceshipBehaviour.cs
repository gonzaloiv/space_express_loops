using System;
using System.Collections.Generic;
using DigitalLove.FlowControl;
using DigitalLove.Game.Planets;
using DigitalLove.Global;
using UnityEngine;
using DigitalLove.Game.UI;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipBehaviour : MonoBehaviour
    {
        [SerializeField] private WaitingForRouteState waitingForRouteState;
        [SerializeField] private DestinationSelectionState destinationSelectionState;
        [SerializeField] private OnRouteState onRouteState;

        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private RoutePanel routePanel;
        [SerializeField] private Renderer originZone;

        private StateMachine stateMachine;
        private SpaceshipData data;

        public string Id => data.id;
        public string HubId => data.hubId;
        public string ColorCode => data.colorCode;
        public bool IsActive => gameObject.activeInHierarchy;
        public bool HasRoute => stateMachine.IsCurrentState<OnRouteState>();
        public RoutePanel RoutePanel => routePanel;

        private void Awake()
        {
            stateMachine = StateMachineFactory.Create(new MonoState[] { waitingForRouteState, destinationSelectionState, onRouteState });
            stateMachine.SetCurrentState<WaitingForRouteState>();
        }

        public void SetOnLoopCreated(Action<LoopEventArgs> onLoopCreated)
        {
            destinationSelectionState.SetOnLoopCreated(Id, data.hubId, data.colorCode, onLoopCreated);
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> onLoopComplete)
        {
            onRouteState.SetOnLoopComplete(onLoopComplete);
        }

        public void SetOnLoopEditionButtonClicked(Action<LoopEventArgs> onLoopEditionButtonClicked)
        {
            onRouteState.SetOnLoopEditionButtonClicked(onLoopEditionButtonClicked);
        }

        public void Spawn(SpaceshipData data, Color color, HubBehaviour basePlanet)
        {
            this.data = data;

            transform.SetWorldPose(basePlanet.SpawnPose);

            destinationSelector.Init(basePlanet, color);
            onRouteState.SetSpaceshipData(data, color);
            originZone.material.color = color;

            this.SetActive(true);
        }

        public void Hide() => this.SetActive(false);

        public void SetRoute(IReadOnlyList<PlanetBehaviour> destinations)
        {
            if (destinations == null || destinations.Count == 0)
                return;

            stateMachine.SetCurrentState<DestinationSelectionState>();
            destinationSelector.Debug_SetDestinationPlanet(destinations[destinations.Count - 1]);
            destinationSelectionState.Debug_OnLoopCreated();
        }

        public void ShowGrabMePanel()
        {
            waitingForRouteState.ShowGrabMePanel();
        }

        // ! DEBUG

        public void Debug_InvokeOnLoopEditionButtonClicked()
        {
            onRouteState.Debug_InvokeOnLoopEditionButtonClicked();
        }
    }
}