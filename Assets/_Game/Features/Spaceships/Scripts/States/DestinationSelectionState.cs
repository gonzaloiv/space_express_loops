using System;
using DigitalLove.FlowControl;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class DestinationSelectionState : MonoState
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private GhostBehaviour ghost;

        private string id;
        private string hubId;
        private string colorCode;

        private Action<LoopEventArgs> onLoopCreated;

        public void SetOnLoopCreated(string id, string hubId, string colorCode, Action<LoopEventArgs> onLoopCreated)
        {
            this.id = id;
            this.hubId = hubId;
            this.colorCode = colorCode;
            this.onLoopCreated = onLoopCreated;
        }

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            ghost.SetActive(false);
            destinationSelector.StartLookingForDestination(false);
        }

        public override void Enter()
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;

            ghost.SetActive(true);
            destinationSelector.StartLookingForDestination(true);
        }

        public override void Exit()
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;

            ghost.SetActive(false);
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (pointer.Type == PointerEventType.Unselect)
                OnUnselect();
        }

        [Button]
        private void OnUnselect()
        {
            if (destinationSelector.HasDestinationBeenSelected)
            {
                OnLoopCreated();
            }
            else
            {
                parent.SetCurrentState<WaitingForRouteState>();
            }
        }

        private void OnLoopCreated()
        {
            LoopEventArgs args = new()
            {
                spaceshipId = id,
                destinationIds = new() { destinationSelector.Destination.Id },
                colorCode = colorCode,
                hubId = hubId
            };
            onLoopCreated(args);
            parent.SetCurrentState<OnRouteState>();
        }

        // ! DEBUG
        public void Debug_OnLoopCreated()
        {
            OnLoopCreated();
        }
    }
}