using System.Collections.Generic;
using DigitalLove.Game.Spatial;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public struct TrajectorySettings
    {
        public Vector3 Destination;
        public float CaptureRadius;
        // The ship's half-size: a run crashes once the hull, not just its centre line, meets a planet.
        public float ShipClearance;
        public float StepSecs;
        public int MaxSteps;
        public IReadOnlyList<SpaceZone> Zones;
    }
}
