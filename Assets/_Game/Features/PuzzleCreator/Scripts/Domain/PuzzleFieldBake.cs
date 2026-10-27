using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        private const float StarBudgetFactor = 3f;
        private const float BareMissMargin = 0.9f;
        private const float MinRunFraction = 0.5f;
        private const float MaxRunFactor = 1.5f;

        public static void Apply(
            PuzzleBrief brief, PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleSimSettings settings)
        {
            puzzle.sources = PuzzleSourceBridge.ToSources(planets.planets);
            puzzle.cubeSizeMin = brief.MinCubeSize;
            puzzle.cubeSizeMax = brief.MaxCubeSize;
            puzzle.translatable = brief.Translatable;
            puzzle.moves = brief.Moves;
            puzzle.resizable = brief.Resizable;
            puzzle.rotate = brief.Rotate;
            if (!TryPickRun(
                planets.planets, settings.RunDistance, brief.CorridorClearance,
                out Vector3 origin, out Vector3 destination))
                return;

            puzzle.origin = origin;
            puzzle.destination = destination;
            puzzle.captureRadius = CaptureRadius(puzzle.sources, destination);
            puzzle.zoneCentre = (origin + destination) * 0.5f;
            puzzle.launchSpeed = LaunchSpeed(settings);
        }

        public static PuzzleData Bake(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief,
            out PuzzleBakeStatus status, out PuzzleZone[] solution)
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            Apply(brief, field, planets, settings);
            PuzzleData baked = BakeField(field, settings, brief, out status, out solution);
            Object.DestroyImmediate(field);
            return baked;
        }

        private static PuzzleData BakeField(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief,
            out PuzzleBakeStatus status, out PuzzleZone[] solution)
        {
            solution = null;
            status = PuzzleBakeStatus.UnsupportedBrief;
            if (!brief.SupportsSingleZone)
                return null;

            status = PuzzleBakeStatus.NoRun;
            if (!DeriveWindow(field, settings, brief.WindowTightness))
                return null;

            status = PuzzleBakeStatus.StartNotFailed;
            if (!StartFails(field, settings))
                return null;

            status = PuzzleBakeStatus.NoSolution;
            if (!PuzzleSolutionSearch.TrySelect(field, settings, brief, out PuzzleSolution found))
                return null;

            solution = found.Zones;
            return Verified(field, found, brief, settings, out status);
        }

        private static PuzzleData Verified(
            PuzzleData field, PuzzleSolution solution, PuzzleBrief brief, PuzzleSimSettings settings,
            out PuzzleBakeStatus status)
        {
            field.entropyBudget = solution.EntropySpent * StarBudgetFactor;
            PuzzleData variant = Variant(field, solution, brief);
            if (PuzzleVerifier.Verify(variant, solution.Zones, settings).Solved)
            {
                status = PuzzleBakeStatus.Baked;
                return variant;
            }

            Object.DestroyImmediate(variant);
            status = PuzzleBakeStatus.Unverified;
            return null;
        }

        private static bool DeriveWindow(PuzzleData field, PuzzleSimSettings settings, float tightness)
        {
            if (settings.NominalSpeed <= 0f)
                return false;

            float cross = field.Distance / settings.NominalSpeed;
            float fraction = Mathf.Clamp(tightness, 0f, 1f);
            field.tmin = cross * (1f - fraction);
            field.tmax = cross * (1f + fraction);
            return true;
        }

        private static PuzzleData Variant(PuzzleData field, PuzzleSolution solution, PuzzleBrief brief)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = field.origin;
            puzzle.destination = field.destination;
            puzzle.zoneCentre = brief.Translatable ? field.zoneCentre : solution.Zones[0].centre;
            puzzle.cubeSizeMin = field.cubeSizeMin;
            puzzle.cubeSizeMax = field.cubeSizeMax;
            puzzle.cubeSize = solution.Zones[0].halfExtents.x * 2f;
            puzzle.captureRadius = field.captureRadius;
            puzzle.translatable = field.translatable;
            puzzle.moves = field.moves;
            puzzle.resizable = field.resizable;
            puzzle.rotate = field.rotate;
            puzzle.launchSpeed = field.launchSpeed;
            puzzle.sources = field.sources;
            puzzle.tmin = field.tmin;
            puzzle.tmax = field.tmax;
            puzzle.entropyBudget = field.entropyBudget;
            return puzzle;
        }

        private static bool TryPickRun(
            PuzzleFieldPlanet[] planets, float targetRun, float clearance,
            out Vector3 origin, out Vector3 destination)
        {
            RunChoice choice = new() { Best = float.MaxValue };
            RunTarget target = new(targetRun, clearance);
            for (int i = 0; i < planets.Length; i++)
            {
                for (int j = i + 1; j < planets.Length; j++)
                    Consider(planets, i, j, target, ref choice);
            }

            origin = choice.Origin;
            destination = choice.Destination;
            return choice.Found;
        }

        private static void Consider(
            PuzzleFieldPlanet[] planets, int i, int j, RunTarget target, ref RunChoice choice)
        {
            float run = Vector3.Distance(planets[i].position, planets[j].position);
            if (run < target.Distance * MinRunFraction || run > target.Distance * MaxRunFactor)
                return;

            if (!CorridorIsClear(planets, i, j, target.Clearance))
                return;

            float delta = Mathf.Abs(run - target.Distance);
            if (delta >= choice.Best)
                return;

            choice.Best = delta;
            choice.Origin = planets[i].position;
            choice.Destination = planets[j].position;
            choice.Found = true;
        }

        private static bool CorridorIsClear(PuzzleFieldPlanet[] planets, int a, int b, float clearance)
        {
            Vector3 from = planets[a].position;
            Vector3 to = planets[b].position;
            for (int k = 0; k < planets.Length; k++)
            {
                if (k == a || k == b)
                    continue;

                float distance = PuzzleRunGeometry.DistanceToSegment(planets[k].position, from, to);
                if (distance < clearance + planets[k].radius)
                    return false;
            }

            return true;
        }

        private static bool StartFails(PuzzleData field, PuzzleSimSettings settings) =>
            PuzzleVerifier.Verify(field, null, settings).StartFails;

        private static float CaptureRadius(PuzzleSource[] sources, Vector3 destination)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if ((sources[i].home - destination).sqrMagnitude < 0.000001f)
                    return sources[i].scale * 0.5f;
            }

            return 0.06f;
        }

        private static float LaunchSpeed(PuzzleSimSettings settings) =>
            BareMissMargin * settings.NominalSpeed;

        private readonly struct RunTarget
        {
            public readonly float Distance;
            public readonly float Clearance;

            public RunTarget(float distance, float clearance)
            {
                Distance = distance;
                Clearance = clearance;
            }
        }

        private struct RunChoice
        {
            public Vector3 Origin;
            public Vector3 Destination;
            public float Best;
            public bool Found;
        }
    }
}
