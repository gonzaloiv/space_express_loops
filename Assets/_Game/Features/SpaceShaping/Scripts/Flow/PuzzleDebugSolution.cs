namespace DigitalLove.Game.SpaceShaping
{
    internal static class PuzzleDebugSolution
    {
        public static float Find(SpaceShapingSetup setup, SpaceZone zone, float minScale, float maxScale)
        {
            const float step = 0.001f;
            SpaceShapingSession scratch = new(setup);
            TrajectoryIntegrator integrator = new();
            ShipLaunch launch = new();
            float bestScale = 0f;
            float bestCost = float.MaxValue;
            for (float scale = minScale; scale <= maxScale + step; scale += step)
            {
                scratch.Reset();
                scratch.ApplyZone(new SpaceZone(zone.Centre, zone.HalfExtents, scale, zone.Falloff));
                TrajectoryResult result = scratch.Simulate(scratch.BuildField(), integrator, launch);
                if (!scratch.IsSolved(result) || scratch.IsOverBudget)
                    continue;
                float cost = scratch.EntropyCommitted;
                if (cost >= bestCost)
                    continue;
                bestCost = cost;
                bestScale = scale;
            }

            return bestScale;
        }
    }
}
