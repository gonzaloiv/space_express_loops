using System;
using DigitalLove.FlowControl;
using DigitalLove.Game.Planets;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipBehaviour : MonoBehaviour
    {
        [SerializeField] private MonoState[] states;
        [SerializeField] private BezierRay bezierRay;
        [SerializeField] private DestinationSelectionState destinationSelectionState;
        [SerializeField] private OnRouteState onRouteState;

        private StateMachine stateMachine;

        private string id;
        public string Id => id;

        private void Awake()
        {
            stateMachine = StateMachineFactory.Create(states);
            stateMachine.SetCurrentState<WaitingForRouteState>();
        }

        public void SetOnLoopCreated(Action<LoopData> onLoopCreated)
        {
            destinationSelectionState.SetOnLoopCreated(id, onLoopCreated);
        }

        public void SetOnLoopComplete(Action<int> onLoopComplete)
        {
            onRouteState.SetOnLoopComplete(onLoopComplete);
        }

        public void Spawn(string id, BasePlanetBehaviour basePlanet)
        {
            this.id = id;
            transform.SetWorldPose(basePlanet.GetValidStationPose());
            destinationSelectionState.SetBasePlanet(basePlanet);
            this.SetActive(true);
        }

        // ! DEBUG

        public void SetRoute(PlanetBehaviour planet)
        {
            bezierRay.SetDestinationPlanet(planet);
            destinationSelectionState.OnLoopCreated();
        }

        public void Hide() => this.SetActive(false);
    }
}