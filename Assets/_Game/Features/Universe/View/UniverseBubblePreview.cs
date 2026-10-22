using DG.Tweening;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal class UniverseBubblePreview
    {
        private readonly UniversePoseState poseState;
        private readonly Transform contentRoot;
        private readonly float secs;
        private readonly Ease ease;
        private Tween tween;

        public UniverseBubblePreview(UniversePoseState poseState, Transform contentRoot, float secs, Ease ease)
        {
            this.poseState = poseState;
            this.contentRoot = contentRoot;
            this.secs = secs;
            this.ease = ease;
        }

        public void Begin()
        {
            Kill();
            poseState.BeginPreview();
        }

        public void ApplyDrag(Vector3 worldOffset)
        {
            if (!poseState.IsPreviewActive)
                return;

            Kill();
            poseState.ApplyDrag(worldOffset);
        }

        public void Cancel()
        {
            if (!poseState.IsPreviewActive)
                return;

            UniversePose target = poseState.PreviewStartPose;
            poseState.CancelPreview();
            Kill();

            if (secs <= 0f)
            {
                poseState.ApplyPose(target);
                return;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Join(contentRoot.DOMove(target.position, secs).SetEase(ease));
            sequence.Join(contentRoot.DORotateQuaternion(target.rotation, secs).SetEase(ease));
            tween = sequence;
        }

        public bool Commit(float scale)
        {
            bool wasActive = poseState.IsPreviewActive;
            poseState.CommitPreview(scale);
            Kill();
            return wasActive;
        }

        public void End()
        {
            Kill();
            poseState.EndPreview();
        }

        private void Kill()
        {
            tween?.Kill();
            tween = null;
        }
    }
}
