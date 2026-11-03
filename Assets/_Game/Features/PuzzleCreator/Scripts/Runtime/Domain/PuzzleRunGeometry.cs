using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleRunGeometry
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

        public static float MaxChordDeviation(TrajectoryResult result, Vector3 from, Vector3 to)
        {
            if (!result.HasPoints)
                return 0f;

            float max = 0f;
            for (int i = 0; i < result.Points.Count; i++)
                max = Mathf.Max(max, DistanceToSegment(result.Points[i], from, to));

            return max;
        }

        public static float MaxAngle(TrajectoryResult result, Vector3 from, Vector3 to)
        {
            if (!result.HasPoints || result.Points.Count < 2)
                return 0f;

            Vector3 chord = to - from;
            if (chord.sqrMagnitude < 0.0000001f)
                return 0f;

            Vector3 chordDirection = chord.normalized;
            float max = 0f;
            for (int i = 1; i < result.Points.Count; i++)
            {
                Vector3 segment = result.Points[i] - result.Points[i - 1];
                if (segment.sqrMagnitude < 0.0000001f)
                    continue;

                max = Mathf.Max(max, Vector3.Angle(chordDirection, segment));
            }

            return max;
        }
    }
}
