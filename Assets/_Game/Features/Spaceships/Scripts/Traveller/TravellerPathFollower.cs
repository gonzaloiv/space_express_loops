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
            if (positions == null || positions.Length < 2)
            {
                onPathEnded?.Invoke(false);
                return;
            }

            float speed = gameSpeed != null ? gameSpeed.value : 0f;
            if (speed <= 0f)
            {
                onPathEnded?.Invoke(false);
                return;
            }

            CancelFollowing();
            SetPosition(positions[0]);
            AlignToDirection(positions[1] - positions[0]);

            this.onPathEnded = onPathEnded;
            float duration = positions.GetTotalDistance() / speed;
            pathTween = CreatePathTween(positions, duration);
        }

        private Tween CreatePathTween(Vector3[] positions, float duration)
        {
            return followBody
                .DOPath(positions, duration, PathType.Linear, PathMode.Full3D)
                .SetLookAt(PathLookAhead, true)
                .SetEase(Ease.Linear)
                .SetTarget(this)
                .OnComplete(OnPathTweenComplete);
        }

        private void SetPosition(Vector3 position)
        {
            followBody.position = position;
        }

        private void AlignToDirection(Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            followBody.rotation = rotation;
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
