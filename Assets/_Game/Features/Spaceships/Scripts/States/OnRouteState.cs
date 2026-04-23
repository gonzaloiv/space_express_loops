using DigitalLove.FlowControl;
using DG.Tweening;
using UnityEngine;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine.UI;

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

        private Vector3[] positions;
        private DG.Tweening.Tween followTween;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            body.SetActive(false);
            editPanel.SetActive(false);
        }

        public override void Enter()
        {
            editButton.onClick.AddListener(OnEditButtonClick);

            body.SetActive(true);
            bezierRay.Origin.SetIsInRoute(true);
            bezierRay.Destination.SetIsInRoute(true);
            bezierRay.SetIsLookingForDestination(false);
            positions = bezierRay.GetSplinePositions();
            FollowPath();
            grabbable.SetActive(false);
            editPanel.SetActive(true);
        }

        private void OnEditButtonClick()
        {
            parent.SetCurrentState<WaitingForRouteState>();
        }

        public override void Exit()
        {
            editButton.onClick.RemoveListener(OnEditButtonClick);

            followTween?.Kill();
            body.SetActive(false);
            bezierRay.Origin.SetIsInRoute(false);
            bezierRay.Destination.SetIsInRoute(false);
            editPanel.SetActive(false);
        }

        private void FollowPath()
        {
            if (positions == null || positions.Length < 2)
                return;
            float totalDistance = positions.GetTotalDistance();
            float duration = totalDistance / travelSpeed;
            editPanel.transform.position = positions[0];
            body.transform.position = positions[0];
            followTween = body.transform.DOPath(positions, duration, PathType.Linear)
                .SetLoops(-1)
                .SetLookAt(0.02f)
                .SetEase(Ease.Linear);
        }
    }
}