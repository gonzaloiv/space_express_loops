using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One puzzle attempt: the session, its reusable trajectory buffers, and the latest result state.
    public sealed class PuzzleRun
    {
        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private List<Vector3>[] buffers;

        public PuzzleRun(SpaceShapingSetup setup) => Session = new SpaceShapingSession(setup);

        public SpaceShapingSession Session { get; }

        public TrajectoryResult[] LastResults { get; private set; }

        public bool IsChecked { get; private set; }

        public bool IsFlying { get; private set; }

        public void Reset()
        {
            Session.Reset();
            LastResults = null;
            IsChecked = false;
            IsFlying = false;
        }

        public void PreviewZone(SpaceZone zone) => Session.PreviewZone(zone);

        public void CommitPreview() => Session.CommitPreview();

        public TrajectoryResult[] Simulate(bool flying)
        {
            LastResults = SimulatePreview();
            IsChecked = false;
            IsFlying = flying;
            return LastResults;
        }

        // A preview keeps the checked/flight flags alone; only the path is refreshed.
        public TrajectoryResult[] SimulatePreview()
        {
            LastResults = Session.SimulateAll(
                Session.BuildField(), integrator, launch, Buffers());
            return LastResults;
        }

        public void Check()
        {
            IsChecked = true;
            IsFlying = false;
        }

        public PuzzleReadout Readout() => LastResults == null
            ? PuzzleReadout.Pending(Session)
            : PuzzleReadout.From(Session, LastResults, IsChecked, IsFlying);

        private List<Vector3>[] Buffers()
        {
            if (buffers != null && buffers.Length == Session.ShipCount)
                return buffers;

            buffers = new List<Vector3>[Session.ShipCount];
            for (int i = 0; i < buffers.Length; i++)
                buffers[i] = new List<Vector3>(Session.MaxSteps + 1);

            return buffers;
        }
    }
}
