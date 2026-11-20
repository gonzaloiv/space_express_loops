using Unity.Profiling;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Throttles the live preview while the cube scales: lattice, readout and path at their own rates.
    public sealed class PuzzlePreview
    {
        private static readonly ProfilerMarker LatticeMarker = new("Shaping.Preview.Lattice");
        private static readonly ProfilerMarker ReadoutMarker = new("Shaping.Preview.Readout");
        private static readonly ProfilerMarker PathMarker = new("Shaping.Preview.Path");
        private static readonly ProfilerMarker LinesMarker = new("Shaping.Preview.Lines");

        private readonly PreviewClock clock = new();

        public void Reset() => clock.Reset();

        public void Render(PuzzleRun run, SpaceShapingView view)
        {
            if (clock.TakeLattice(Time.unscaledTime))
            {
                using (LatticeMarker.Auto())
                    Apply(run, view);
            }

            if (clock.TakeReadout(Time.unscaledTime))
            {
                using (ReadoutMarker.Auto())
                    view.ShowReadout(PuzzleReadout.Pending(run.Session));
            }

            if (clock.TakePreview(Time.unscaledTime))
                RenderPath(run, view);
        }

        public static void Apply(PuzzleRun run, SpaceShapingView view)
        {
            view.ApplyFromSession(run.Session);
            view.Render(run.Session);
        }

        private static void RenderPath(PuzzleRun run, SpaceShapingView view)
        {
            using (PathMarker.Auto())
            {
                TrajectoryResult[] results = run.SimulatePreview();
                using (LinesMarker.Auto())
                    view.ShowTrajectories(run.Session, results, true);
            }
        }
    }
}
