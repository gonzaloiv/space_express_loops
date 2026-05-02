using System;
using DG.Tweening;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{

    public class TravellerPathFollower : MonoBehaviour
    {
        private const float MinSpeed = 1e-4f;

        [SerializeField] private FloatValue gameSpeed;
        [SerializeField] private Transform followBody;

        private Tween pathTween;
        private float pathReferenceSpeed;
        private Action<bool> onPathEnded;

        private void Update()
        {
            if (pathTween == null || !pathTween.IsActive())
                return;
            pathTween.timeScale = Mathf.Max(gameSpeed.value, MinSpeed) / pathReferenceSpeed;
        }

        public void FollowPath(Vector3[] positions, Action<bool> onPathEnded)
        {
            StopFollowing();
            if (positions == null || positions.Length == 0)
                return;

            this.onPathEnded = onPathEnded;
            followBody.position = positions[0];

            float pathLength = positions.GetTotalDistance();
            if (pathLength < 1e-6f)
            {
                CompletePathFollowing();
                return;
            }

            pathReferenceSpeed = Mathf.Max(gameSpeed.value, MinSpeed);
            float duration = pathLength / pathReferenceSpeed;

            var pathPoints = new Vector3[positions.Length];
            Array.Copy(positions, pathPoints, positions.Length);

            pathTween = followBody.DOPath(pathPoints, duration, PathType.Linear, PathMode.Full3D)
                .SetEase(Ease.Linear)
                .SetTarget(this)
                .SetLookAt(0.01f)
                .OnComplete(OnPathTweenComplete);
        }

        public void StopFollowing()
        {
            if (pathTween != null && pathTween.IsActive())
                pathTween.Kill(false);
            pathTween = null;
            onPathEnded = null;
        }

        public void EndWithFailure()
        {
            if (pathTween != null && pathTween.IsActive())
                pathTween.Kill(false);
            pathTween = null;
            onPathEnded?.Invoke(false);
            onPathEnded = null;
        }

        private void OnPathTweenComplete()
        {
            pathTween = null;
            CompletePathFollowing();
        }

        private void CompletePathFollowing()
        {
            onPathEnded?.Invoke(true);
            onPathEnded = null;
        }
    }
}
