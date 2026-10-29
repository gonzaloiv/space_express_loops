using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        private const float WideSearchBand = 0.35f;

        public static bool Apply(
            PuzzleBrief brief, PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleSimSettings settings)
        {
            puzzle.sources = PuzzleSourceBridge.ToSources(planets.planets);
            puzzle.cubeSizeMin = brief.MinCubeSize;
            puzzle.cubeSizeMax = brief.MaxCubeSize;
            puzzle.translatable = brief.Translatable;
            puzzle.moves = brief.Moves;
            puzzle.resizable = brief.Resizable;
            puzzle.rotate = brief.Rotate;
            if (!PuzzleRunChoice.TryPick(
                planets.planets, settings.RunDistance, brief.CorridorClearance,
                out Vector3 origin, out Vector3 destination))
                return false;

            puzzle.origin = origin;
            puzzle.destination = destination;
            puzzle.captureRadius = puzzle.CaptureRadiusAtDestination();
            puzzle.zoneCentre = (origin + destination) * 0.5f;
            puzzle.launchSpeed = LaunchSpeed(settings, brief);
            puzzle.launchAngleDegs = LaunchAngleDegs(brief);
            return true;
        }

        public static PuzzleData Bake(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief,
            out PuzzleBakeStatus status, out PuzzleZone[] solution)
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            if (!Apply(brief, field, planets, settings))
            {
                Object.DestroyImmediate(field);
                solution = null;
                return Fail(PuzzleBakeStatus.NoRun, out status);
            }

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

            if (!Solve(field, settings, brief, crossing, out PuzzleSolution found))
                return Fail(PuzzleBakeStatus.NoSolution, out status);

            if (!StartFails(field, settings))
                return Fail(PuzzleBakeStatus.StartNotFailed, out status);

            if (HasShorterSolution(field, settings, brief, found))
                return Fail(PuzzleBakeStatus.FewerZones, out status);

            PuzzleData verified = Verified(field, found, brief, settings, out status);
            if (verified == null)
            {
                solution = null;
                return null;
            }

            if (!HasRoutes(verified, settings, brief, found.Zones))
            {
                Object.DestroyImmediate(verified);
                solution = null;
                return Fail(PuzzleBakeStatus.SingleRoute, out status);
            }

            solution = found.Zones;
            return verified;
        }

        private static bool HasRoutes(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleZone[] zones)
        {
            puzzle.routes = PuzzleRouteCount.Count(puzzle, settings, zones);

            // A movable single-zone level must offer a choice of where to scale. A multi-zone chain is
            // already pinned by the anchored window, so it is allowed to have exactly one route.
            return !brief.Translatable || brief.Moves > 1 || puzzle.routes >= PuzzleRouteCount.MinRoutes;
        }

        private static bool HasShorterSolution(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleSolution found)
        {
            if (brief.Moves <= 1)
                return false;

            float width = found.Zones[0].Width;
            float budget = found.EntropySpent * PuzzleGrade.BudgetFactor;
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
            {
                float search = brief.Moves > 1
                    ? Mathf.Max(brief.WindowTightness, WideSearchBand)
                    : brief.WindowTightness;
                PuzzleWindow.SetCrossing(field, crossing, search);
            }

            return true;
        }

        private static bool Solve(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, float crossing,
            out PuzzleSolution found)
        {
            found = default;
            if (!PuzzleSolutionSearch.TrySelect(field, settings, brief, out found))
                return false;

            return SettleWindow(field, settings, brief, crossing, found.ShipTime);
        }

        private static bool SettleWindow(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, float crossing, float shipTime)
        {
            if (!brief.OffPath && brief.Moves <= 1)
                return true;

            return PuzzleWindow.SetSolution(field, crossing, brief.WindowTightness, shipTime);
        }

        private static PuzzleData Verified(
            PuzzleData field, PuzzleSolution solution, PuzzleBrief brief, PuzzleSimSettings settings,
            out PuzzleBakeStatus status)
        {
            field.entropyBudget = solution.EntropySpent * PuzzleGrade.BudgetFactor;
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
            PuzzleData puzzle = Object.Instantiate(field);
            puzzle.zoneCentre = brief.Translatable ? field.zoneCentre : solution.Zones[0].centre;
            puzzle.cubeSize = solution.Zones[0].halfExtents.x * 2f;
            puzzle.moves = solution.Zones.Length;
            return puzzle;
        }

        private static bool StartFails(PuzzleData field, PuzzleSimSettings settings) =>
            PuzzleVerifier.Verify(field, null, settings).StartFails;

        private static float LaunchSpeed(PuzzleSimSettings settings, PuzzleBrief brief) =>
            brief.StartMargin * settings.NominalSpeed;

        private static float LaunchAngleDegs(PuzzleBrief brief)
        {
            if (!brief.ApplyRandomAngle)
                return 0f;

            SeededRandom random = new(brief.Seed);
            float magnitude = random.Range(6f, 12f);
            return random.NextInt() % 2 == 0 ? magnitude : -magnitude;
        }
    }
}
