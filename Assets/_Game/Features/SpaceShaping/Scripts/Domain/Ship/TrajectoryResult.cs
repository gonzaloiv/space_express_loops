using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public struct TrajectoryResult
    {
        public TrajectoryOutcome Outcome;
        public List<Vector3> Points;
        public float ShipTime;

        public TrajectoryResult(TrajectoryOutcome outcome, List<Vector3> points, float shipTime)
        {
            Outcome = outcome;
            Points = points;
            ShipTime = shipTime;
        }
    }
}
