using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal static class PuzzleRunGeometry
    {
        public static float DistanceToSegment(Vector3 point, Vector3 from, Vector3 to)
        {
            Vector3 segment = to - from;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.0000001f)
                return Vector3.Distance(point, from);

            float t = Mathf.Clamp01(Vector3.Dot(point - from, segment) / lengthSq);
            return Vector3.Distance(point, from + segment * t);
        }
    }
}
