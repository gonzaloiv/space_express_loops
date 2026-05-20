using System;
using DG.Tweening;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class TravellerPathFollower : MonoBehaviour
    {
        private const float PathLookAhead = 0.01f;

        [SerializeField] private FloatValue gameSpeed;
        [SerializeField] private Transform followBody;

        private Tween pathTween;

        private Action<bool> onPathEnded;

        public bool IsFollowingPath => pathTween != null && pathTween.IsActive();

        public void FollowPath(Vector3[] positions, Action<bool> onPathEnded)
        {
            CancelFollowing();
            SetPosition(positions[0]);

            this.onPathEnded = onPathEnded;
            float duration = positions.GetTotalDistance() / gameSpeed.value;
            pathTween = CreatePathTween(positions, duration);
        }

        private Tween CreatePathTween(Vector3[] positions, float duration)
        {
            return followBody
                .DOPath(positions, duration, PathType.Linear, PathMode.Full3D)
                .SetLookAt(PathLookAhead)
                .SetEase(Ease.Linear)
                .SetTarget(this)
                .OnComplete(OnPathTweenComplete);
        }

        private void SetPosition(Vector3 position)
        {
            followBody.position = position;
        }

        public void CancelFollowing()
        {
            KillTween();
            onPathEnded = null;
        }

        public void EndWithFailure()
        {
            KillTween();
            onPathEnded?.Invoke(false);
            onPathEnded = null;
        }

        public void EndWithSuccess()
        {
            KillTween();
            onPathEnded?.Invoke(true);
            onPathEnded = null;
        }

        private void KillTween()
        {
            if (pathTween != null && pathTween.IsActive())
                pathTween.Kill(false);
            pathTween = null;
        }

        private void OnDestroy()
        {
            KillTween();
        }

        private void OnPathTweenComplete()
        {
            pathTween = null;
            onPathEnded?.Invoke(true);
            onPathEnded = null;
        }
    }
}
