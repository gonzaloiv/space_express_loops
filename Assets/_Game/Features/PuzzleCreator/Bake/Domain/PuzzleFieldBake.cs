using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        public static PuzzleBakeResult Bake(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief) =>
            Run(planets, settings, brief);

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
            PuzzleRunRequest run = BuildRunRequest(brief, planets, settings);
            if (!PuzzleRunChoice.TryPick(run, out Vector3 origin, out Vector3 destination))
                return false;

            puzzle.origin = origin;
            puzzle.destination = destination;
            puzzle.captureRadius = puzzle.CaptureRadiusAtDestination();
            puzzle.zoneCentre = (origin + destination) * 0.5f;
            return true;
        }

        private static PuzzleRunRequest BuildRunRequest(
            PuzzleBrief brief, PuzzlePlanetListAsset planets, PuzzleSimSettings settings) => new()
        {
            Planets = planets.planets,
            TargetRun = settings.RunDistance,
            EndpointClearance = brief.EndpointClearance,
            SunRadius = brief.SunRadius,
            RequirePlanetBlocker = true,
            AllowSunOnChord = brief.Mechanism != PuzzleMechanism.Curve
                || brief.SunRule == PuzzleSunRule.InZone
        };

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

        private static PuzzleBakeResult Run(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief)
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            if (!Apply(brief, field, planets, settings))
            {
                Object.DestroyImmediate(field);
                return Fail(PuzzleBakeStatus.NoRun);
            }

            PuzzleBakeResult result = BakeField(field, settings, brief);
            Object.DestroyImmediate(field);
            return result;
        }

        private static PuzzleBakeResult BakeField(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief)
        {
            if (!brief.SupportsZones)
                return Fail(PuzzleBakeStatus.UnsupportedBrief);

            if (brief.Mechanism == PuzzleMechanism.Mixed && brief.Zones < 2)
                return Fail(PuzzleBakeStatus.UnsupportedMechanism);

            PuzzleBakeStatus selected = PuzzleSolutionSelection.Select(
                field, settings, brief, out PuzzleSolution found, out PuzzleSolutionKind kind);
            if (selected != PuzzleBakeStatus.Baked)
                return Fail(selected, kind);

            PuzzleBakeStatus start = PuzzleStartCheck.Status(field, settings, brief);
            if (start != PuzzleBakeStatus.Baked)
                return Fail(start);

            if (PuzzleSolutionSelection.HasShorter(field, settings, brief, found))
                return Fail(PuzzleBakeStatus.FewerZones);

            return Confirm(field, settings, brief, found, kind);
        }

        private static PuzzleBakeResult Confirm(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleSolution found,
            PuzzleSolutionKind kind)
        {
            field.entropyBudget = found.EntropySpent * PuzzleGrade.BudgetFactor;
            PuzzleData variant = Variant(field, found, brief);
            PuzzleVerification check = PuzzleVerifier.Verify(variant, found.Zones, settings);
            kind = check.Kind;
            if (!check.Solved)
            {
                Object.DestroyImmediate(variant);
                return Fail(PuzzleBakeStatus.Unverified, kind);
            }

            if (!PuzzleSunRuleCheck.Matches(variant, found.Zones, brief.SunRule))
            {
                Object.DestroyImmediate(variant);
                return Fail(PuzzleBakeStatus.SunRuleFailed, kind);
            }

            if (!HasRoutes(variant, settings, brief, found.Zones))
            {
                Object.DestroyImmediate(variant);
                return Fail(PuzzleBakeStatus.SingleRoute, kind);
            }

            return new PuzzleBakeResult
            {
                Puzzle = variant,
                Solution = found.Zones,
                Status = PuzzleBakeStatus.Baked,
                Kind = kind
            };
        }

        private static bool HasRoutes(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleZone[] zones)
        {
            puzzle.routes = PuzzleRouteCount.Count(puzzle, settings, zones);
            return puzzle.routes >= Mathf.Max(1, brief.MinRoutes);
        }

        private static PuzzleBakeResult Fail(PuzzleBakeStatus status, PuzzleSolutionKind kind = PuzzleSolutionKind.Other) =>
            new() { Status = status, Kind = kind };

        private static PuzzleData Variant(PuzzleData field, PuzzleSolution solution, PuzzleBrief brief)
        {
            PuzzleData puzzle = Object.Instantiate(field);
            puzzle.frames = BuildFrames(field, solution, brief);
            puzzle.zones = solution.Zones.Length;
            return puzzle;
        }

        // A fixed level ships the solved zone geometry and the player finds the scales; a movable
        // level ships one start frame at the brief's start rule and the player moves and scales it.
        public static PuzzleFrame[] BuildFrames(
            PuzzleData field, PuzzleSolution solution, PuzzleBrief brief)
        {
            if (!brief.Translatable)
                return SolvedFrames(solution.Zones);

            Vector3 centre = brief.StartRule == PuzzleStartRule.FieldCentre
                ? Vector3.zero
                : field.zoneCentre;
            return new[]
            {
                new PuzzleFrame { centre = centre, size = solution.Zones[0].Width }
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
