using System;
using DG.Tweening;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class TravellerPathFollower : MonoBehaviour
    {
        [SerializeField] private FloatValue gameSpeed;
        [SerializeField] private Transform followBody;
        [SerializeField] private bool lockRotationToYaw = true;

        private Tween pathTween;
        private Action<bool> onPathEnded;
        private Vector3 lastSampledPosition;

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
            followBody.position = positions[0];
            AlignToDirection(positions[1] - positions[0]);

            this.onPathEnded = onPathEnded;
            float duration = positions.GetTotalDistance() / speed;
            lastSampledPosition = followBody.position;

            pathTween = followBody.DOPath(positions, duration, PathType.Linear, PathMode.Full3D)
                .SetEase(Ease.Linear)
                .SetTarget(this)
                .OnUpdate(UpdateRotationFromMovement)
                .OnComplete(OnPathTweenComplete);
        }

        private void UpdateRotationFromMovement()
        {
            Vector3 delta = followBody.position - lastSampledPosition;
            lastSampledPosition = followBody.position;
            AlignToDirection(delta);
        }

        private void AlignToDirection(Vector3 direction)
        {
            if (lockRotationToYaw)
                direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            followBody.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
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
