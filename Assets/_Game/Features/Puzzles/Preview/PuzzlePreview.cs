using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Throttles the live preview while the cube scales: lattice, readout and path at their own rates.
    public sealed class PuzzlePreview
    {
        private readonly PreviewClock clock = new();

        public void Reset() => clock.Reset();

        public void Render(PuzzleRun run, SpaceShapingView view)
        {
            if (clock.TakeLattice(Time.unscaledTime))
                Apply(run, view);

            if (clock.TakeReadout(Time.unscaledTime))
                view.ShowReadout(PuzzleReadout.Pending(run.Session));

            if (clock.TakePreview(Time.unscaledTime))
                view.ShowTrajectories(run.Session, run.SimulatePreview(), true);
        }

        public static void Apply(PuzzleRun run, SpaceShapingView view)
        {
            view.ApplyFromSession(run.Session);
            view.Render(run.Session);
        }
    }
}
