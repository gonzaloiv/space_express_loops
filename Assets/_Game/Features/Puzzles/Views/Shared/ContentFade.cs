using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public readonly struct ContentFade
    {
        private readonly SpaceShapingSession session;
        private readonly float near;
        private readonly float far;

        public ContentFade(SpaceShapingSession session, float near, float far)
        {
            this.session = session;
            this.near = near;
            this.far = far;
        }

        public float Alpha(Vector3 point)
        {
            float distance = Distance(point);
            if (far <= near)
                return distance <= near ? 1f : 0f;

            float t = Mathf.Clamp01((distance - near) / (far - near));
            return 1f - Mathf.SmoothStep(0f, 1f, t);
        }

        private float Distance(Vector3 point)
        {
            float best = SegmentGeometry.DistanceToSegment(
                point, session.Origin, session.Destination);
            for (int i = 0; i < session.SourceCount; i++)
            {
                float radius = session.GetScale(i) * 0.5f;
                float limit = best + radius;
                float sqr = (point - session.GetPosition(i)).sqrMagnitude;
                if (limit >= 0f && sqr >= limit * limit)
                    continue;

                float surface = Mathf.Sqrt(sqr) - radius;
                if (surface < best)
                    best = surface;
            }

            return best;
        }
    }
}
