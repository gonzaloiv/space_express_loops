using DigitalLove.FlowControl;
using DigitalLove.Game.Planets;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class DestinationSelectionState : MonoState
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private BezierRay bezierRay;
        [SerializeField] private PlanetBehaviour origin;
        [SerializeField] private GhostBehaviour ghost;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            ghost.SetActive(false);
            bezierRay.SetOriginPlanet(origin);
        }

        public override void Enter()
        {
            bezierRay.planetFound += OnPlanetFound;
            grabbable.WhenPointerEventRaised += OnPointerEvent;

            ghost.SetActive(true);
        }

        public override void Exit()
        {
            bezierRay.planetFound -= OnPlanetFound;
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }

        private void OnPlanetFound(PlanetBehaviour behaviour)
        {
            parent.SetCurrentState<LoopDesignState>();
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (pointer.Type == PointerEventType.Unselect)
                OnUnselect();
        }

        [Button]
        private void OnUnselect()
        {
            ghost.SetActive(false);
            parent.SetCurrentState<WaitingForRouteState>();
        }
    }
}