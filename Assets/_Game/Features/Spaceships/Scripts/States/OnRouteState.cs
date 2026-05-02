using DigitalLove.FlowControl;
using UnityEngine;
using DigitalLove.Global;
using UnityEngine.UI;
using System;
using System.Collections;
using DigitalLove.Game.Planets;
using Oculus.Interaction;

namespace DigitalLove.Game.Spaceships
{
    public class OnRouteState : MonoState
    {
        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private TravellerBehaviour traveller;
        [SerializeField] private SplineContainerWrapper splineContainerWrapper;
        [SerializeField] private Transform dragZone;

        [Header("UI")]
        [SerializeField] private LettersPanel lettersPanel;
        [SerializeField] private GameObject editPanel;
        [SerializeField] private Button editButton;

        private int pickedLetters;
        private string id;
        private Coroutine loopCoroutine;

        private Action<LoopCompleteEventArgs> loopComplete;
        private Action<LoopEventArgs> onLoopEditionButtonClicked;

        private LoopEventArgs GetLoopEventArgs() => new()
        {
            spaceshipId = id,
            originId = destinationSelector.BasePlanet.Id,
            destinationId = destinationSelector.Destination.Id
        };

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            traveller.Hide();
            editPanel.SetActive(false);
            splineContainerWrapper.SetLineRendererActive(false);
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> loopComplete) => this.loopComplete = loopComplete;

        public void SetOnLoopEditionButtonClicked(string id, Action<LoopEventArgs> onLoopEditionButtonClicked)
        {
            this.id = id;
            this.onLoopEditionButtonClicked = onLoopEditionButtonClicked;
        }

        public override void Enter()
        {
            editButton.onClick.AddListener(OnEditButtonClick);

            editPanel.SetActive(true);
            destinationSelector.StartLookingForDestination(false);
            splineContainerWrapper.CreateLoop(destinationSelector.BasePlanet, destinationSelector.Destination);
            splineContainerWrapper.SetLineRendererActive(true);

            dragZone.gameObject.SetActive(false);
            grabbable.SetActive(false);

            StartPathToDestination();
        }

        private void OnEditButtonClick()
        {
            onLoopEditionButtonClicked(GetLoopEventArgs());
            parent.SetCurrentState<WaitingForRouteState>();
        }

        private void StartPathToDestination()
        {
            pickedLetters = 0;
            traveller.ShowEmpty();
            lettersPanel.SetActive(false);

            FollowPath(splineContainerWrapper.GoPositions, OnArrivedToDestination);
        }

        private void FollowPath(Vector3[] positions, Action onComplete)
        {
            traveller.FollowPath(positions, OnPathEnded);
            void OnPathEnded(bool hasCompleted)
            {
                if (!hasCompleted)
                {
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

                traveller.ShowLoaded();
                lettersPanel.ShowLetters(pickedLetters, 0);

                FollowPath(splineContainerWrapper.ReturnPositions, OnGotBackToBase);
            }
            loopCoroutine = StartCoroutine(Stop());
        }

        private void OnGotBackToBase()
        {
            IEnumerator Stop()
            {
                yield return new WaitForSeconds(1);
                loopComplete(new LoopCompleteEventArgs(GetLoopEventArgs(), pickedLetters));
                StartPathToDestination();
            }
            loopCoroutine = StartCoroutine(Stop());
        }

        public override void Exit()
        {
            editButton.onClick.RemoveListener(OnEditButtonClick);

            if (loopCoroutine != null)
                StopCoroutine(loopCoroutine);
            traveller.Hide();
            editPanel.SetActive(false);
            splineContainerWrapper.SetLineRendererActive(false);
        }

        // ! DEBUG

        public void Debug_InvokeOnLoopEditionButtonClicked()
        {
            OnEditButtonClick();
        }
    }
}