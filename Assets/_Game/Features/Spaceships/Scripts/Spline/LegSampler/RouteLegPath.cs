using DigitalLove.Game.Planets;
using UnityEngine;
using System.Collections.Generic;

namespace DigitalLove.Game.Spaceships
{
    /// <summary>
    /// One traveller leg and its line-renderer samples. PickupPlanet is set only on outbound stops.
    /// </summary>
    public readonly struct RouteLegPath
    {
        public Vector3[] Positions { get; }
        public PlanetBehaviour PickupPlanet { get; }

        public RouteLegPath(Vector3[] positions, PlanetBehaviour pickupPlanet)
        {
            Positions = positions;
            PickupPlanet = pickupPlanet;
        }
    }

    public static class RouteLegPathExtensions
    {
        public static Vector3[] Merge(this IReadOnlyList<RouteLegPath> legs)
        {
            List<Vector3> merged = new();
            foreach (RouteLegPath leg in legs)
                AppendPath(merged, leg.Positions);

            return merged.ToArray();
        }

        private static void AppendPath(List<Vector3> merged, Vector3[] segment)
        {
            if (segment == null || segment.Length == 0)
                return;

            int startIndex = merged.Count > 0 ? 1 : 0;
            for (int i = startIndex; i < segment.Length; i++)
                merged.Add(segment[i]);
        }
    }
}
