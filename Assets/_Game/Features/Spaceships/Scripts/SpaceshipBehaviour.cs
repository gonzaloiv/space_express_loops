using System;
using DigitalLove.FlowControl;
using DigitalLove.Game.Planets;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipBehaviour : MonoBehaviour
    {
        public const int MaxLetters = 5;

        [SerializeField] private MonoState[] states;
        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private DestinationSelectionState destinationSelectionState;
        [SerializeField] private OnRouteState onRouteState;

        private StateMachine stateMachine;

        private string id;

        public string Id => id;
        public bool IsActive => gameObject.activeInHierarchy;
        public bool HasRoute => stateMachine.IsCurrentState<OnRouteState>();

        private void Awake()
        {
            stateMachine = StateMachineFactory.Create(states);
            stateMachine.SetCurrentState<WaitingForRouteState>();
        }

        public void SetOnLoopCreated(Action<LoopEventArgs> onLoopCreated)
        {
            destinationSelectionState.SetOnLoopCreated(id, onLoopCreated);
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> onLoopComplete)
        {
            onRouteState.SetOnLoopComplete(onLoopComplete);
        }

        public void SetOnLoopEditionButtonClicked(Action<LoopEventArgs> onLoopEditionButtonClicked)
        {
            onRouteState.SetOnLoopEditionButtonClicked(id, onLoopEditionButtonClicked);
        }

        public void Spawn(string id, PlanetBaseBehaviour basePlanet)
        {
            this.id = id;
            StationBehaviour station = basePlanet.GetValidStation();
            transform.SetWorldPose(station.WorldPose);
            station.SetIsTaken(true);
            destinationSelector.SetBase(basePlanet);
            this.SetActive(true);
        }

        public void Hide() => this.SetActive(false);

        public void SetRoute(PlanetBehaviour planet)
        {
            stateMachine.SetCurrentState<DestinationSelectionState>();
            destinationSelector.Debug_SetDestinationPlanet(planet);
            destinationSelectionState.Debug_OnLoopCreated();
        }

        // ! DEBUG

        public void Debug_InvokeOnLoopEditionButtonClicked()
        {
            onRouteState.Debug_InvokeOnLoopEditionButtonClicked();
        }
    }
}