using DigitalLove.Game.Planets;
using UnityEngine;
using System.Collections.Generic;

namespace DigitalLove.Game.Spaceships
{
    public readonly struct RouteLeg
    {
        public Vector3[] positions { get; }
        public PlanetBehaviour pickupPlanet { get; }

        public RouteLeg(List<Vector3> positions, PlanetBehaviour pickupPlanet)
        {
            this.positions = positions.ToArray();
            this.pickupPlanet = pickupPlanet;
        }
    }
}
