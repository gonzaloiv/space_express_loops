using System.Collections.Generic;

namespace DigitalLove.Game.SpaceShaping
{
    public static class TrajectoryResults
    {
        // A multi-ship level is solved only when every run arrives.
        public static bool AllArrived(this IReadOnlyList<TrajectoryResult> results)
        {
            if (results == null || results.Count == 0)
                return false;

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Outcome != TrajectoryOutcome.Arrive)
                    return false;
            }

            return true;
        }

        // The outcome the field is judged by: crash beats miss beats arrival.
        public static TrajectoryOutcome Worst(this IReadOnlyList<TrajectoryResult> results)
        {
            TrajectoryOutcome outcome = TrajectoryOutcome.Arrive;
            if (results == null)
                return outcome;

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Outcome == TrajectoryOutcome.Crash)
                    return TrajectoryOutcome.Crash;

                if (results[i].Outcome != TrajectoryOutcome.Arrive)
                    outcome = results[i].Outcome;
            }

            return outcome;
        }
    }
}
