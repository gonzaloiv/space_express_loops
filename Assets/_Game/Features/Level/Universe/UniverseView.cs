using System;
using DG.Tweening;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentRoot;

        [Header("Bubble")]
        [SerializeField] private float miniBubbleRadius = 0.28f;
        [SerializeField] private float bubbleDistance = 0.45f;
        [SerializeField] private float bubbleHeightOffset = -0.1f;
        [SerializeField] private float minMiniScale = 0.02f;

        [Header("Transition")]
        [SerializeField] private float transitionSecs = 0.6f;
        [SerializeField] private Ease transitionEase = Ease.InOutSine;

        private UniversePose roomPose;
        private Vector3 contentCenter;
        private float contentRadius = 2.5f;
        private bool hasRoomAnchor;
        private bool isMini;
        private bool isTransitioning;
        private bool isBubblePreviewActive;
        private UniversePose bubblePreviewStartPose;
        private Tween transition;
        private Tween previewTween;

        public event Action<bool> TransitionStarted;
        public event Action<bool> TransitionCompleted;

        public bool IsMini => isMini;
        public bool IsTransitioning => isTransitioning;
        public bool HasRoomAnchor => hasRoomAnchor;
        public float Scale => contentRoot.localScale.x;
        public float ContentRadius => contentRadius;
        public float MiniScale => Mathf.Clamp(miniBubbleRadius / contentRadius, minMiniScale, 1f);
        public UniversePose BubblePose => isMini ? CurrentPose : BuildMiniPose();

        private void Awake()
        {
            roomPose = UniversePose.FromTransform(contentRoot);
        }

        public void SetRoomAnchor(Pose pose, Bounds roomBounds)
        {
            Quaternion yaw = YawOnly(pose.rotation);
            roomPose = new UniversePose(pose.position, yaw, 1f);
            if (roomBounds.size.sqrMagnitude > 0.0001f)
            {
                contentCenter = Quaternion.Inverse(yaw) * (roomBounds.center - pose.position);
                contentRadius = Mathf.Max(roomBounds.extents.magnitude, 0.1f);
            }
            hasRoomAnchor = true;

            if (!isMini && !isTransitioning)
                ApplyPose(roomPose);
        }

        [Button]
        public void ToggleView() => SetBubbleView(!isMini, false);

        public void SetBubbleView(bool bubble, bool instant)
        {
            if (!hasRoomAnchor || isTransitioning)
                return;

            CommitBubblePreview();
            UniversePose target = bubble ? BuildMiniPose() : roomPose;
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
            ApplyPose(roomPose);
            try
            {
                action();
            }
            finally
            {
                ApplyPose(previous);
            }
        }

        public UniversePose CurrentPose => new(contentRoot.position, contentRoot.rotation, contentRoot.localScale.x);

        public void BeginBubblePreview()
        {
            if (!isMini || isTransitioning || isBubblePreviewActive)
                return;

            previewTween?.Kill();
            bubblePreviewStartPose = CurrentPose;
            isBubblePreviewActive = true;
        }

        public void ApplyBubblePreview(Vector3 position, Quaternion rotation)
        {
            if (!isBubblePreviewActive)
                return;

            previewTween?.Kill();
            contentRoot.position = new Vector3(position.x, bubblePreviewStartPose.position.y, position.z);
            contentRoot.rotation = YawOnly(rotation);
        }

        public void CancelBubblePreview()
        {
            if (!isBubblePreviewActive)
                return;

            isBubblePreviewActive = false;
            previewTween?.Kill();

            if (transitionSecs <= 0f)
            {
                ApplyPose(bubblePreviewStartPose);
                return;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Join(contentRoot.DOMove(bubblePreviewStartPose.position, transitionSecs).SetEase(transitionEase));
            sequence.Join(contentRoot.DORotateQuaternion(bubblePreviewStartPose.rotation, transitionSecs).SetEase(transitionEase));
            sequence.SetTarget(this);
            previewTween = sequence;
        }

        public void CommitBubblePreview()
        {
            isBubblePreviewActive = false;
            previewTween?.Kill();
            previewTween = null;
        }

        private UniversePose BuildMiniPose()
        {
            Camera camera = Camera.main;
            Quaternion rotation = YawOnly(roomPose.rotation);
            float scale = MiniScale;
            Vector3 anchor = camera != null
                ? camera.transform.position + camera.transform.forward * bubbleDistance
                    + camera.transform.up * bubbleHeightOffset
                : contentRoot.position + Vector3.forward * bubbleDistance;
            Vector3 position = anchor - rotation * (scale * contentCenter);
            return new UniversePose(position, rotation, scale);
        }

        private void StartTransition(UniversePose target, bool toMini)
        {
            transition?.Kill();
            previewTween?.Kill();
            previewTween = null;
            isBubblePreviewActive = false;
            isTransitioning = true;
            TransitionStarted?.Invoke(toMini);

            Sequence sequence = DOTween.Sequence();
            sequence.Join(contentRoot.DOMove(target.position, transitionSecs).SetEase(transitionEase));
            sequence.Join(contentRoot.DORotateQuaternion(target.rotation, transitionSecs).SetEase(transitionEase));
            sequence.Join(contentRoot.DOScale(Vector3.one * target.scale, transitionSecs).SetEase(transitionEase));
            sequence.OnComplete(() => CompleteTransition(target, toMini));
            sequence.SetTarget(this);
            transition = sequence;
        }

        private void CompleteTransition(UniversePose target, bool toMini)
        {
            ApplyPose(target);
            transition = null;
            isTransitioning = false;
            isMini = toMini;
            TransitionCompleted?.Invoke(toMini);
        }

        private void ApplyPose(UniversePose pose)
        {
            contentRoot.position = pose.position;
            contentRoot.rotation = pose.rotation;
            contentRoot.localScale = Vector3.one * pose.scale;
        }

        private static Quaternion YawOnly(Quaternion rotation)
        {
            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(forward.normalized, Vector3.up)
                : Quaternion.identity;
        }

        private void OnDisable()
        {
            bool wasTransitioning = isTransitioning;
            transition?.Kill();
            transition = null;
            previewTween?.Kill();
            previewTween = null;
            isBubblePreviewActive = false;
            isTransitioning = false;
            if (wasTransitioning)
                TransitionCompleted?.Invoke(isMini);
        }

        private void OnValidate()
        {
            miniBubbleRadius = Mathf.Max(0.05f, miniBubbleRadius);
            bubbleDistance = Mathf.Max(0.1f, bubbleDistance);
            transitionSecs = Mathf.Max(0f, transitionSecs);
            minMiniScale = Mathf.Max(0.001f, minMiniScale);
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
