using Unity.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // How visible the lattice is at a point: opaque near the run and the planets, fading out with
    // distance. The planets are native spheres (xyz centre, w radius) so the Burst lattice job runs it.
    public readonly struct ContentFade
    {
        private readonly Vector3 origin;
        private readonly Vector3 destination;
        private readonly NativeArray<Vector4>.ReadOnly spheres;
        private readonly float near;
        private readonly float far;

        public ContentFade(
            Vector3 origin, Vector3 destination, NativeArray<Vector4>.ReadOnly spheres,
            float near, float far)
        {
            this.origin = origin;
            this.destination = destination;
            this.spheres = spheres;
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
            float best = SegmentGeometry.DistanceToSegment(point, origin, destination);
            for (int i = 0; i < spheres.Length; i++)
            {
                Vector4 sphere = spheres[i];
                Vector3 centre = sphere;
                float radius = sphere.w;
                float limit = best + radius;
                float sqr = (point - centre).sqrMagnitude;
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
