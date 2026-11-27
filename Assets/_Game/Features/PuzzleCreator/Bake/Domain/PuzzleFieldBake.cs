using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldBake
    {
        public static PuzzleBakeResult Bake(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief,
            PuzzleScanTimings timings = null) =>
            Run(planets, brief.Reach(settings), brief, timings);

        public static bool Apply(
            PuzzleBrief brief, PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleSimSettings settings) =>
            Apply(brief, puzzle, planets, settings, out _, null);

        public static bool Apply(
            PuzzleBrief brief, PuzzleData puzzle, PuzzlePlanetListAsset planets,
            PuzzleSimSettings settings, out PuzzleRunReport runReport, PuzzleScanTimings timings = null)
        {
            PuzzleSource[] planetSources = PuzzleSourceBridge.ToSources(planets.planets);
            ApplyCubeRange(puzzle, planets, brief);
            puzzle.translatable = brief.Translatable;
            puzzle.zones = brief.Zones;
            puzzle.resizable = brief.Resizable;
            puzzle.noFail = brief.NoFail;
            puzzle.noGrade = brief.NoGrade;
            puzzle.noIntro = brief.NoIntro;
            puzzle.growOnly = brief.GrowOnly;
            PuzzleRunRequest run = BuildRunRequest(brief, planets, settings);
            if (brief.SunPlacement == PuzzleSunPlacement.OffCorridor)
            {
                run.SunRadius = 0f;
                run.AllowSunOnChord = true;
            }

            long started = timings != null ? timings.Start() : 0;
            bool picked = PuzzleRunChoice.TryPick(
                run, out Vector3 origin, out Vector3 destination, out runReport);
            timings?.Add(PuzzleScanTimings.Setup, started);
            if (!picked)
                return false;

            if (!AddSun(brief, puzzle, planetSources, planets.planets))
                return false;

            puzzle.origin = origin;
            puzzle.destination = destination;
            puzzle.captureRadius = puzzle.CaptureRadiusAtDestination();
            puzzle.zoneCentre = (origin + destination) * 0.5f;
            return true;
        }

        private static bool AddSun(
            PuzzleBrief brief, PuzzleData puzzle, PuzzleSource[] planetSources,
            PuzzleFieldPlanet[] planets)
        {
            if (brief.SunRadius <= 0f)
            {
                puzzle.sources = planetSources;
                return true;
            }

            if (brief.SunPlacement != PuzzleSunPlacement.OffCorridor)
            {
                puzzle.sources = PuzzleSunSources.WithSun(
                    planetSources, brief.SunRadius, brief.SunScalable);
                return true;
            }

            if (!PuzzleSunPlacementChoice.TryHome(
                    planets, brief.SunRadius, brief.EndpointClearance, out Vector3 sunHome))
                return false;

            puzzle.sources = PuzzleSunSources.WithSun(
                planetSources, brief.SunRadius, brief.SunScalable, sunHome);
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
            AllowSunOnChord = brief.SunOnChord,
            HasCandidate = planets.hasCandidate,
            CandidateOrigin = planets.candidateOrigin,
            CandidateDestination = planets.candidateDestination
        };

        // A job that puts the cube over the star must use a cube wide enough to cover it — never a
        // smaller one, or the search can scale the star from its centre point with a cube the player
        // would see sitting inside the star.
        private static void ApplyCubeRange(
            PuzzleData puzzle, PuzzlePlanetListAsset planets, PuzzleBrief brief)
        {
            CubeRange(planets, out float min, out float max);
            if (brief.SunRule != PuzzleSunRule.CentredGrown)
            {
                puzzle.cubeSizeMin = min;
                puzzle.cubeSizeMax = max;
                return;
            }

            float enclosing = PuzzleSunRuleCheck.SunEnclosingSize(brief.SunRadius * 2f);
            puzzle.cubeSizeMax = Mathf.Max(max, enclosing);
            puzzle.cubeSizeMin = Mathf.Max(min, enclosing);
        }

        // The cube must enclose the planet it covers, so the range spans the field's planets with the
        // same margin the search requires: the smallest planet for the floor, the largest for the
        // ceiling.
        public static void CubeRange(PuzzleFieldPlanet[] planets, out float min, out float max)
        {
            float smallest = float.MaxValue;
            float largest = 0f;
            for (int i = 0; i < planets.Length; i++)
            {
                smallest = Mathf.Min(smallest, planets[i].radius);
                largest = Mathf.Max(largest, planets[i].radius);
            }

            min = Mathf.Max(smallest, 0.01f) * 2f * PuzzleZoneCandidates.PlanetMargin;
            max = Mathf.Max(largest, 0.01f) * 2f * PuzzleZoneCandidates.PlanetMargin;
        }

        private static void CubeRange(PuzzlePlanetListAsset planets, out float min, out float max) =>
            CubeRange(planets.planets, out min, out max);

        private static PuzzleBakeResult Run(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief,
            PuzzleScanTimings timings)
        {
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            if (!Apply(brief, field, planets, settings, out PuzzleRunReport runReport, timings))
            {
                Object.DestroyImmediate(field);
                return Fail(PuzzleBakeStatus.NoRun, runReport);
            }

            PuzzleBakeResult result = BakeField(field, settings, brief, timings);
            result.RunReport = runReport;
            Object.DestroyImmediate(field);
            return result;
        }

        private static PuzzleBakeResult BakeField(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief, PuzzleScanTimings timings)
        {
            if (!brief.SupportsZones)
                return Fail(PuzzleBakeStatus.UnsupportedBrief);

            if (brief.Mechanism == PuzzleMechanism.Mixed && brief.Zones < 2)
                return Fail(PuzzleBakeStatus.UnsupportedMechanism);

            long started = timings != null ? timings.Start() : 0;
            PuzzleBakeStatus selected = PuzzleSolutionSelection.Select(
                field, settings, brief, out PuzzleSolution found, out PuzzleSolutionKind kind);
            timings?.Add(PuzzleScanTimings.Select, started);
            if (selected != PuzzleBakeStatus.Baked)
                return Fail(selected, kind);

            return Check(field, settings, brief, found, kind, timings);
        }

        // The constructor's answer takes the same path as a searched one: the start, the shorter-zone
        // rule, then the confirm (entropy, frames, routes, sun rule).
        private static PuzzleBakeResult Check(
            PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief,
            PuzzleSolution found, PuzzleSolutionKind kind, PuzzleScanTimings timings)
        {
            PuzzleBakeStatus start = PuzzleStartCheck.Status(field, settings, brief);
            if (start != PuzzleBakeStatus.Baked)
                return Fail(start, kind);

            long started = timings != null ? timings.Start() : 0;
            bool shorter = PuzzleSolutionSelection.HasShorter(field, settings, brief, found);
            timings?.Add(PuzzleScanTimings.Select, started);
            if (shorter)
                return Fail(PuzzleBakeStatus.FewerZones, kind);

            return PuzzleFieldConfirm.Run(field, settings, brief, found, timings);
        }

        internal static PuzzleBakeResult Fail(
            PuzzleBakeStatus status, PuzzleSolutionKind kind = PuzzleSolutionKind.Other) =>
            new() { Status = status, Kind = kind };

        private static PuzzleBakeResult Fail(PuzzleBakeStatus status, PuzzleRunReport runReport) =>
            new() { Status = status, RunReport = runReport };

        // A fixed level ships the solved zone geometry and the player finds the scales; a movable
        // level ships one start frame near the brief's start rule and the player moves and scales it.
        public static PuzzleFrame[] BuildFrames(
            PuzzleData field, PuzzleSolution solution, PuzzleBrief brief, PuzzleSimSettings settings)
        {
            if (!brief.Translatable)
                return SolvedFrames(solution.Zones);

            float size = solution.Zones[0].Width;
            Vector3 centre = PuzzleStartFrame.Place(field, settings, StartSpot(field, brief.StartRule), size);
            return new[] { new PuzzleFrame { centre = centre, size = size } };
        }

        public static Vector3 StartSpot(PuzzleData field, PuzzleStartRule rule) =>
            rule == PuzzleStartRule.FieldCentre ? Vector3.zero : field.zoneCentre;

        private static PuzzleFrame[] SolvedFrames(PuzzleZone[] zones)
        {
            PuzzleFrame[] frames = new PuzzleFrame[zones.Length];
            for (int i = 0; i < zones.Length; i++)
                frames[i] = new PuzzleFrame { centre = zones[i].centre, size = zones[i].Width };

            return frames;
        }
    }
}
