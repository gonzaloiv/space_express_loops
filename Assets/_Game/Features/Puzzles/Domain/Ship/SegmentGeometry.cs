using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class SegmentGeometry
    {
        internal static float ClosestT(Vector3 point, Vector3 from, Vector3 to)
        {
            Vector3 segment = to - from;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.0000001f)
                return 0f;

            return Mathf.Clamp01(Vector3.Dot(point - from, segment) / lengthSq);
        }

        public static float DistanceToSegment(Vector3 point, Vector3 from, Vector3 to) =>
            Vector3.Distance(point, from + (to - from) * ClosestT(point, from, to));
    }
}
