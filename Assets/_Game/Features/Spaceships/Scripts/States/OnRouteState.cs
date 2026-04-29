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
        [SerializeField] private float travelSpeed = 2f;
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private GameObject editPanel;
        [SerializeField] private Button editButton;
        [SerializeField] private LettersPanel lettersPanel;

        private DG.Tweening.Tween followTween;
        private int pickedLetters;

        private string id;
        private Action<int> loopComplete;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            body.SetActive(false);
            editPanel.SetActive(false);
            lettersPanel.SetActive(false);
            lettersPanel.SetMaxLetters(SpaceshipBehaviour.MaxLetters);
            followTween?.Kill();
        }

        public void SetOnLoopComplete(Action<int> loopComplete) => this.loopComplete = loopComplete;

        public override void Enter()
        {
            editButton.onClick.AddListener(OnEditButtonClick);

            ShowBody();
            ShowEditPanel(bezierRay.Spline.OriginPosition);
            OnGotBackToBase();
        }

        private void OnEditButtonClick() => parent.SetCurrentState<WaitingForRouteState>();

        private void ShowBody()
        {
            body.SetActive(true);
            grabbable.SetActive(false);
            bezierRay.Destination.SetIsInRoute(true);
            bezierRay.SetIsLookingForDestination(false);
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
            float duration = totalDistance / travelSpeed;
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
                pickedLetters = bezierRay.Destination.PickLetters(SpaceshipBehaviour.MaxLetters);
                lettersPanel.Show(pickedLetters);
                FollowPath(bezierRay.Spline.ReturnPositions, OnGotBackToBase);
            }
            StartCoroutine(Stop());
        }

        private void OnGotBackToBase()
        {
            IEnumerator Stop()
            {
                yield return new WaitForSeconds(1);
                if (loopComplete != null && pickedLetters != 0)
                    loopComplete(pickedLetters);
                pickedLetters = 0;
                lettersPanel.SetActive(false);
                FollowPath(bezierRay.Spline.GoPositions, OnArrivedToDestination);
            }
            StartCoroutine(Stop());
        }

        public override void Exit()
        {
            editButton.onClick.RemoveListener(OnEditButtonClick);

            followTween?.Kill();
            HideBody();
        }

        private void HideBody()
        {
            body.SetActive(false);
            bezierRay.Destination.SetIsInRoute(false);
            editPanel.SetActive(false);
        }
    }
}