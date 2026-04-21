using DigitalLove.FlowControl;
using DG.Tweening;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class OnRouteState : MonoState
    {
        [SerializeField] private GameObject body;
        [SerializeField] private BezierRay bezierRay;
        [SerializeField] private float travelDuration = 5f;

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
            body.transform.position = positions[0];
            followTween = body.transform.DOPath(positions, travelDuration, PathType.Linear)
                .SetLoops(-1)
                .SetLookAt(0.01f)
                .SetEase(Ease.Linear);
        }
    }
}