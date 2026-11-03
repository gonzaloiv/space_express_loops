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
            return 1f - t * t * (3f - 2f * t);
        }

        private float Distance(Vector3 point)
        {
            float best = SegmentDistance(point, session.Origin, session.Destination);
            for (int i = 0; i < session.SourceCount; i++)
            {
                float surface = Vector3.Distance(point, session.GetPosition(i)) - session.GetScale(i) * 0.5f;
                best = Mathf.Min(best, surface);
            }

            return best;
        }

        private static float SegmentDistance(Vector3 point, Vector3 from, Vector3 to)
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
