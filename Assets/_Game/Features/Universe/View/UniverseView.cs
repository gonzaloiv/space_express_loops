using System;
using DG.Tweening;
using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public class UniverseView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private Transform roomAnchor;
        [SerializeField] private Transform bubbleAnchor;
        [SerializeField] private UniverseBubbleSettings settings = new();

        private readonly ModeController<UniverseViewMode> mode = new(UniverseViewMode.Room);
        private UniverseContentRig rig;
        private Tween anchorTween;
        private Tween transitionTween;
        private Vector3 previewStartPosition;
        private Quaternion previewStartRotation;
        private bool isPreviewActive;

        public IModeController<UniverseViewMode> Mode => mode;
        public event Action BubbleTransformCommitted;
        public BubblePlacement BubblePlacement => rig.BuildPlacement();
        internal UniverseMapFeedback MapFeedback { get; } = new();
        internal BubbleViewState State => new(
            Mode.Current, Mode.IsTransitioning, rig.HasRoomAnchor,
            new BubbleMetrics(rig.ContentPosition, rig.Scale, rig.MiniScale, rig.BubbleRadiusWorld),
            rig.CenterWorld);

        private void Awake()
        {
            rig = new UniverseContentRig(contentRoot, roomAnchor, bubbleAnchor, settings);
            rig.Initialize();
        }

        public void SetRoomAnchor(Pose pose, Bounds roomBounds)
        {
            bool hadAnchor = rig.HasRoomAnchor;
            rig.SetRoomAnchor(pose, roomBounds);
            if (Mode.IsTransitioning)
                return;

            if (!hadAnchor || Mode.Current == UniverseViewMode.Bubble)
            {
                rig.EnterBubble();
                mode.TrySet(UniverseViewMode.Bubble);
                return;
            }

            rig.ApplyRoomPose();
        }

        internal void SetBubbleView(UniverseViewMode target, bool instant)
        {
            if (!rig.HasRoomAnchor || Mode.IsTransitioning || Mode.Current == target)
                return;

            CommitBubblePreview();
            if (instant || settings.transitionSecs <= 0f)
            {
                if (target == UniverseViewMode.Room)
                    rig.ApplyRoomPose();
                else
                    rig.EnterBubble();
                mode.TrySet(target);
                return;
            }

            if (mode.BeginTransition(target))
                PlayTransition(target);
        }

        public void WithRoomPose(Action action)
        {
            if (action == null)
                return;

            if (Mode.Current == UniverseViewMode.Room)
            {
                action();
                return;
            }

            rig.BeginTemporaryRoomPose();
            try
            {
                action();
            }
            finally
            {
                rig.EndTemporaryRoomPose(Mode.Current, Mode.IsTransitioning);
            }
        }

        public void SetContentCenter(Vector3 worldCenter)
        {
            rig.SetContentCenterLocal(
                rig.WorldToContentLocal(worldCenter),
                Mode.Current == UniverseViewMode.Bubble && !Mode.IsTransitioning);
        }

        internal void ResetBubbleMap()
        {
            KillAnchorTween();
            isPreviewActive = false;
            if (Mode.Current == UniverseViewMode.Bubble && !Mode.IsTransitioning)
                TweenAnchor(rig.HomePosition, rig.HomeRotation, rig.MiniScale);
        }

        internal void BeginBubblePreview()
        {
            if (Mode.Current != UniverseViewMode.Bubble || Mode.IsTransitioning)
                return;

            KillAnchorTween();
            rig.ParentContentToBubble();
            previewStartPosition = bubbleAnchor.position;
            previewStartRotation = bubbleAnchor.rotation;
            isPreviewActive = true;
        }

        internal void ApplyBubbleDrag(Vector3 worldOffset)
        {
            if (!isPreviewActive)
                return;

            Vector3 position = previewStartPosition + worldOffset;
            position.y = previewStartPosition.y;
            bubbleAnchor.SetPositionAndRotation(position, previewStartRotation);
        }

        internal void CancelBubblePreview()
        {
            if (!isPreviewActive)
                return;

            isPreviewActive = false;
            TweenAnchor(previewStartPosition, previewStartRotation, bubbleAnchor.localScale.x);
        }

        internal void CommitBubblePreview()
        {
            if (!isPreviewActive)
                return;

            isPreviewActive = false;
            KillAnchorTween();
            BubbleTransformCommitted?.Invoke();
        }

        public void RestoreBubbleTransform(Vector3 offsetLocal, Quaternion yaw) =>
            rig.RestorePlacement(
                offsetLocal,
                yaw,
                Mode.Current == UniverseViewMode.Bubble && !Mode.IsTransitioning);

        private void PlayTransition(UniverseViewMode target)
        {
            KillTransition();
            KillAnchorTween();
            isPreviewActive = false;
            contentRoot.SetParent(roomAnchor, true);
            ResolveTransitionTarget(target, out Vector3 position, out Quaternion rotation, out float scale);

            Sequence sequence = DOTween.Sequence();
            sequence.Join(contentRoot.DOMove(position, settings.transitionSecs).SetEase(settings.transitionEase));
            sequence.Join(contentRoot.DORotateQuaternion(rotation, settings.transitionSecs).SetEase(settings.transitionEase));
            sequence.Join(contentRoot.DOScale(scale, settings.transitionSecs).SetEase(settings.transitionEase));
            transitionTween = sequence.OnComplete(() =>
            {
                transitionTween = null;
                rig.CompleteTransition(target);
                mode.CompleteTransition();
            });
        }

        private void ResolveTransitionTarget(
            UniverseViewMode target,
            out Vector3 position,
            out Quaternion rotation,
            out float scale)
        {
            if (target == UniverseViewMode.Bubble)
            {
                rig.PrepareBubble();
                scale = bubbleAnchor.localScale.x;
                position = rig.GetBubbleContentPosition();
                rotation = bubbleAnchor.rotation;
                return;
            }

            position = roomAnchor.position;
            rotation = roomAnchor.rotation;
            scale = 1f;
        }

        private void TweenAnchor(Vector3 position, Quaternion rotation, float scale)
        {
            KillAnchorTween();
            if (settings.transitionSecs <= 0f)
            {
                bubbleAnchor.SetPositionAndRotation(position, rotation);
                bubbleAnchor.localScale = Vector3.one * scale;
                return;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Join(bubbleAnchor.DOMove(position, settings.transitionSecs).SetEase(settings.transitionEase));
            sequence.Join(bubbleAnchor.DORotateQuaternion(rotation, settings.transitionSecs).SetEase(settings.transitionEase));
            sequence.Join(bubbleAnchor.DOScale(scale, settings.transitionSecs).SetEase(settings.transitionEase));
            anchorTween = sequence;
        }

        private void KillAnchorTween()
        {
            anchorTween?.Kill();
            anchorTween = null;
        }

        private void KillTransition()
        {
            transitionTween?.Kill();
            transitionTween = null;
        }
        private void OnDisable()
        {
            KillTransition();
            KillAnchorTween();
            isPreviewActive = false;
            mode.CancelTransition();
        }
    }
}
