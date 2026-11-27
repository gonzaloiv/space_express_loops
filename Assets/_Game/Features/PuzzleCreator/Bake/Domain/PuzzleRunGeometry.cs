using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleRunGeometry
    {
        public static float DistanceToSegment(Vector3 point, Vector3 from, Vector3 to) =>
            SegmentGeometry.DistanceToSegment(point, from, to);

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

        // The point of the run nearest a target; the fan-out aim moves a destination onto it.
        public static Vector3 ClosestPoint(TrajectoryResult result, Vector3 point)
        {
            if (!result.HasPoints)
                return point;

            Vector3 best = result.Points[0];
            float bestSquared = (best - point).sqrMagnitude;
            for (int i = 1; i < result.Points.Count; i++)
            {
                Vector3 candidate =
                    SegmentGeometry.ClosestPoint(point, result.Points[i - 1], result.Points[i]);
                float squared = (candidate - point).sqrMagnitude;
                if (squared >= bestSquared)
                    continue;

                best = candidate;
                bestSquared = squared;
            }

            return best;
        }

        // The horizontal side of the point relative to the run's nearest segment: +1 left, -1 right,
        // 0 when the run has no segment or the point sits exactly on it.
        public static int SideOf(TrajectoryResult result, Vector3 point)
        {
            if (!result.HasPoints || result.Points.Count < 2)
                return 0;

            int nearest = 1;
            float best = float.MaxValue;
            for (int i = 1; i < result.Points.Count; i++)
            {
                float distance = DistanceToSegment(point, result.Points[i - 1], result.Points[i]);
                if (distance >= best)
                    continue;

                best = distance;
                nearest = i;
            }

            Vector3 from = result.Points[nearest - 1];
            Vector3 to = result.Points[nearest];
            float side = Vector3.Dot(Vector3.Cross(to - from, point - from), Vector3.up);
            return side > 0f ? 1 : side < 0f ? -1 : 0;
        }
    }
}
