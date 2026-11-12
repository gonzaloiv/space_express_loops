using DigitalLove.Game.Spatial;

namespace DigitalLove.Game.SpaceShaping
{
    public static class TrajectoryOutcomeExtensions
    {
        // The cube's check lamp for a run: a solve wins; otherwise a crash or a miss.
        public static ZoneVerdict ToVerdict(this TrajectoryOutcome outcome, bool solved)
        {
            if (solved)
                return ZoneVerdict.Solved;

            return outcome == TrajectoryOutcome.Crash ? ZoneVerdict.Crash : ZoneVerdict.Miss;
        }
    }
}
