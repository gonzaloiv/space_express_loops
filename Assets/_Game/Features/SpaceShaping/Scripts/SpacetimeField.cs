using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpacetimeField
    {
        private readonly List<SpaceSource> sources = new();

        public IReadOnlyList<SpaceSource> Sources => sources;

        public void Clear() => sources.Clear();

        public void Add(SpaceSource source) => sources.Add(source);

        public float Potential(Vector3 position)
        {
            float potential = 0f;
            for (int i = 0; i < sources.Count; i++)
            {
                SpaceSource source = sources[i];
                float distance = Mathf.Max(Vector3.Distance(position, source.Position), source.Radius);
                potential -= source.Strength / distance;
            }

            return potential;
        }

        public Vector3 Acceleration(Vector3 position)
        {
            Vector3 acceleration = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                SpaceSource source = sources[i];
                Vector3 offset = source.Position - position;
                float distance = Mathf.Max(offset.magnitude, source.Radius);
                acceleration += offset * (source.Strength / (distance * distance * distance));
            }

            return acceleration;
        }

        public Vector3 Pull(Vector3 position)
        {
            Vector3 pull = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                SpaceSource source = sources[i];
                Vector3 offset = source.Position - position;
                float distance = Mathf.Max(offset.magnitude, source.Radius);
                pull += offset * (source.Strength / (distance * distance));
            }

            return pull;
        }
    }
}
