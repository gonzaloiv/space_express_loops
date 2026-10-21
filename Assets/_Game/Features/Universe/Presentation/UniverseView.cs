using System;
using DG.Tweening;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public class UniverseView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentRoot;

        [Header("Bubble")]
        [SerializeField, Min(0.05f)] private float miniBubbleRadius = 0.28f;
        [SerializeField, Min(0.1f)] private float bubbleDistance = 0.45f;
        [SerializeField] private float bubbleHeightOffset = -0.1f;
        [SerializeField, Min(0.001f)] private float minMiniScale = 0.02f;

        [Header("Transition")]
        [SerializeField, Min(0f)] private float transitionSecs = 0.6f;
        [SerializeField] private Ease transitionEase = Ease.InOutSine;

        private UniversePoseState poseState;
        private bool isMini;
        private bool isTransitioning;
        private Tween transition;
        private Tween previewTween;

        public event Action<bool> TransitionStarted;
        public event Action<bool> TransitionCompleted;
        public event Action BubbleTransformCommitted;
        public bool IsMini => isMini;
        public bool IsTransitioning => isTransitioning;
        public bool HasRoomAnchor => poseState.HasRoomAnchor;
        public float Scale => contentRoot.localScale.x;
        public float MiniScale => poseState.GetMiniScale(miniBubbleRadius, minMiniScale);
        public float BubbleRadiusWorld => poseState.ContentRadius * Scale;
        public Vector3 CenterWorld => poseState.CenterWorld;
        public Vector3 BubbleOffsetLocal => poseState.BubbleOffsetLocal;
        public bool IsMapGestureBlocked { get; private set; }
        public bool IsMapGestureActive { get; private set; }
        public float ScaleFeedback { get; private set; }
        public Quaternion BubbleYaw => poseState.BubbleYaw;
        private void Awake()
        {
            poseState = new UniversePoseState(contentRoot, bubbleDistance, bubbleHeightOffset);
        }
        public void SetRoomAnchor(Pose pose, Bounds roomBounds)
        {
            bool hadAnchor = poseState.HasRoomAnchor;
            poseState.SetRoomAnchor(pose, roomBounds);
            if (isTransitioning)
                return;

            if (!hadAnchor || isMini)
            {
                poseState.SetBubbleYawToCamera();
                UniversePose bubblePose = BuildMiniPose();
                ApplyPose(bubblePose);
                poseState.CaptureBubbleCenter(bubblePose);
                isMini = true;
                return;
            }

            ApplyPose(poseState.RoomPose);
        }
        public void SetBubbleView(bool bubble, bool instant)
        {
            if (!HasRoomAnchor || isTransitioning)
                return;

            CommitBubblePreview();
            if (bubble)
            {
                poseState.ResetBubbleOffset();
                poseState.SetBubbleYawToCamera();
            }
            UniversePose target = bubble ? BuildMiniPose() : poseState.BuildRoomPose();
            if (bubble)
                poseState.CaptureBubbleCenter(target);
            if (instant || transitionSecs <= 0f)
            {
                ApplyPose(target);
                isMini = bubble;
                TransitionCompleted?.Invoke(bubble);
                return;
            }

            StartTransition(target, bubble);
        }
        public void WithRoomPose(Action action)
        {
            if (action == null)
                return;

            if (!isMini)
            {
                action();
                return;
            }

            UniversePose previous = CurrentPose;
            poseState.ApplyPose(poseState.BuildRoomPose());
            try
            {
                action();
            }
            finally
            {
                poseState.ApplyPose(previous);
            }
        }
        public UniversePose CurrentPose => poseState.CurrentPose;
        public Vector3 WorldToContentLocal(Vector3 worldPosition) =>
            contentRoot.InverseTransformPoint(worldPosition);
        public void SetMapGestureBlocked(bool blocked) => IsMapGestureBlocked = blocked;
        public void SetMapGestureActive(bool active) => IsMapGestureActive = active;
        public void SetScaleFeedback(float value) => ScaleFeedback = value;
        public void ResetBubbleMap()
        {
            previewTween?.Kill();
            previewTween = null;
            poseState.EndPreview();
            if (isMini && !isTransitioning)
                poseState.RecenterBubble(MiniScale, transitionSecs);
            else
                poseState.ResetBubbleOffset();
        }
        public void SetContentCenterLocal(Vector3 centerLocal)
        {
            poseState.SetContentCenterLocal(centerLocal);
            if (isMini && !isTransitioning)
                poseState.ApplyBubblePose(MiniScale);
        }
        public void BeginBubblePreview()
        {
            if (!isMini || isTransitioning)
                return;

            previewTween?.Kill();
            poseState.BeginPreview();
        }
        public void ApplyBubbleDrag(Vector3 worldOffset)
        {
            if (!poseState.IsPreviewActive)
                return;

            previewTween?.Kill();
            poseState.ApplyDrag(worldOffset);
        }

        public void CancelBubblePreview()
        {
            if (!poseState.IsPreviewActive)
                return;

            UniversePose target = poseState.PreviewStartPose;
            poseState.CancelPreview();
            previewTween?.Kill();

            if (transitionSecs <= 0f)
            {
                ApplyPose(target);
                return;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Join(contentRoot.DOMove(target.position, transitionSecs).SetEase(transitionEase));
            sequence.Join(contentRoot.DORotateQuaternion(target.rotation, transitionSecs).SetEase(transitionEase));
            sequence.SetTarget(this);
            previewTween = sequence;
        }

        public void CommitBubblePreview()
        {
            bool wasPreviewActive = poseState.IsPreviewActive;
            poseState.CommitPreview(MiniScale);
            previewTween?.Kill();
            previewTween = null;

            if (wasPreviewActive)
                BubbleTransformCommitted?.Invoke();
        }

        public void RestoreBubbleTransform(Vector3 offsetLocal, Quaternion yaw)
        {
            poseState.RestoreBubbleTransform(offsetLocal, yaw);
            if (isMini && !isTransitioning)
                poseState.ApplyBubblePose(MiniScale);
        }

        private UniversePose BuildMiniPose() => poseState.BuildMiniPose(MiniScale);

        private void StartTransition(UniversePose target, bool toMini)
        {
            transition?.Kill();
            poseState.KillRecenter();
            previewTween?.Kill();
            previewTween = null;
            poseState.EndPreview();
            isTransitioning = true;
            TransitionStarted?.Invoke(toMini);

            UniversePose start = CurrentPose;
            Vector3 centerLocal = poseState.CenterLocal;
            Vector3 startCenter = poseState.CenterWorld;
            Vector3 endCenter = target.position + target.rotation * (target.scale * centerLocal);
            transition = DOTween.To(
                    () => 0f,
                    t => poseState.ApplyTransitionPose(start, target, centerLocal, startCenter, endCenter, t),
                    1f,
                    transitionSecs)
                .SetEase(transitionEase)
                .SetTarget(this)
                .OnComplete(() => CompleteTransition(target, toMini));
        }

        private void CompleteTransition(UniversePose target, bool toMini)
        {
            ApplyPose(target);
            transition = null;
            isTransitioning = false;
            isMini = toMini;
            TransitionCompleted?.Invoke(toMini);
        }

        private void ApplyPose(UniversePose pose) => poseState.ApplyPose(pose);

        private void OnDisable()
        {
            bool wasTransitioning = isTransitioning;
            transition?.Kill();
            transition = null;
            previewTween?.Kill();
            previewTween = null;
            poseState.KillRecenter();
            poseState.EndPreview();
            isTransitioning = false;
            if (wasTransitioning)
                TransitionCompleted?.Invoke(isMini);
        }

        public readonly struct UniversePose
        {
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly float scale;

            public UniversePose(Vector3 position, Quaternion rotation, float scale)
            {
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }

            public static UniversePose FromTransform(Transform transform) =>
                new(transform.position, transform.rotation, transform.localScale.x);
        }
    }
}
