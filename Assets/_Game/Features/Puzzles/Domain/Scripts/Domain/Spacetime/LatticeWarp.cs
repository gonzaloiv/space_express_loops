using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public readonly struct LatticeWarp
    {
        private const float ZoneShells = 3f;

        private readonly IReadOnlyList<SpaceZone> zones;

        public LatticeWarp(IReadOnlyList<SpaceZone> zones)
        {
            this.zones = zones;
        }

        public Vector3 Warp(Vector3 node)
        {
            Vector3 warped = node;
            if (zones != null)
            {
                for (int i = 0; i < zones.Count; i++)
                {
                    SpaceZone zone = zones[i];
                    if (!zone.IsActive)
                        continue;

                    float inside = VisualInsideFactor(zone, node);
                    warped = Vector3.Lerp(
                        warped, zone.Centre + (warped - zone.Centre) * zone.Scale, inside);
                }
            }

            return warped;
        }

        private static float VisualInsideFactor(SpaceZone zone, Vector3 node)
        {
            float edge = Mathf.Max(zone.Falloff * zone.Extent, 0.0001f);
            float outside = edge * Mathf.Max(ZoneShells, 0f);
            float t = Mathf.Clamp01((zone.Margin(node) + outside) / (edge + outside));
            return t * t * (3f - 2f * t);
        }
    }
}
