using System;
using DigitalLove.FlowControl;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public class UniverseView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private UniverseBubbleSettings settings = new();

        private readonly ModeController<UniverseViewMode> mode = new(UniverseViewMode.Room);
        private UniversePoseState poseState;
        private UniverseViewTransition transition;
        private UniverseBubblePreview preview;

        public IModeController<UniverseViewMode> Mode => mode;
        public event Action BubbleTransformCommitted;
        public BubblePlacement BubblePlacement => new(poseState.BubbleOffsetLocal, poseState.BubbleYaw);
        internal UniverseMapFeedback MapFeedback { get; } = new();
        internal BubbleViewState State => new(
            Mode.Current,
            Mode.IsTransitioning,
            poseState.HasRoomAnchor,
            new BubbleMetrics(poseState.CurrentPose.position, poseState.Scale, poseState.MiniScale, poseState.BubbleRadiusWorld),
            poseState.CenterWorld);

        private void Awake()
        {
            poseState = new UniversePoseState(
                contentRoot,
                settings.bubbleDistance,
                settings.bubbleHeightOffset,
                settings.miniBubbleRadius,
                settings.minMiniScale);
            transition = new UniverseViewTransition(poseState, settings.transitionSecs, settings.transitionEase);
            preview = new UniverseBubblePreview(poseState, contentRoot, settings.transitionSecs, settings.transitionEase);
        }

        public void SetRoomAnchor(Pose pose, Bounds roomBounds)
        {
            bool hadAnchor = poseState.HasRoomAnchor;
            poseState.SetRoomAnchor(pose, roomBounds);
            if (Mode.IsTransitioning)
                return;

            if (!hadAnchor || Mode.Current == UniverseViewMode.Bubble)
            {
                poseState.SetBubbleYawToCamera();
                UniversePose bubblePose = BuildMiniPose();
                ApplyPose(bubblePose);
                poseState.CaptureBubbleCenter(bubblePose);
                mode.TrySet(UniverseViewMode.Bubble);
                return;
            }

            ApplyPose(poseState.RoomPose);
        }

        internal void SetBubbleView(UniverseViewMode target, bool instant)
        {
            if (!poseState.HasRoomAnchor || Mode.IsTransitioning || Mode.Current == target)
                return;

            CommitBubblePreview();
            if (instant || settings.transitionSecs <= 0f)
            {
                ApplyPose(PrepareTarget(target));
                mode.TrySet(target);
                return;
            }

            if (!mode.BeginTransition(target))
                return;

            StartTransition(PrepareTarget(target));
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

            UniversePose previous = poseState.CurrentPose;
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

        internal Vector3 WorldToContentLocal(Vector3 worldPosition) =>
            contentRoot.InverseTransformPoint(worldPosition);
        internal void ResetBubbleMap()
        {
            preview.End();
            if (Mode.Current == UniverseViewMode.Bubble && !Mode.IsTransitioning)
                poseState.RecenterBubble(poseState.MiniScale, settings.transitionSecs);
            else
                poseState.ResetBubbleOffset();
        }
        internal void SetContentCenterLocal(Vector3 centerLocal)
        {
            poseState.SetContentCenterLocal(centerLocal);
            if (Mode.Current == UniverseViewMode.Bubble && !Mode.IsTransitioning)
                poseState.ApplyBubblePose(poseState.MiniScale);
        }
        internal void BeginBubblePreview()
        {
            if (Mode.Current != UniverseViewMode.Bubble || Mode.IsTransitioning)
                return;

            preview.Begin();
        }
        internal void ApplyBubbleDrag(Vector3 worldOffset) => preview.ApplyDrag(worldOffset);
        internal void CancelBubblePreview() => preview.Cancel();
        internal void CommitBubblePreview()
        {
            if (preview.Commit(poseState.MiniScale))
                BubbleTransformCommitted?.Invoke();
        }
        public void RestoreBubbleTransform(Vector3 offsetLocal, Quaternion yaw)
        {
            poseState.RestoreBubbleTransform(offsetLocal, yaw);
            if (Mode.Current == UniverseViewMode.Bubble && !Mode.IsTransitioning)
                poseState.ApplyBubblePose(poseState.MiniScale);
        }

        private UniversePose BuildMiniPose() => poseState.BuildMiniPose(poseState.MiniScale);

        private UniversePose PrepareTarget(UniverseViewMode target)
        {
            if (target == UniverseViewMode.Room)
                return poseState.BuildRoomPose();

            poseState.ResetBubbleOffset();
            poseState.SetBubbleYawToCamera();
            UniversePose pose = BuildMiniPose();
            poseState.CaptureBubbleCenter(pose);
            return pose;
        }

        private void StartTransition(UniversePose target)
        {
            poseState.KillRecenter();
            preview.End();
            UniversePose start = poseState.CurrentPose;
            transition.Play(
                start,
                target,
                poseState.CenterLocal,
                poseState.CenterWorld,
                () => CompleteTransition(target));
        }

        private void CompleteTransition(UniversePose target)
        {
            ApplyPose(target);
            mode.CompleteTransition();
        }

        private void ApplyPose(UniversePose pose) => poseState.ApplyPose(pose);

        private void OnDisable()
        {
            transition?.Kill();
            preview?.End();
            poseState?.KillRecenter();
            mode.CancelTransition();
        }
    }
}
