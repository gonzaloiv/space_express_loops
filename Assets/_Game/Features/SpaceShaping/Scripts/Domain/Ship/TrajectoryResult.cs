using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public struct TrajectoryResult
    {
        public TrajectoryOutcome Outcome;
        public List<Vector3> Points;
        public List<float> Times;
        public float ShipTime;

        public TrajectoryResult(
            TrajectoryOutcome outcome, List<Vector3> points, List<float> times, float shipTime)
        {
            Outcome = outcome;
            Points = points;
            Times = times;
            ShipTime = shipTime;
        }

        public bool HasPoints => Points != null && Points.Count > 0;

        public float SampleTime(float index)
        {
            int last = Times.Count - 1;
            int lower = Mathf.Clamp(Mathf.FloorToInt(index), 0, last);
            int upper = Mathf.Min(lower + 1, last);
            return Mathf.Lerp(Times[lower], Times[upper], index - lower);
        }

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
