using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Draws the field's wells on the lattice: every point is pulled toward each source, the way the
    // ship is. The direction is the physics'; the depth is compressed (√strength over a distance
    // softened by the source's radius) so the sun does not swamp the planets. A pull never reaches a
    // source's surface, and a point already inside any source stays put.
    public readonly struct WellPull
    {
        // A cap of 1 would let a node land on the surface; the cap stays just short of it.
        public const float MaxFractionCap = 0.95f;

        private readonly IReadOnlyList<SpaceSource> sources;
        private readonly float gain;
        private readonly float maxFraction;

        public WellPull(IReadOnlyList<SpaceSource> sources, float gain, float maxFraction)
        {
            this.sources = sources;
            this.gain = Mathf.Max(gain, 0f);
            this.maxFraction = Mathf.Clamp(maxFraction, 0f, MaxFractionCap);
        }

        public Vector3 Pull(Vector3 point)
        {
            if (sources == null || InsideAny(point))
                return point;

            Vector3 shift = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
                shift += Toward(sources[i], point);

            return point + shift * Reach(point, shift);
        }

        private bool InsideAny(Vector3 point)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                if ((point - sources[i].Position).sqrMagnitude <= sources[i].Radius * sources[i].Radius)
                    return true;
            }

            return false;
        }

        // How much of the summed shift the point may take: each pull is capped alone, but the sum of
        // a far, strong source and a near one can still carry a point through the near one. The move
        // stops where it would come closer to any source than its own cap allows.
        private float Reach(Vector3 point, Vector3 shift)
        {
            float a = shift.sqrMagnitude;
            if (a <= 0f)
                return 1f;

            float reach = 1f;
            for (int i = 0; i < sources.Count; i++)
                reach = Mathf.Min(reach, ReachPast(sources[i], point, shift, a));

            return reach;
        }

        // The first fraction of the move at which the point touches the source's keep-away sphere
        // (radius plus the gap the cap leaves), or 1 if it never does.
        private float ReachPast(SpaceSource source, Vector3 point, Vector3 shift, float a)
        {
            Vector3 fromSource = point - source.Position;
            float gap = fromSource.magnitude - source.Radius;
            if (gap <= 0f)
                return 1f;

            float keep = source.Radius + (1f - maxFraction) * gap;
            float b = 2f * Vector3.Dot(fromSource, shift);
            float c = fromSource.sqrMagnitude - keep * keep;
            float discriminant = b * b - 4f * a * c;
            if (b >= 0f || discriminant < 0f)
                return 1f;

            float first = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
            return first >= 0f && first < 1f ? first : 1f;
        }

        private Vector3 Toward(SpaceSource source, Vector3 point)
        {
            Vector3 offset = source.Position - point;
            float distance = offset.magnitude;
            float gap = distance - source.Radius;
            if (gap <= 0f || source.Strength <= 0f)
                return Vector3.zero;

            float softened = Mathf.Sqrt(distance * distance + source.Radius * source.Radius);
            float depth = gain * Mathf.Sqrt(source.Strength) / softened;
            return offset * (Mathf.Min(depth, maxFraction * gap) / distance);
        }
    }
}
