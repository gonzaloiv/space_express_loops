using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public static class SpaceZoneExtensions
    {
        // The strongest deviation from neutral scale, signed; drives the lattice tint and warp.
        public static float StrongestScaleOffset(this IReadOnlyList<SpaceZone> zones)
        {
            float strongest = 1f;
            for (int i = 0; i < zones.Count; i++)
            {
                if (Mathf.Abs(zones[i].Scale - 1f) > Mathf.Abs(strongest - 1f))
                    strongest = zones[i].Scale;
            }

            return strongest - 1f;
        }
    }
}
