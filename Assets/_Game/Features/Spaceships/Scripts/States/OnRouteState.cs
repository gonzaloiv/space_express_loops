using DigitalLove.FlowControl;
using DG.Tweening;
using UnityEngine;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine.UI;
using System;
using System.Collections;
using DigitalLove.Game.Planets;

namespace DigitalLove.Game.Spaceships
{
    public class OnRouteState : MonoState
    {
        [SerializeField] private GameObject body;
        [SerializeField] private BezierRay bezierRay;
        [SerializeField] private FloatValue speed;
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private GameObject editPanel;
        [SerializeField] private Button editButton;
        [SerializeField] private LettersPanel lettersPanel;
        [SerializeField] private Renderer rend;
        [SerializeField] private ColorValue loadedColor;
        [SerializeField] private ColorValue defaultColor;

        private DG.Tweening.Tween followTween;
        private int pickedLetters;
        private string id;

        private Action<LoopCompleteEventArgs> loopComplete;
        private Action<LoopEventArgs> onLoopEditionButtonClicked;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            body.gameObject.SetActive(false);
            editPanel.SetActive(false);
            followTween?.Kill();
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

            ShowEditPanel(bezierRay.Spline.OriginPosition);
            bezierRay.SetIsLookingForDestination(false);
            bezierRay.Destination.SetIsInRoute(true);
            grabbable.SetActive(false);
            OnGotBackToBase();
        }

        private void OnEditButtonClick()
        {
            onLoopEditionButtonClicked(new LoopEventArgs()
            {
                spaceshipId = id,
                originId = bezierRay.OriginId,
                destinationId = bezierRay.Destination.Id
            });
            parent.SetCurrentState<WaitingForRouteState>();
        }

        private void ShowEditPanel(Vector3 position)
        {
            editPanel.SetActive(true);
            editPanel.transform.position = position;
        }

        private void FollowPath(Vector3[] positions, Action onComplete)
        {
            if (positions == null || positions.Length < 2)
                return;
            float totalDistance = positions.GetTotalDistance();
            float duration = totalDistance / speed.value;
            body.transform.position = positions[0];
            followTween = body.transform.DOPath(positions, duration, PathType.Linear)
                .SetLookAt(0.02f)
                .SetEase(Ease.Linear)
                .OnComplete(onComplete.Invoke);
        }

        private void OnArrivedToDestination()
        {
            IEnumerator Stop()
            {
                yield return new WaitForSeconds(1);
                pickedLetters = bezierRay.Destination.PlanetStore.PickLetters(SpaceshipBehaviour.MaxLetters);

                lettersPanel.ShowLetters(pickedLetters, 0);
                rend.material.color = loadedColor.value;

                FollowPath(bezierRay.Spline.ReturnPositions, OnGotBackToBase);
            }
            StartCoroutine(Stop());
        }

        private void OnGotBackToBase()
        {
            IEnumerator Stop()
            {
                yield return new WaitForSeconds(1);
                InvokeLoopComplete();
                pickedLetters = 0;

                body.gameObject.SetActive(true);
                lettersPanel.SetActive(false);
                rend.material.color = defaultColor.value;

                FollowPath(bezierRay.Spline.GoPositions, OnArrivedToDestination);
            }
            StartCoroutine(Stop());
        }

        private void InvokeLoopComplete()
        {
            if (loopComplete == null || pickedLetters == 0)
                return;
            loopComplete(new LoopCompleteEventArgs()
            {
                spaceshipId = id,
                originId = bezierRay.OriginId,
                destinationId = bezierRay.Destination.Id,
                value = pickedLetters
            });
        }

        public override void Exit()
        {
            editButton.onClick.RemoveListener(OnEditButtonClick);

            followTween?.Kill();
            HideBody();
        }

        private void HideBody()
        {
            body.gameObject.SetActive(false);
            bezierRay.Destination.SetIsInRoute(false);
            editPanel.SetActive(false);
        }

        // ! DEBUG

        public void Debug_InvokeOnLoopEditionButtonClicked()
        {
            OnEditButtonClick();
        }
    }
}