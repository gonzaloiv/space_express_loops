using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public struct TrajectorySettings
    {
        public Vector3 Destination;
        public float CaptureRadius;
        public float StepSecs;
        public int MaxSteps;
        public float LightSpeed;
        public IReadOnlyList<SpaceZone> Zones;
    }
}
