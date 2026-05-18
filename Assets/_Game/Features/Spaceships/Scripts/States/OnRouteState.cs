using DigitalLove.FlowControl;
using UnityEngine;
using System;
using Oculus.Interaction;
using DigitalLove.Game.UI;
using DigitalLove.Global;

namespace DigitalLove.Game.Spaceships
{
    public class OnRouteState : MonoState
    {
        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private TravellerBehaviour traveller;
        [SerializeField] private SplineContainerWrapper splineContainerWrapper;
        [SerializeField] private Transform dragZone;
        [SerializeField] private RoutePanel routePanel;
        [SerializeField] private float legDelay = 1f;

        private SpaceshipData data;
        private TravellerLoopRunner loopRunner;

        private Action<LoopCompleteEventArgs> loopCompleted;
        private Action<LoopEventArgs> onLoopEditionButtonClicked;

        private LoopEventArgs CurrentLoopEventArgs => new()
        {
            spaceshipId = data.id,
            destinationIds = new() { destinationSelector.Destination.Id },
            colorCode = data.colorCode,
            hubId = data.hubId
        };

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            loopRunner ??= new TravellerLoopRunner(this, traveller, legDelay);
            loopRunner.Stop();
            routePanel.Hide();
            splineContainerWrapper.SetLineRendererActive(false);
            dragZone.gameObject.SetActive(true);
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> loopCompleted)
        {
            this.loopCompleted = loopCompleted;
            loopRunner?.SetOnLoopIterationComplete(loopCompleted);
        }

        public void SetOnLoopEditionButtonClicked(Action<LoopEventArgs> onLoopEditionButtonClicked)
        {
            this.onLoopEditionButtonClicked = onLoopEditionButtonClicked;
        }

        public void SetSpaceshipData(SpaceshipData data, Color color)
        {
            this.data = data;
            splineContainerWrapper.SetColor(color);
            routePanel.SetData(data.id, color);
        }

        public override void Enter()
        {
            routePanel.editButtonClicked += OnEditButtonClick;

            destinationSelector.StartLookingForDestination(false);
            splineContainerWrapper.CreateLoop(destinationSelector.BasePlanet, destinationSelector.Destination);
            splineContainerWrapper.SetLineRendererActive(true);

            Vector3 goPositions = splineContainerWrapper.GoPositions[splineContainerWrapper.GoPositions.Length / 3];
            routePanel.SetPosition(goPositions);
            routePanel.Show();

            dragZone.gameObject.SetActive(false);
            grabbable.SetActive(false);

            loopRunner ??= new TravellerLoopRunner(this, traveller, legDelay);
            loopRunner.SetOnLoopIterationComplete(loopCompleted);
            loopRunner.StartLoop(
                splineContainerWrapper,
                data.id,
                () => destinationSelector.Destination.PlanetStore.PickAllLetters(),
                () => CurrentLoopEventArgs);
        }

        private void OnEditButtonClick()
        {
            onLoopEditionButtonClicked(CurrentLoopEventArgs);
            parent.SetCurrentState<WaitingForRouteState>();
        }

        public override void Exit()
        {
            routePanel.editButtonClicked -= OnEditButtonClick;

            loopRunner?.Stop();
            routePanel.Hide();
            splineContainerWrapper.SetLineRendererActive(false);
            dragZone.gameObject.SetActive(true);
        }

        // ! DEBUG

        public void Debug_InvokeOnLoopEditionButtonClicked()
        {
            OnEditButtonClick();
        }
    }
}
