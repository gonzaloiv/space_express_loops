using DigitalLove.FlowControl;
using DG.Tweening;
using UnityEngine;
using DigitalLove.Global;

namespace DigitalLove.Game.Spaceships
{
    public class OnRouteState : MonoState
    {
        [SerializeField] private GameObject body;
        [SerializeField] private BezierRay bezierRay;
        [SerializeField] private float travelSpeed = 2f;

        private Vector3[] positions;
        private Tween followTween;

        public override void Init(StateMachine parent)
        {
            base.Init(parent);
            body.SetActive(false);
        }

        public override void Enter()
        {
            body.SetActive(true);
            positions = bezierRay.GetSplinePositions();
            FollowPath();
        }

        public override void Exit()
        {
            followTween?.Kill();
            body.SetActive(false);
        }

        private void FollowPath()
        {
            if (positions == null || positions.Length < 2)
                return;
            float totalDistance = positions.GetTotalDistance();
            float duration = totalDistance / travelSpeed;
            body.transform.position = positions[0];
            followTween = body.transform.DOPath(positions, duration, PathType.Linear)
                .SetLoops(-1)
                .SetLookAt(0.01f)
                .SetEase(Ease.Linear);
        }
    }
}