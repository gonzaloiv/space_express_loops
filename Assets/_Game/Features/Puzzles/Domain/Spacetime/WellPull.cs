using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Draws the field's wells on the lattice: every point is pulled toward each source, the way the
    // ship is. The direction is the physics'; the depth is compressed (√strength over a distance
    // softened by the source's radius) so the sun does not swamp the planets. A pull never reaches a
    // source's surface, and a point already inside a source stays put.
    public readonly struct WellPull
    {
        private readonly IReadOnlyList<SpaceSource> sources;
        private readonly float gain;
        private readonly float maxFraction;

        public WellPull(IReadOnlyList<SpaceSource> sources, float gain, float maxFraction)
        {
            this.sources = sources;
            this.gain = Mathf.Max(gain, 0f);
            this.maxFraction = Mathf.Clamp01(maxFraction);
        }

        public Vector3 Pull(Vector3 point)
        {
            if (sources == null)
                return point;

            Vector3 shift = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
                shift += Toward(sources[i], point);

            return point + shift;
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
