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
        private Tween blendTween;
        private Vector3 previewStartOffsetLocal;
        private bool isPreviewActive;
        private float bubbleBlend;

        public IModeController<UniverseViewMode> Mode => mode;
        public event Action BubbleTransformCommitted;
        public BubblePlacement BubblePlacement => rig.BuildPlacement();
        public bool IsBubbleView => Mode.Current == UniverseViewMode.Bubble && !Mode.IsTransitioning;
        public Vector3 FrameCenterWorld => rig.CenterWorld;
        public Vector3 ContentPosition => rig.ContentPosition;
        public Quaternion FrameRotation => rig.HomeRotation;
        public float ContentRadiusWorld => rig.ContentRadiusWorld;
        public Vector3 ScopeRadiiWorld => rig.ScopeRadiiWorld;
        public float BubbleBlend => bubbleBlend;
        internal UniverseMapFeedback MapFeedback { get; } = new();
        internal BubbleViewState State => new(
            Mode.Current, Mode.IsTransitioning, rig.HasRoomAnchor,
            new BubbleMetrics
            {
                ContentPosition = rig.ContentPosition,
                Scale = rig.Scale,
                MiniScale = rig.MiniScale,
                BubbleRadiusWorld = rig.BubbleRadiusWorld,
                ContentRadiusWorld = rig.ContentRadiusWorld,
                ScopeRadii = rig.ScopeRadiiWorld,
                FrameRotation = rig.HomeRotation
            },
            rig.CenterWorld);

        private void Awake()
        {
            rig = new UniverseContentRig(contentRoot, roomAnchor, bubbleAnchor, settings);
            rig.Initialize();
        }

        public void SetRoomAnchor(Pose pose)
        {
            bool hadAnchor = rig.HasRoomAnchor;
            rig.SetRoomAnchor(pose);
            if (Mode.IsTransitioning)
                return;

            if (!hadAnchor || Mode.Current == UniverseViewMode.Bubble)
            {
                rig.EnterBubble();
                ApplyBlend(1f, true);
                mode.TrySet(UniverseViewMode.Bubble);
                return;
            }

            rig.ApplyRoomPose();
            ApplyBlend(0f, true);
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
                ApplyBlend(target == UniverseViewMode.Bubble ? 1f : 0f, true);
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

        public void SetContentRadius(float contentRadiusLocal) => rig.SetContentRadiusLocal(contentRadiusLocal);

        internal void ResetBubbleMap()
        {
            isPreviewActive = false;
            if (Mode.Current == UniverseViewMode.Bubble && !Mode.IsTransitioning)
                TweenFocusOffset(Vector3.zero);
        }

        internal void BeginBubblePreview()
        {
            if (Mode.Current != UniverseViewMode.Bubble || Mode.IsTransitioning)
                return;

            KillAnchorTween();
            previewStartOffsetLocal = rig.FocusOffsetLocal;
            isPreviewActive = true;
        }

        internal void ApplyBubbleDrag(Vector3 worldOffset)
        {
            if (!isPreviewActive)
                return;

            rig.ApplyDrag(previewStartOffsetLocal, worldOffset);
        }

        internal void CancelBubblePreview()
        {
            if (!isPreviewActive)
                return;

            isPreviewActive = false;
            TweenFocusOffset(previewStartOffsetLocal);
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
            ApplyBlend(target == UniverseViewMode.Bubble ? 1f : 0f, false);
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
                rig.RecaptureBubble();
                scale = rig.MiniScale;
                position = rig.GetDisplayContentPosition(scale);
                rotation = rig.HomeRotation;
                return;
            }

            scale = 1f;
            position = rig.GetDisplayContentPosition(scale);
            rotation = rig.HomeRotation;
        }

        private void TweenFocusOffset(Vector3 targetOffset)
        {
            KillAnchorTween();
            if (settings.transitionSecs <= 0f)
            {
                rig.SetFocusOffset(targetOffset);
                return;
            }

            Vector3 start = rig.FocusOffsetLocal;
            anchorTween = DOTween
                .To(() => start, rig.SetFocusOffset, targetOffset, settings.transitionSecs)
                .SetEase(settings.transitionEase);
        }

        private void KillAnchorTween()
        {
            anchorTween?.Kill();
            anchorTween = null;
        }

        private void ApplyBlend(float target, bool instant)
        {
            blendTween?.Kill();
            blendTween = null;
            if (instant)
            {
                bubbleBlend = target;
                return;
            }

            blendTween = DOTween.To(() => bubbleBlend, v => bubbleBlend = v, target, settings.transitionSecs)
                .SetEase(settings.transitionEase);
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
            blendTween?.Kill();
            blendTween = null;
            isPreviewActive = false;
            mode.CancelTransition();
        }
    }
}
