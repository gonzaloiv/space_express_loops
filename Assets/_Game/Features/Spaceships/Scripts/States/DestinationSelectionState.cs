using System;
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
        [SerializeField] private GhostBehaviour ghost;

        private PlanetBehaviour destination;
        private string id;

        private Action<LoopData> onLoopCreated;

        public void SetOnLoopCreated(string id, Action<LoopData> onLoopCreated)
        {
            this.id = id;
            this.onLoopCreated = onLoopCreated;
        }

        public void SetBasePlanet(BasePlanetBehaviour basePlanet)
        {
            bezierRay.Init(basePlanet, ghost.Body);
        }

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            ghost.SetActive(false);

            bezierRay.SetActive(false);
        }

        public override void Enter()
        {
            bezierRay.planetSelected += OnPlanetFound;
            grabbable.WhenPointerEventRaised += OnPointerEvent;

            ghost.SetActive(true);
            bezierRay.SetActive(true);
        }

        public override void Exit()
        {
            bezierRay.planetSelected -= OnPlanetFound;
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }

        private void OnPlanetFound(PlanetBehaviour planet)
        {
            destination = planet;
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
            if (destination != null)
            {
                OnLoopCreated();
            }
            else
            {
                parent.SetCurrentState<WaitingForRouteState>();
            }
        }

        public void OnLoopCreated()
        {
            LoopData loopData = new()
            {
                spaceshipId = id,
                destinationId = bezierRay.Destination.Id
            };
            parent.SetCurrentState<OnRouteState>();
            if (onLoopCreated != null)
                onLoopCreated.Invoke(loopData);
        }
    }
}