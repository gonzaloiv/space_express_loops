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
        [SerializeField] private Transform contentRoot;
        [SerializeField] private Transform pathSpace;

        private Tween pathTween;

        private Action<bool> onPathEnded;

        public bool IsFollowingPath => pathTween != null && pathTween.IsActive();

        public void FollowPath(Vector3[] contentLocalPositions, Action<bool> onPathEnded)
        {
            CancelFollowing();
            Vector3[] pathLocal = ToPathSpace(contentLocalPositions);
            SetPosition(pathLocal[0]);

            this.onPathEnded = onPathEnded;
            float duration = pathLocal.GetTotalDistance() / gameSpeed.value;
            pathTween = CreatePathTween(pathLocal, duration);
        }

        private Vector3[] ToPathSpace(Vector3[] contentLocalPositions)
        {
            Vector3[] pathLocal = new Vector3[contentLocalPositions.Length];
            for (int i = 0; i < contentLocalPositions.Length; i++)
            {
                Vector3 world = contentRoot.TransformPoint(contentLocalPositions[i]);
                pathLocal[i] = pathSpace.InverseTransformPoint(world);
            }

            return pathLocal;
        }

        private Tween CreatePathTween(Vector3[] positions, float duration)
        {
            return followBody
                .DOLocalPath(positions, duration, PathType.Linear, PathMode.Full3D)
                .SetLookAt(PathLookAhead)
                .SetEase(Ease.Linear)
                .SetTarget(this)
                .OnComplete(OnPathTweenComplete);
        }

        private void SetPosition(Vector3 position)
        {
            followBody.localPosition = position;
        }

        public void PauseFollowing()
        {
            if (pathTween != null && pathTween.IsActive())
                pathTween.Pause();
        }

        public void ResumeFollowing()
        {
            if (pathTween != null && pathTween.IsActive())
                pathTween.Play();
        }

        public void CancelFollowing()
        {
            KillTween();
            onPathEnded = null;
        }

        public void EndFollowing(bool success)
        {
            KillTween();
            onPathEnded?.Invoke(success);
            onPathEnded = null;
        }

        private void KillTween()
        {
            if (pathTween != null && pathTween.IsActive())
                pathTween.Kill(false);
            pathTween = null;
        }

        private void OnDisable()
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
