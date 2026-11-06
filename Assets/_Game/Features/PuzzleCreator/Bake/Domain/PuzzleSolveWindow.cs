using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // How forgiving a baked solve is. The first zone's scale sweeps the player's whole range with the
    // other zones held at the solution; the window is the run of arriving scales around the solution,
    // as a share of the range. A wide window is a forgiving puzzle; a thin one is a knife-edge.
    public static class PuzzleSolveWindow
    {
        public const int DefaultSamples = 200;
        private const int SnapSamples = 2;

        public static float Measure(
            PuzzleData puzzle, PuzzleZone[] solution, PuzzleSimSettings sim,
            int samples = DefaultSamples)
        {
            if (puzzle == null || solution == null || solution.Length == 0 || samples < 2)
                return 0f;

            if (sim.MaxScaleFactor <= sim.MinScaleFactor)
                return 0f;

            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, sim);
            bool[] arrives = Sweep(setup, solution, sim, samples);
            int run = RunAround(arrives, SampleOf(solution[0].scale, sim, samples));
            return run < 2 ? 0f : (run - 1) / (float)(samples - 1);
        }

        private static bool[] Sweep(
            SpaceShapingSetup setup, PuzzleZone[] solution, PuzzleSimSettings sim, int samples)
        {
            bool[] arrives = new bool[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)(samples - 1);
                arrives[i] = Arrives(
                    setup, solution, Mathf.Lerp(sim.MinScaleFactor, sim.MaxScaleFactor, t));
            }

            return arrives;
        }

        private static bool Arrives(SpaceShapingSetup setup, PuzzleZone[] solution, float scale)
        {
            SpaceShapingSession session = new(setup);
            for (int i = 0; i < solution.Length; i++)
            {
                PuzzleZone zone = solution[i];
                if (i == 0)
                    zone.scale = scale;
                session.ApplyZone(zone.ToSpaceZone());
            }

            return session.SimulateAll().AllArrived();
        }

        private static int SampleOf(float scale, PuzzleSimSettings sim, int samples)
        {
            float t = Mathf.InverseLerp(sim.MinScaleFactor, sim.MaxScaleFactor, scale);
            return Mathf.Clamp(Mathf.RoundToInt(t * (samples - 1)), 0, samples - 1);
        }

        // The sampled solution can fall just outside a thin window; snap to the nearest arriving
        // sample within a couple of steps before measuring the run.
        private static int RunAround(bool[] arrives, int centre)
        {
            int start = NearestArriving(arrives, centre);
            if (start < 0)
                return 0;

            int low = start;
            while (low > 0 && arrives[low - 1])
                low--;

            int high = start;
            while (high < arrives.Length - 1 && arrives[high + 1])
                high++;

            return high - low + 1;
        }

        private static int NearestArriving(bool[] arrives, int centre)
        {
            for (int offset = 0; offset <= SnapSamples; offset++)
            {
                if (centre - offset >= 0 && arrives[centre - offset])
                    return centre - offset;

                if (centre + offset < arrives.Length && arrives[centre + offset])
                    return centre + offset;
            }

            return -1;
        }
    }
}
