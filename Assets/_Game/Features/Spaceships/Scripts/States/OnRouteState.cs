using DigitalLove.FlowControl;
using UnityEngine;
using DigitalLove.Global;
using System;
using System.Collections;
using Oculus.Interaction;
using DigitalLove.Game.UI;

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

        private int pickedLetters;
        private SpaceshipData data;
        private Coroutine loopCoroutine;

        private Action<LoopCompleteEventArgs> loopComplete;
        private Action<LoopEventArgs> onLoopEditionButtonClicked;

        private LoopEventArgs GetLoopEventArgs => new()
        {
            spaceshipId = data.id,
            originId = destinationSelector.BasePlanet.Id,
            destinationId = destinationSelector.Destination.Id
        };

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            traveller.Hide();
            routePanel.Hide();
            splineContainerWrapper.SetLineRendererActive(false);
            dragZone.gameObject.SetActive(true);
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> loopComplete) => this.loopComplete = loopComplete;

        public void SetOnLoopEditionButtonClicked(Action<LoopEventArgs> onLoopEditionButtonClicked)
        {
            this.onLoopEditionButtonClicked = onLoopEditionButtonClicked;
        }

        public void SetSpaceshipData(SpaceshipData data)
        {
            this.data = data;
            splineContainerWrapper.SetColor(data.color);
            routePanel.Init(data.id, data.color, 50);
        }

        public override void Enter()
        {
            routePanel.editButtonClicked += OnEditButtonClick;

            destinationSelector.StartLookingForDestination(false);
            splineContainerWrapper.CreateLoop(destinationSelector.BasePlanet, destinationSelector.Destination);
            splineContainerWrapper.SetLineRendererActive(true);

            Vector3 goPositions = splineContainerWrapper.GoPositions[splineContainerWrapper.GoPositions.Length / 3];
            routePanel.Show(goPositions);

            dragZone.gameObject.SetActive(false);
            grabbable.SetActive(false);

            StartPathToDestination();
        }

        private void OnEditButtonClick()
        {
            onLoopEditionButtonClicked(GetLoopEventArgs);
            parent.SetCurrentState<WaitingForRouteState>();
        }

        private void StartPathToDestination()
        {
            pickedLetters = 0;
            traveller.ShowEmpty();
            FollowPath(splineContainerWrapper.GoPositions, OnArrivedToDestination);
        }

        private void FollowPath(Vector3[] positions, Action onComplete)
        {
            traveller.FollowPath(positions, OnPathEnded);
            void OnPathEnded(bool hasCompleted)
            {
                if (!hasCompleted)
                {
                    loopComplete(new LoopCompleteEventArgs(data.id, pickedLetters));
                    StartPathToDestination();
                }
                else
                {
                    onComplete();
                }
            }
        }

        private void OnArrivedToDestination()
        {
            IEnumerator Stop()
            {
                yield return new WaitForSeconds(1);
                pickedLetters = destinationSelector.Destination.PlanetStore.PickLetters(SpaceshipBehaviour.MaxLetters);

                traveller.ShowLoaded(pickedLetters);

                FollowPath(splineContainerWrapper.ReturnPositions, OnGotBackToBase);
            }
            loopCoroutine = StartCoroutine(Stop());
        }

        private void OnGotBackToBase()
        {
            IEnumerator Stop()
            {
                yield return new WaitForSeconds(1);
                loopComplete(new LoopCompleteEventArgs(GetLoopEventArgs, pickedLetters));
                StartPathToDestination();
            }
            loopCoroutine = StartCoroutine(Stop());
        }

        public override void Exit()
        {
            routePanel.editButtonClicked -= OnEditButtonClick;

            if (loopCoroutine != null)
                StopCoroutine(loopCoroutine);
            traveller.Hide();
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