using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public struct TrajectoryResult
    {
        public TrajectoryOutcome Outcome;
        public List<Vector3> Points;

        public TrajectoryResult(TrajectoryOutcome outcome, List<Vector3> points)
        {
            Outcome = outcome;
            Points = points;
        }

        public bool HasPoints => Points != null && Points.Count > 0;

        public float ClosestApproach(Vector3 point)
        {
            if (!HasPoints)
                return 0f;

            float closest = float.MaxValue;
            for (int i = 0; i < Points.Count; i++)
                closest = Mathf.Min(closest, Vector3.Distance(Points[i], point));

            return closest;
        }
    }
}
