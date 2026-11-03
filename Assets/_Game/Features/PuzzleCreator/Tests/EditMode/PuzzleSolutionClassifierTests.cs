using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleSolutionClassifierTests
    {
        [Test]
        public void Classify_ShrunkCrashSource_IsClear()
        {
            PuzzleData puzzle = Field(Source(0.2f, false));
            SpaceShapingSetup setup = Setup(puzzle);
            SpaceShapingSession solved = Solve(setup, Zone(0f, 0.6f));
            TrajectoryResult[] start = { Crash(0) };

            Assert.AreEqual(
                PuzzleSolutionKind.Clear, PuzzleSolutionClassifier.Classify(solved, setup, start));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Classify_GrownCrashSource_IsCurve()
        {
            PuzzleData puzzle = Field(Source(0.2f, false));
            SpaceShapingSetup setup = Setup(puzzle);
            SpaceShapingSession solved = Solve(setup, Zone(0f, 1.4f));
            TrajectoryResult[] start = { Crash(0) };

            Assert.AreEqual(
                PuzzleSolutionKind.Curve, PuzzleSolutionClassifier.Classify(solved, setup, start));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Classify_ClearAndCurve_IsMixed()
        {
            PuzzleData puzzle = Field(Source(0.2f, false), Other());
            SpaceShapingSetup setup = Setup(puzzle);
            SpaceShapingSession solved = Solve(
                setup, Zone(0f, 0.6f), Zone(0.3f, 1.4f));
            TrajectoryResult[] start = { Crash(0) };

            Assert.AreEqual(
                PuzzleSolutionKind.Mixed, PuzzleSolutionClassifier.Classify(solved, setup, start));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Classify_GrowThenShrinkOffChord_IsMixed()
        {
            PuzzleData puzzle = Field(Source(0.2f, false), Other());
            SpaceShapingSetup setup = Setup(puzzle);
            SpaceShapingSession solved = Solve(
                setup, Zone(0f, 1.4f), Zone(0.3f, 0.6f));
            TrajectoryResult[] start = { Crash(0) };
            TrajectoryResult[] solvedRuns = { ArriveThrough(0.3f) };

            Assert.AreEqual(
                PuzzleSolutionKind.Mixed,
                PuzzleSolutionClassifier.Classify(solved, setup, start, solvedRuns));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Classify_ShrunkOffChordSource_IsNone()
        {
            PuzzleData puzzle = Field(Other());
            SpaceShapingSetup setup = Setup(puzzle);
            SpaceShapingSession solved = Solve(setup, Zone(0.3f, 0.6f));
            TrajectoryResult[] start = { Miss() };

            Assert.AreEqual(
                PuzzleSolutionKind.Other, PuzzleSolutionClassifier.Classify(solved, setup, start));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Classify_GrownOffChordSource_IsCurve()
        {
            PuzzleData puzzle = Field(Other());
            SpaceShapingSetup setup = Setup(puzzle);
            SpaceShapingSession solved = Solve(setup, Zone(0.3f, 1.4f));
            TrajectoryResult[] start = { Miss() };

            Assert.AreEqual(
                PuzzleSolutionKind.Curve, PuzzleSolutionClassifier.Classify(solved, setup, start));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Classify_StartMissOnChord_IsClear()
        {
            PuzzleData puzzle = Field(Source(0.2f, false));
            SpaceShapingSetup setup = Setup(puzzle);
            SpaceShapingSession solved = Solve(setup, Zone(0f, 0.6f));
            TrajectoryResult[] start = { Miss() };

            Assert.AreEqual(
                PuzzleSolutionKind.Clear, PuzzleSolutionClassifier.Classify(solved, setup, start));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Classify_FixedCrashSource_IsNone()
        {
            PuzzleData puzzle = Field(Source(0.2f, true));
            SpaceShapingSetup setup = Setup(puzzle);
            SpaceShapingSession solved = Solve(setup, Zone(0f, 0.6f));
            TrajectoryResult[] start = { Crash(0) };

            Assert.AreEqual(
                PuzzleSolutionKind.Other, PuzzleSolutionClassifier.Classify(solved, setup, start));
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void Matches_RequestedRole_IsStrict()
        {
            Assert.IsTrue(PuzzleSolutionClassifier.Matches(
                PuzzleMechanism.Any, PuzzleSolutionKind.Other));
            Assert.IsTrue(PuzzleSolutionClassifier.Matches(
                PuzzleMechanism.Curve, PuzzleSolutionKind.Curve));
            Assert.IsTrue(PuzzleSolutionClassifier.Matches(
                PuzzleMechanism.Clear, PuzzleSolutionKind.Clear));
            Assert.IsTrue(PuzzleSolutionClassifier.Matches(
                PuzzleMechanism.Mixed, PuzzleSolutionKind.Mixed));
            Assert.IsFalse(PuzzleSolutionClassifier.Matches(
                PuzzleMechanism.Curve, PuzzleSolutionKind.Clear));
            Assert.IsFalse(PuzzleSolutionClassifier.Matches(
                PuzzleMechanism.Clear, PuzzleSolutionKind.Curve));
            Assert.IsFalse(PuzzleSolutionClassifier.Matches(
                PuzzleMechanism.Mixed, PuzzleSolutionKind.Curve));
        }

        private static PuzzleData Field(params PuzzleSource[] sources)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-0.5f, 0f, 0f);
            puzzle.destination = new Vector3(0.5f, 0f, 0f);
            puzzle.captureRadius = 0.05f;
            puzzle.sources = sources;
            return puzzle;
        }

        private static PuzzleSource Source(float scale, bool fixedMass) => new()
        {
            home = Vector3.zero,
            scale = scale,
            fixedMass = fixedMass
        };

        private static PuzzleSource Other() => new()
        {
            home = new Vector3(0f, 0f, 0.3f),
            scale = 0.1f
        };

        private static PuzzleZone Zone(float z, float scale) =>
            PuzzleZoneCandidates.Zone(new Vector3(0f, 0f, z), 0.2f, scale, 0f);

        private static SpaceShapingSetup Setup(PuzzleData puzzle)
        {
            SpaceShapingSettings settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            Object.DestroyImmediate(settings);
            return SpaceShapingSetup.Build(puzzle, sim);
        }

        private static SpaceShapingSession Solve(SpaceShapingSetup setup, params PuzzleZone[] zones)
        {
            SpaceShapingSession session = new(setup);
            for (int i = 0; i < zones.Length; i++)
                session.ApplyZone(zones[i].ToSpaceZone());

            return session;
        }

        private static TrajectoryResult Crash(int source) =>
            new(TrajectoryOutcome.Crash, new List<Vector3>(), source);

        private static TrajectoryResult Miss() =>
            new(TrajectoryOutcome.Miss, new List<Vector3>());

        private static TrajectoryResult ArriveThrough(float z) =>
            new(TrajectoryOutcome.Arrive, new List<Vector3>
            {
                new(-0.5f, 0f, 0f),
                new(0f, 0f, z),
                new(0.5f, 0f, 0f)
            });
    }
}
