using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        private const float StarBudgetFactor = 3f;
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
            puzzle.launchSpeed = LaunchSpeed(settings, brief);
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
            if (!brief.SupportsMoves)
                return Fail(PuzzleBakeStatus.UnsupportedBrief, out status);

            if (!PrepareField(field, settings, brief, out float crossing))
                return Fail(PuzzleBakeStatus.NoRun, out status);

            if (!brief.OffPath && !StartFails(field, settings))
                return Fail(PuzzleBakeStatus.StartNotFailed, out status);

            if (!Solve(field, settings, brief, crossing, out PuzzleSolution found, out bool startRejected))
                return Fail(PuzzleBakeStatus.NoSolution, out status);

            if (startRejected)
                return Fail(PuzzleBakeStatus.StartNotFailed, out status);

            if (HasShorterSolution(field, settings, brief, found))
                return Fail(PuzzleBakeStatus.FewerZones, out status);

            solution = found.Zones;
            return Verified(field, found, brief, settings, out status);
        }

        private static bool HasShorterSolution(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleSolution found)
        {
            if (brief.Moves <= 1)
                return false;

            float width = found.Zones[0].Width;
            float budget = found.EntropySpent * StarBudgetFactor;
            PuzzleBrief shorter = brief;
            for (int moves = 1; moves < brief.Moves; moves++)
            {
                shorter.Moves = moves;
                if (PuzzleSolutionSearch.TryFindAtWidth(field, settings, shorter, width, out PuzzleSolution candidate)
                    && candidate.EntropySpent <= budget)
                    return true;
            }

            return false;
        }

        private static PuzzleData Fail(PuzzleBakeStatus value, out PuzzleBakeStatus status)
        {
            status = value;
            return null;
        }

        private static bool PrepareField(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, out float crossing)
        {
            if (!PuzzleWindow.TryCrossing(field, settings, out crossing))
                return false;

            if (!brief.OffPath)
                PuzzleWindow.SetCrossing(field, crossing, brief.WindowTightness);

            return true;
        }

        private static bool Solve(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, float crossing,
            out PuzzleSolution found, out bool startRejected)
        {
            startRejected = false;
            if (!PuzzleSolutionSearch.TrySelect(field, settings, brief, out found))
                return false;

            if (!SettleWindow(field, settings, brief, crossing, found.ShipTime))
                return false;

            startRejected = !StartFails(field, settings);
            return true;
        }

        private static bool SettleWindow(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, float crossing, float shipTime)
        {
            if (!brief.OffPath)
                return true;

            return PuzzleWindow.SetSolution(field, crossing, brief.WindowTightness, shipTime);
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
            puzzle.moves = solution.Zones.Length;
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

        private static float LaunchSpeed(PuzzleSimSettings settings, PuzzleBrief brief) =>
            brief.StartMargin * settings.NominalSpeed;

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
