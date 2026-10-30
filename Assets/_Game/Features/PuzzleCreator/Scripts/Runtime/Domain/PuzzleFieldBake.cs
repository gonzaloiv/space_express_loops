using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        public static bool Apply(
            PuzzleBrief brief, PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleSimSettings settings)
        {
            puzzle.sources = PuzzleSourceBridge.ToSources(planets.planets);
            CubeRange(planets, out float min, out float max);
            puzzle.cubeSizeMin = min;
            puzzle.cubeSizeMax = max;
            puzzle.translatable = brief.Translatable;
            puzzle.zones = brief.Zones;
            puzzle.resizable = brief.Resizable;
            if (!PuzzleRunChoice.TryPick(
                planets.planets, settings.RunDistance, brief.EndpointClearance,
                out Vector3 origin, out Vector3 destination))
                return false;

            puzzle.origin = origin;
            puzzle.destination = destination;
            puzzle.captureRadius = puzzle.CaptureRadiusAtDestination();
            puzzle.zoneCentre = (origin + destination) * 0.5f;
            return true;
        }

        // The cube must enclose the planet it covers, so the range spans the field's planets with the
        // same margin the search requires: the smallest planet for the floor, the largest for the
        // ceiling.
        private static void CubeRange(PuzzlePlanetListAsset planets, out float min, out float max)
        {
            float smallest = float.MaxValue;
            float largest = 0f;
            for (int i = 0; i < planets.planets.Length; i++)
            {
                float radius = planets.planets[i].radius;
                smallest = Mathf.Min(smallest, radius);
                largest = Mathf.Max(largest, radius);
            }

            min = Mathf.Max(smallest, 0.01f) * 2f * PuzzleZoneCandidates.PlanetMargin;
            max = Mathf.Max(largest, 0.01f) * 2f * PuzzleZoneCandidates.PlanetMargin;
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
            if (!brief.SupportsZones)
                return Fail(PuzzleBakeStatus.UnsupportedBrief, out status);

            if (!PuzzleSolutionSearch.TrySelect(field, settings, brief, out PuzzleSolution found))
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
            // already pinned by the baked solution, so it is allowed to have exactly one route.
            return !brief.Translatable || brief.Zones > 1 || puzzle.routes >= PuzzleRouteCount.MinRoutes;
        }

        private static bool HasShorterSolution(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleSolution found)
        {
            if (brief.Zones <= 1)
                return false;

            float width = found.Zones[0].Width;
            float budget = found.EntropySpent * PuzzleGrade.BudgetFactor;
            PuzzleBrief shorter = brief;
            for (int zones = 1; zones < brief.Zones; zones++)
            {
                shorter.Zones = zones;
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
            puzzle.frames = BuildFrames(field, solution, brief);
            puzzle.zones = solution.Zones.Length;
            return puzzle;
        }

        // A fixed level ships the solved zone geometry and the player finds the scales; a movable
        // level ships one start frame at the run midpoint and the player moves and scales it.
        public static PuzzleFrame[] BuildFrames(
            PuzzleData field, PuzzleSolution solution, PuzzleBrief brief)
        {
            if (!brief.Translatable)
                return SolvedFrames(solution.Zones);

            return new[]
            {
                new PuzzleFrame { centre = field.zoneCentre, size = solution.Zones[0].Width }
            };
        }

        private static PuzzleFrame[] SolvedFrames(PuzzleZone[] zones)
        {
            PuzzleFrame[] frames = new PuzzleFrame[zones.Length];
            for (int i = 0; i < zones.Length; i++)
                frames[i] = new PuzzleFrame { centre = zones[i].centre, size = zones[i].Width };

            return frames;
        }

        private static bool StartFails(PuzzleData field, PuzzleSimSettings settings) =>
            PuzzleVerifier.Verify(field, null, settings).StartFails;
    }
}
