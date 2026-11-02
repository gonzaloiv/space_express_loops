using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSolutionClassifier
    {
        private const float ScaleEpsilon = 0.0001f;
        private const float EndpointEpsilon = 0.01f;

        public static PuzzleSolutionKind Classify(
            SpaceShapingSession solved, SpaceShapingSetup setup,
            IReadOnlyList<TrajectoryResult> startResults) =>
            Classify(solved, setup, startResults, null);

        public static PuzzleSolutionKind Classify(
            SpaceShapingSession solved, SpaceShapingSetup setup,
            IReadOnlyList<TrajectoryResult> startResults,
            IReadOnlyList<TrajectoryResult> solvedResults) =>
            Classify(solved, setup, Obstacles(setup, startResults), solvedResults);

        internal static PuzzleSolutionKind Classify(
            SpaceShapingSession solved, SpaceShapingSetup setup, bool[] obstacles,
            IReadOnlyList<TrajectoryResult> solvedResults)
        {
            bool clear = false;
            bool curve = false;
            for (int i = 0; i < setup.Sources.Length; i++)
            {
                if (setup.Sources[i].FixedMass)
                    continue;

                float delta = solved.GetScale(i) - setup.Sources[i].InitialScale;
                if (delta < -ScaleEpsilon
                    && (obstacles[i] || NearAnyPath(setup.Sources[i], solvedResults)))
                    clear = true;
                if (delta > ScaleEpsilon)
                    curve = true;
            }

            return Kind(clear, curve);
        }

        private static bool NearAnyPath(
            SourceSpec source, IReadOnlyList<TrajectoryResult> solvedResults)
        {
            if (solvedResults == null)
                return false;

            for (int i = 0; i < solvedResults.Count; i++)
            {
                if (NearPath(source, solvedResults[i]))
                    return true;
            }

            return false;
        }

        private static bool NearPath(SourceSpec source, TrajectoryResult result)
        {
            if (!result.HasPoints)
                return false;

            float radius = source.InitialScale * 0.5f + EndpointEpsilon;
            if (result.Points.Count == 1)
                return (result.Points[0] - source.Home).sqrMagnitude <= radius * radius;

            for (int i = 1; i < result.Points.Count; i++)
            {
                if (PuzzleRunGeometry.DistanceToSegment(
                        source.Home, result.Points[i - 1], result.Points[i]) <= radius)
                    return true;
            }

            return false;
        }

        private static PuzzleSolutionKind Kind(bool clear, bool curve)
        {
            if (clear && curve)
                return PuzzleSolutionKind.Mixed;
            if (clear)
                return PuzzleSolutionKind.Clear;
            if (curve)
                return PuzzleSolutionKind.Curve;
            return PuzzleSolutionKind.Other;
        }

        public static bool Matches(PuzzleMechanism requested, PuzzleSolutionKind kind)
        {
            if (requested == PuzzleMechanism.Any)
                return true;

            if (requested == PuzzleMechanism.Curve)
                return kind == PuzzleSolutionKind.Curve;

            if (requested == PuzzleMechanism.Clear)
                return kind == PuzzleSolutionKind.Clear;

            return requested == PuzzleMechanism.Mixed && kind == PuzzleSolutionKind.Mixed;
        }

        internal static bool[] Obstacles(
            SpaceShapingSetup setup, IReadOnlyList<TrajectoryResult> startResults)
        {
            bool[] obstacles = new bool[setup.Sources.Length];
            MarkCrashSources(obstacles, startResults);
            MarkChords(obstacles, setup);
            return obstacles;
        }

        private static void MarkCrashSources(
            bool[] obstacles, IReadOnlyList<TrajectoryResult> startResults)
        {
            if (startResults == null)
                return;

            for (int i = 0; i < startResults.Count; i++)
            {
                TrajectoryResult result = startResults[i];
                if (result.Outcome != TrajectoryOutcome.Crash)
                    continue;

                if (result.CrashSource < 0 || result.CrashSource >= obstacles.Length)
                    continue;

                obstacles[result.CrashSource] = true;
            }
        }

        private static void MarkChords(bool[] obstacles, SpaceShapingSetup setup)
        {
            if (setup.Ships == null)
                return;

            for (int ship = 0; ship < setup.Ships.Length; ship++)
            {
                for (int i = 0; i < setup.Sources.Length; i++)
                {
                    if (IsObstacle(setup.Sources[i], setup.Ships[ship]))
                        obstacles[i] = true;
                }
            }
        }

        private static bool IsObstacle(SourceSpec source, ShipSpec ship)
        {
            if (IsEndpoint(source.Home, ship.Origin) || IsEndpoint(source.Home, ship.Destination))
                return false;

            float radius = source.InitialScale * 0.5f;
            return PuzzleRunGeometry.DistanceToSegment(source.Home, ship.Origin, ship.Destination)
                <= radius + EndpointEpsilon;
        }

        private static bool IsEndpoint(Vector3 home, Vector3 point) =>
            (home - point).sqrMagnitude < EndpointEpsilon * EndpointEpsilon;
    }
}
