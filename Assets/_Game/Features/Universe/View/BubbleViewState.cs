using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal readonly struct BubbleMetrics
    {
        public readonly Vector3 ContentPosition;
        public readonly float Scale;
        public readonly float MiniScale;
        public readonly float BubbleRadiusWorld;
        public readonly float ContentRadiusWorld;
        public readonly Quaternion FrameRotation;

        public BubbleMetrics(
            Vector3 contentPosition,
            float scale,
            float miniScale,
            float bubbleRadiusWorld,
            float contentRadiusWorld,
            Quaternion frameRotation)
        {
            ContentPosition = contentPosition;
            Scale = scale;
            MiniScale = miniScale;
            BubbleRadiusWorld = bubbleRadiusWorld;
            ContentRadiusWorld = contentRadiusWorld;
            FrameRotation = frameRotation;
        }
    }

    internal readonly struct BubbleViewState
    {
        public readonly UniverseViewMode Mode;
        public readonly bool IsTransitioning;
        public readonly bool HasRoomAnchor;
        public readonly BubbleMetrics Metrics;
        public readonly Vector3 CenterWorld;

        public BubbleViewState(
            UniverseViewMode mode,
            bool isTransitioning,
            bool hasRoomAnchor,
            BubbleMetrics metrics,
            Vector3 centerWorld)
        {
            Mode = mode;
            IsTransitioning = isTransitioning;
            HasRoomAnchor = hasRoomAnchor;
            Metrics = metrics;
            CenterWorld = centerWorld;
        }

        public bool IsBubble => Mode == UniverseViewMode.Bubble;

        public bool ContainsContentPoint(Vector3 worldPoint)
        {
            Vector3 local = Quaternion.Inverse(Metrics.FrameRotation) * (worldPoint - CenterWorld);
            float half = Metrics.ContentRadiusWorld;
            return Mathf.Abs(local.x) <= half && Mathf.Abs(local.y) <= half && Mathf.Abs(local.z) <= half;
        }
    }
}
