using System;
using DG.Tweening;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal class UniverseViewTransition
    {
        private readonly UniversePoseState poseState;
        private readonly float secs;
        private readonly Ease ease;
        private Tween tween;

        public UniverseViewTransition(UniversePoseState poseState, float secs, Ease ease)
        {
            this.poseState = poseState;
            this.secs = secs;
            this.ease = ease;
        }

        public void Play(
            UniversePose start,
            UniversePose target,
            Vector3 centerLocal,
            Vector3 startCenter,
            Action onComplete)
        {
            Kill();
            tween = DOTween.To(
                    () => 0f,
                    t => poseState.ApplyTransitionPose(start, target, centerLocal, startCenter, t),
                    1f,
                    secs)
                .SetEase(ease)
                .OnComplete(() =>
                {
                    tween = null;
                    onComplete?.Invoke();
                });
        }

        public void Kill()
        {
            tween?.Kill();
            tween = null;
        }
    }
}
