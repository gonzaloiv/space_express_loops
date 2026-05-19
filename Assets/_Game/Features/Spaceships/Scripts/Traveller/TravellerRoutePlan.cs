using System.Collections.Generic;
using DigitalLove.Game.Planets;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class TravellerRoutePlan
    {
        public IReadOnlyList<Vector3[]> Segments { get; }
        public IReadOnlyList<PlanetBehaviour> PickupPlanets { get; }

        public TravellerRoutePlan(IReadOnlyList<Vector3[]> segments, IReadOnlyList<PlanetBehaviour> pickupPlanets)
        {
            Segments = segments;
            PickupPlanets = pickupPlanets;
        }
    }
}
