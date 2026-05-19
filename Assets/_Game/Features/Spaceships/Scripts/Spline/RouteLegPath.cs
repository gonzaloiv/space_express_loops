using DigitalLove.Game.Planets;
using UnityEngine;

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
}
