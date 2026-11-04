using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzlePostClearPlacement
    {
        public static void Apply(
            PuzzlePlanetListAsset planets, PuzzleSeed seed, SpaceShapingSettings settings)
        {
            if (seed.mechanism != PuzzleMechanism.Mixed || planets == null)
                return;

            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            if (!PuzzleFieldBake.Apply(brief, field, planets, sim))
            {
                planets.postClear = default;
                Object.DestroyImmediate(field);
                return;
            }

            PuzzleBrief clear = brief;
            clear.Mechanism = PuzzleMechanism.Clear;
            if (!PuzzleSolutionSearch.TrySelect(field, sim, clear, out PuzzleSolution solution))
            {
                planets.postClear = default;
                Object.DestroyImmediate(field);
                return;
            }

            PuzzleZoneRun.Simulate(field, sim, solution.Zones, out _, out TrajectoryResult path);
            float clearWidth = solution.Zones.Length > 0
                ? solution.Zones[0].Width
                : 0.01f;
            if (!path.HasPoints)
            {
                planets.postClear = default;
                Object.DestroyImmediate(field);
                return;
            }

            MoveWell(planets, field, path, settings);
            if (!PuzzleFieldBake.Apply(brief, field, planets, sim))
            {
                planets.postClear = default;
                Object.DestroyImmediate(field);
                return;
            }

            planets.postClear = MakeBlockerFatal(
                planets, field, settings, sim, brief, clearWidth);
            Object.DestroyImmediate(field);
        }

        private static void MoveWell(
            PuzzlePlanetListAsset planets, PuzzleData field, TrajectoryResult path,
            SpaceShapingSettings settings)
        {
            int well = planets.wellIndex;
            if (well < 0 || well >= planets.planets.Length)
                return;

            int start = Mathf.Clamp(path.Points.Count * 3 / 5, 1, path.Points.Count - 1);
            for (int p = start; p < path.Points.Count; p++)
            {
                Vector3 point = path.Points[p];
                if (!PuzzleBlockerPlacement.Fits(
                        planets.planets, well, point, field.origin, field.destination, settings))
                    continue;

                planets.planets[well].position = point;
                return;
            }
        }

        private static PuzzlePostClearReport MakeBlockerFatal(
            PuzzlePlanetListAsset planets, PuzzleData field, SpaceShapingSettings settings,
            PuzzleSimSettings sim, PuzzleBrief brief, float clearWidth)
        {
            int blocker = planets.blockerIndex;
            PuzzlePostClearReport report = new()
            {
                Tried = true,
                BlockerIndex = blocker,
                ClearWidth = clearWidth
            };
            if (blocker < 0 || blocker >= planets.planets.Length)
                return report;

            report.BlockerRadius = planets.planets[blocker].radius;
            if (TrySweep(
                    planets, field, settings, sim, brief, blocker, clearWidth,
                    out PuzzlePostClearReport swept))
                return swept;

            return report;
        }

        private static bool TrySweep(
            PuzzlePlanetListAsset planets, PuzzleData field, SpaceShapingSettings settings,
            PuzzleSimSettings sim, PuzzleBrief brief, int blocker, float clearWidth,
            out PuzzlePostClearReport report)
        {
            report = default;
            PuzzleBrief curve = brief;
            curve.Mechanism = PuzzleMechanism.Curve;
            if (!PuzzleSolutionSearch.TrySelect(field, sim, curve, out PuzzleSolution solution))
                return false;

            PuzzleZoneRun.Simulate(field, sim, solution.Zones, out _, out TrajectoryResult path);
            if (!path.HasPoints)
                return false;

            Vector3 originalPosition = planets.planets[blocker].position;
            float originalRadius = planets.planets[blocker].radius;
            Vector3[] points = CandidatePoints(path, originalPosition);
            float[] scales = { 1.5f, 2f, 2.5f, 3f };
            for (int s = 0; s < scales.Length; s++)
            {
                float grown = Mathf.Min(originalRadius * scales[s], settings.planetRadiusMax);
                if (grown <= originalRadius + 0.001f)
                    continue;

                planets.planets[blocker].radius = grown;
                float band = grown * PuzzleRunChoice.PlanetBlockBandFactor;
                for (int p = 0; p < points.Length; p++)
                {
                    Vector3 point = points[p];
                    if (PuzzleRunGeometry.DistanceToSegment(
                            point, field.origin, field.destination) > band)
                        continue;

                    if (!PuzzleBlockerPlacement.Fits(
                            planets.planets, blocker, point,
                            field.origin, field.destination, settings))
                        continue;

                    planets.planets[blocker].position = point;
                    report = Verify(planets, sim, brief, blocker, clearWidth);
                    if (report.Kept)
                    {
                        report.Moved = point != originalPosition;
                        report.Grown = true;
                        return true;
                    }

                    planets.planets[blocker].position = originalPosition;
                }

                planets.planets[blocker].radius = originalRadius;
            }

            return false;
        }

        private static Vector3[] CandidatePoints(TrajectoryResult path, Vector3 original)
        {
            Vector3[] points = new Vector3[5];
            points[0] = original;
            int start = Mathf.Clamp(path.Points.Count * 2 / 5, 1, path.Points.Count - 1);
            int span = path.Points.Count - start;
            for (int i = 0; i < 4; i++)
            {
                int index = Mathf.Min(start + span * i / 4, path.Points.Count - 1);
                points[i + 1] = path.Points[index];
            }

            return points;
        }

        private static PuzzlePostClearReport Verify(
            PuzzlePlanetListAsset planets, PuzzleSimSettings sim, PuzzleBrief brief,
            int blocker, float clearWidth)
        {
            PuzzlePostClearReport report = new()
            {
                Tried = true,
                BlockerIndex = blocker,
                BlockerRadius = planets.planets[blocker].radius,
                ClearWidth = clearWidth
            };

            PuzzleBakeResult mixed = PuzzleFieldBake.Bake(planets, sim, brief);
            report.MixedBaked = mixed.Baked;
            report.MixedKind = mixed.Kind;
            report.MixedZones = mixed.Solution != null ? mixed.Solution.Length : 0;
            Destroy(mixed.Puzzle);

            PuzzleBrief curve = brief;
            curve.Mechanism = PuzzleMechanism.Curve;
            PuzzleBakeResult curved = PuzzleFieldBake.Bake(planets, sim, curve);
            report.CurveBaked = curved.Baked;
            Destroy(curved.Puzzle);

            PuzzleBrief clear = brief;
            clear.Mechanism = PuzzleMechanism.Clear;
            PuzzleBakeResult cleared = PuzzleFieldBake.Bake(planets, sim, clear);
            report.ClearBaked = cleared.Baked;
            Destroy(cleared.Puzzle);

            report.Kept = report.MixedBaked
                && report.MixedKind == PuzzleSolutionKind.Mixed
                && report.MixedZones >= 2
                && !report.CurveBaked
                && !report.ClearBaked;
            return report;
        }

        private static void Destroy(PuzzleData puzzle)
        {
            if (puzzle != null)
                Object.DestroyImmediate(puzzle);
        }
    }
}
