using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.EditorTools
{
    internal static class PuzzleDebugSolution
    {
        private const float Step = 0.001f;

        public static float Find(SpaceShapingSetup setup, SpaceZone zone)
        {
            SpaceShapingSession scratch = new(setup);
            TrajectoryIntegrator integrator = new();
            ShipLaunch launch = new();
            float bestScale = 0f;
            float bestCost = float.MaxValue;
            for (float scale = setup.Sim.MinScaleFactor;
                scale <= setup.Sim.MaxScaleFactor + Step;
                scale += Step)
            {
                scratch.Reset();
                scratch.ApplyZone(new SpaceZone(zone.Centre, zone.HalfExtents, scale, zone.Falloff));
                TrajectoryResult result = scratch.Simulate(scratch.BuildField(), integrator, launch);
                if (!scratch.IsSolved(result) || scratch.IsOverBudget)
                    continue;

                if (scratch.EntropyCommitted >= bestCost)
                    continue;

                bestCost = scratch.EntropyCommitted;
                bestScale = scale;
            }

            return bestScale;
        }
    }
}
