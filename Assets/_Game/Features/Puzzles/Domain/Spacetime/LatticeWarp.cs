using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Draws the zones exactly as the physics applies them: each zone transforms the point with the same
    // profile SpaceZone.ApplyTo uses on the planets, in the same order, so the lattice never shows a warp
    // the ship does not fly through.
    public readonly struct LatticeWarp
    {
        private readonly IReadOnlyList<SpaceZone> zones;

        public LatticeWarp(IReadOnlyList<SpaceZone> zones)
        {
            this.zones = zones;
        }

        public Vector3 Warp(Vector3 node)
        {
            Vector3 warped = node;
            if (zones == null)
                return warped;

            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].IsActive)
                    warped = zones[i].Transform(warped);
            }

            return warped;
        }
    }
}
