using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        public static bool Apply(
            PuzzleBrief brief, PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleSimSettings settings)
        {
            puzzle.sources = WithSun(
                PuzzleSourceBridge.ToSources(planets.planets), brief.SunRadius, brief.SunScalable);
            CubeRange(planets, out float min, out float max);
            puzzle.cubeSizeMin = min;
            puzzle.cubeSizeMax = max;
            puzzle.translatable = brief.Translatable;
            puzzle.zones = brief.Zones;
            puzzle.resizable = brief.Resizable;
            puzzle.noFail = brief.NoFail;
            puzzle.noGrade = brief.NoGrade;
            if (!PuzzleRunChoice.TryPick(
                planets.planets, settings.RunDistance, brief.EndpointClearance, brief.SunRadius,
                out Vector3 origin, out Vector3 destination))
                return false;

            puzzle.origin = origin;
            puzzle.destination = destination;
            puzzle.captureRadius = puzzle.CaptureRadiusAtDestination();
            puzzle.zoneCentre = (origin + destination) * 0.5f;
            return true;
        }

        // The sun is a source at the field's centre, fixed or scalable as the seed says. It stays out of
        // the cube range so a planet-sized cube still reads as the player's tool, while the run choice
        // keeps the star's band clear.
        private static PuzzleSource[] WithSun(
            PuzzleSource[] sources, float sunRadius, bool sunScalable)
        {
            if (sunRadius <= 0f)
                return sources;

            PuzzleSource[] withSun = new PuzzleSource[sources.Length + 1];
            withSun[0] = new PuzzleSource
            {
                home = Vector3.zero,
                scale = sunRadius * 2f,
                name = SunName(sources.Length),
                fixedMass = !sunScalable,
                centreOnly = true
            };

            for (int i = 0; i < sources.Length; i++)
                withSun[i + 1] = sources[i];

            return withSun;
        }

        private static string SunName(int planetCount)
        {
            PlanetNames names = PlanetNames.Instance;
            return names != null ? names.NameAt(planetCount) : string.Empty;
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
            out PuzzleBakeStatus status, out PuzzleZone[] solution) =>
            Bake(planets, settings, brief, out status, out solution, out _);

        public static PuzzleData Bake(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief,
            out PuzzleBakeStatus status, out PuzzleZone[] solution, out PuzzleSolutionKind kind)
        {
            kind = PuzzleSolutionKind.Other;
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            if (!Apply(brief, field, planets, settings))
            {
                Object.DestroyImmediate(field);
                solution = null;
                return Fail(PuzzleBakeStatus.NoRun, out status);
            }

            PuzzleData baked = BakeField(
                field, settings, brief, out status, out solution, out kind);
            Object.DestroyImmediate(field);
            return baked;
        }

        private static PuzzleData BakeField(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief,
            out PuzzleBakeStatus status, out PuzzleZone[] solution, out PuzzleSolutionKind kind)
        {
            solution = null;
            kind = PuzzleSolutionKind.Other;
            if (!brief.SupportsZones)
                return Fail(PuzzleBakeStatus.UnsupportedBrief, out status);

            if (brief.Mechanism == PuzzleMechanism.Mixed && brief.Zones < 2)
                return Fail(PuzzleBakeStatus.UnsupportedMechanism, out status);

            PuzzleBakeStatus selected = PuzzleSolutionSelection.Select(
                field, settings, brief, out PuzzleSolution found, out kind);
            if (selected != PuzzleBakeStatus.Baked)
                return Fail(selected, out status);

            return Finish(field, settings, brief, found, out status, out solution, out kind);
        }

        private static PuzzleData Finish(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleSolution found,
            out PuzzleBakeStatus status, out PuzzleZone[] solution, out PuzzleSolutionKind kind)
        {
            solution = null;
            kind = found.Kind;
            PuzzleBakeStatus start = PuzzleStartCheck.Status(field, settings, brief);
            if (start != PuzzleBakeStatus.Baked)
                return Fail(start, out status);

            if (PuzzleSolutionSelection.HasShorter(field, settings, brief, found))
                return Fail(PuzzleBakeStatus.FewerZones, out status);

            return Confirm(field, settings, brief, found, out status, out solution, out kind);
        }

        private static PuzzleData Confirm(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleSolution found,
            out PuzzleBakeStatus status, out PuzzleZone[] solution, out PuzzleSolutionKind kind)
        {
            solution = null;
            PuzzleData verified = Verified(
                field, found, brief, settings, out status, out kind);
            if (verified == null)
                return null;

            if (!HasRoutes(verified, settings, brief, found.Zones))
            {
                Object.DestroyImmediate(verified);
                return Fail(PuzzleBakeStatus.SingleRoute, out status);
            }

            solution = found.Zones;
            return verified;
        }

        private static bool HasRoutes(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleZone[] zones)
        {
            puzzle.routes = PuzzleRouteCount.Count(puzzle, settings, zones);
            return puzzle.routes >= Mathf.Max(1, brief.MinRoutes);
        }

        private static PuzzleData Fail(PuzzleBakeStatus value, out PuzzleBakeStatus status)
        {
            status = value;
            return null;
        }

        private static PuzzleData Verified(
            PuzzleData field, PuzzleSolution solution, PuzzleBrief brief, PuzzleSimSettings settings,
            out PuzzleBakeStatus status, out PuzzleSolutionKind kind)
        {
            field.entropyBudget = solution.EntropySpent * PuzzleGrade.BudgetFactor;
            PuzzleData variant = Variant(field, solution, brief);
            PuzzleVerification check = PuzzleVerifier.Verify(variant, solution.Zones, settings);
            kind = check.Kind;
            if (check.Solved)
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
    }
}
