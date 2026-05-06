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

        private Tween pathTween;
        private Action<bool> onPathEnded;

        public void FollowPath(Vector3[] positions, Action<bool> onPathEnded)
        {
            StopFollowing();
            followBody.position = positions[0];

            this.onPathEnded = onPathEnded;
            float duration = positions.GetTotalDistance() / gameSpeed.value;

            pathTween = followBody.DOPath(positions, duration, PathType.Linear, PathMode.Full3D)
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
            OnPathTweenComplete();
        }

        private void OnPathTweenComplete()
        {
            pathTween = null;
            onPathEnded?.Invoke(true);
            onPathEnded = null;
        }
    }
}
