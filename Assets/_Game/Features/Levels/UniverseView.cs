using System;
using DG.Tweening;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseView : MonoBehaviour
    {
        [Header("Bubble")]
        [SerializeField] private float miniBubbleRadius = 0.22f;
        [SerializeField] private float bubbleDistance = 0.45f;
        [SerializeField] private float bubbleHeightOffset = -0.1f;
        [SerializeField] private float minMiniScale = 0.02f;

        [Header("Transition")]
        [SerializeField] private float transitionSecs = 0.6f;
        [SerializeField] private Ease transitionEase = Ease.InOutSine;

        [Header("Debug")]

        private UniversePose roomPose;
        private Vector3 contentCenter;
        private float contentRadius = 2.5f;
        private bool hasRoomAnchor;
        private bool isMini;
        private bool isTransitioning;
        private Tween transition;

        public event Action<bool> TransitionStarted;
        public event Action<bool> TransitionCompleted;

        public bool IsMini => isMini;
        public bool IsTransitioning => isTransitioning;
        public bool HasRoomAnchor => hasRoomAnchor;
        public float Scale => transform.localScale.x;
        public Transform ContentRoot => transform;

        private void Awake()
        {
            roomPose = UniversePose.FromTransform(transform);
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

            UniversePose target = bubble ? BuildMiniPose() : roomPose;
            if (instant || transitionSecs <= 0f)
            {
                ApplyPose(target);
                SetMiniState(bubble);
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

        public UniversePose CurrentPose => new(transform.position, transform.rotation, transform.localScale.x);

        private UniversePose BuildMiniPose()
        {
            Camera camera = Camera.main;
            Quaternion rotation = YawOnly(roomPose.rotation);
            float scale = Mathf.Clamp(miniBubbleRadius / contentRadius, minMiniScale, 1f);
            Vector3 anchor = camera != null
                ? camera.transform.position + camera.transform.forward * bubbleDistance
                    + camera.transform.up * bubbleHeightOffset
                : transform.position + Vector3.forward * bubbleDistance;
            Vector3 position = anchor - rotation * (scale * contentCenter);
            return new UniversePose(position, rotation, scale);
        }

        private void StartTransition(UniversePose target, bool toMini)
        {
            transition?.Kill();
            isTransitioning = true;
            TransitionStarted?.Invoke(toMini);

            Sequence sequence = DOTween.Sequence();
            sequence.Join(transform.DOMove(target.position, transitionSecs).SetEase(transitionEase));
            sequence.Join(transform.DORotateQuaternion(target.rotation, transitionSecs).SetEase(transitionEase));
            sequence.Join(transform.DOScale(Vector3.one * target.scale, transitionSecs).SetEase(transitionEase));
            sequence.OnComplete(() => CompleteTransition(target, toMini));
            sequence.SetTarget(this);
            transition = sequence;
        }

        private void CompleteTransition(UniversePose target, bool toMini)
        {
            ApplyPose(target);
            transition = null;
            isTransitioning = false;
            SetMiniState(toMini);
            TransitionCompleted?.Invoke(toMini);
        }

        private void SetMiniState(bool mini)
        {
            if (isMini == mini)
                return;

            isMini = mini;
        }

        private void ApplyPose(UniversePose pose)
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
            transform.localScale = Vector3.one * pose.scale;
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
            transition?.Kill();
            transition = null;
            isTransitioning = false;
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
