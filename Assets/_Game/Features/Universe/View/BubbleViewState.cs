using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public readonly struct BubbleViewState
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

        public bool IsBubble => Mode == UniverseViewMode.Bubble && !IsTransitioning;

        public bool ContainsContentPoint(Vector3 worldPoint)
        {
            Vector3 local = Quaternion.Inverse(Metrics.FrameRotation) * (worldPoint - CenterWorld);
            float half = Metrics.ContentRadiusWorld;
            return Mathf.Abs(local.x) <= half && Mathf.Abs(local.y) <= half && Mathf.Abs(local.z) <= half;
        }
    }

    public struct BubbleMetrics
    {
        public Vector3 ContentPosition;
        public float Scale;
        public float MiniScale;
        public float BubbleRadiusWorld;
        public float ContentRadiusWorld;
        public Vector3 ScopeRadii;
        public Quaternion FrameRotation;
    }
}
