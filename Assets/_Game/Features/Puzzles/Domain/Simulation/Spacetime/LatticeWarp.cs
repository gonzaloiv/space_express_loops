using DigitalLove.Game.Spatial;
using Unity.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Draws the zones exactly as the physics applies them: each zone transforms the point with the same
    // profile SpaceZone.ApplyTo uses on the planets, in the same order, so the zone warp matches what
    // the ship flies through. The wells are drawn after the zones, so a grown planet's well deepens
    // with the pinch; they are a picture of the gravity, not a warp the ship follows (a pull with no
    // sources draws none). The zones are native so the Burst lattice job runs this same code.
    public readonly struct LatticeWarp
    {
        private readonly NativeArray<SpaceZone>.ReadOnly zones;
        private readonly WellPull wells;

        public LatticeWarp(NativeArray<SpaceZone>.ReadOnly zones, WellPull wells)
        {
            this.zones = zones;
            this.wells = wells;
        }

        public Vector3 Warp(Vector3 node) => wells.Pull(ApplyZones(node));

        private Vector3 ApplyZones(Vector3 node)
        {
            Vector3 warped = node;
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i].IsActive)
                    warped = zones[i].Transform(warped);
            }

            return warped;
        }
    }
}
