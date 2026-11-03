using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Trims a simulated path to the clear caps at each end: the launch planet's surface plus a gap
    // and the destination planet's surface plus the same gap. The crossings are interpolated, so a
    // coarse step never leaves a drawn run touching or inside a planet.
    public static class TrajectoryCaps
    {
        public static Vector3 LaunchPoint(
            Vector3 origin, Vector3 destination, float radius, float gap)
        {
            Vector3 direction = Direction(origin, destination);
            return origin + direction * (radius + gap);
        }

        public static Vector3 ArrivalPoint(
            Vector3 origin, Vector3 destination, float radius, float gap)
        {
            Vector3 direction = Direction(origin, destination);
            return destination - direction * (radius + gap);
        }

        public static TrajectoryResult Clip(
            TrajectoryResult result, Vector3 origin, float launchRadius,
            Vector3 destination, float arrivalRadius)
        {
            if (!result.HasPoints)
                return result;

            List<Vector3> points = result.Points;
            int start = FirstOutside(points, origin, launchRadius);
            int end = LastOutside(points, destination, arrivalRadius);
            if (end < start || end < 0)
                return new TrajectoryResult(result.Outcome, new List<Vector3>());

            List<Vector3> clipped = new(end - start + 3);
            if (start > 0)
                clipped.Add(Crossing(points[start - 1], points[start], origin, launchRadius));
            for (int i = start; i <= end; i++)
                clipped.Add(points[i]);
            if (end < points.Count - 1)
                clipped.Add(Crossing(points[end + 1], points[end], destination, arrivalRadius));

            return new TrajectoryResult(result.Outcome, clipped);
        }

        public static TrajectoryResult ClipToBox(
            TrajectoryResult result, Vector3 centre, float half)
        {
            if (!result.HasPoints)
                return result;

            List<Vector3> points = result.Points;
            int lastInside = LastInside(points, centre, half);
            if (lastInside == points.Count - 1)
                return result;

            if (lastInside < 0)
                return new TrajectoryResult(result.Outcome, new List<Vector3>());

            List<Vector3> clipped = new(lastInside + 2);
            for (int i = 0; i <= lastInside; i++)
                clipped.Add(points[i]);
            clipped.Add(BoxCrossing(points[lastInside], points[lastInside + 1], centre, half));

            return new TrajectoryResult(result.Outcome, clipped);
        }

        private static int LastInside(List<Vector3> points, Vector3 centre, float half)
        {
            int last = -1;
            for (int i = 0; i < points.Count; i++)
            {
                if (!Inside(points[i], centre, half))
                    break;

                last = i;
            }

            return last;
        }

        private static bool Inside(Vector3 point, Vector3 centre, float half) =>
            Mathf.Abs(point.x - centre.x) <= half
            && Mathf.Abs(point.y - centre.y) <= half
            && Mathf.Abs(point.z - centre.z) <= half;

        private static Vector3 BoxCrossing(
            Vector3 inside, Vector3 outside, Vector3 centre, float half)
        {
            Vector3 segment = outside - inside;
            float t = 1f;
            if (Mathf.Abs(segment.x) > 0.0000001f)
                t = Mathf.Min(t, ((segment.x > 0f ? centre.x + half : centre.x - half) - inside.x)
                    / segment.x);
            if (Mathf.Abs(segment.y) > 0.0000001f)
                t = Mathf.Min(t, ((segment.y > 0f ? centre.y + half : centre.y - half) - inside.y)
                    / segment.y);
            if (Mathf.Abs(segment.z) > 0.0000001f)
                t = Mathf.Min(t, ((segment.z > 0f ? centre.z + half : centre.z - half) - inside.z)
                    / segment.z);

            return inside + segment * Mathf.Clamp01(t);
        }

        private static Vector3 Direction(Vector3 origin, Vector3 destination)
        {
            Vector3 direction = destination - origin;
            return direction.sqrMagnitude > 0.000001f ? direction.normalized : Vector3.forward;
        }

        private static int FirstOutside(List<Vector3> points, Vector3 centre, float radius)
        {
            float radiusSq = radius * radius;
            for (int i = 0; i < points.Count; i++)
            {
                if ((points[i] - centre).sqrMagnitude >= radiusSq)
                    return i;
            }

            return points.Count;
        }

        private static int LastOutside(List<Vector3> points, Vector3 centre, float radius)
        {
            float radiusSq = radius * radius;
            for (int i = points.Count - 1; i >= 0; i--)
            {
                if ((points[i] - centre).sqrMagnitude >= radiusSq)
                    return i;
            }

            return -1;
        }

        // The point where a segment from inside a sphere to outside it crosses the surface.
        private static Vector3 Crossing(
            Vector3 inside, Vector3 outside, Vector3 centre, float radius)
        {
            Vector3 segment = outside - inside;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.0000001f)
                return outside;

            Vector3 offset = inside - centre;
            float b = 2f * Vector3.Dot(offset, segment);
            float c = Vector3.Dot(offset, offset) - radius * radius;
            float discriminant = b * b - 4f * lengthSq * c;
            if (discriminant <= 0f)
                return outside;

            float t = (-b + Mathf.Sqrt(discriminant)) / (2f * lengthSq);
            return inside + segment * Mathf.Clamp01(t);
        }
    }
}
