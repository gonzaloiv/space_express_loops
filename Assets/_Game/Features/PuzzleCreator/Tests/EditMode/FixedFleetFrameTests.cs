using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // A fixed fleet ships the solved zone as its frame, so the validator checks that the frame solves
    // in place (a PuzzleFrame stores only centre and size) and keeps clear of endpoints. 02_05 is the
    // edge-sun fixture; 02_01 is the two-ship negative fixture that still solves under today's settings.
    public class FixedFleetFrameTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private const string FleetPath = "Assets/_Game/Resources/Chapters/02/Job_02_01_Puzzle.asset";
        private const string EdgeFleetPath =
            "Assets/_Game/Resources/Chapters/02/Job_02_05_Puzzle.asset";

        [Test]
        public void Validate_FixedFrameAtTheFleetZone_Passes()
        {
            PuzzleData puzzle = LoadEdge();
            PuzzleSimSettings sim = Sim();
            Assert.IsTrue(
                PuzzleFleetSolution.TryFind(puzzle, sim, PuzzleMechanism.Any, out PuzzleZone zone),
                "no fleet zone");

            PuzzleData fixedPuzzle = Fixed(puzzle, zone);
            try
            {
                Assert.IsTrue(
                    Validate(fixedPuzzle, zone, sim, out List<string> failures),
                    string.Join("\n", failures));
                Assert.Less(
                    Vector3.Distance(fixedPuzzle.frames[0].centre, zone.centre), 1e-4f,
                    "a fixed fleet ships the solution zone as its frame");
            }
            finally
            {
                Object.DestroyImmediate(fixedPuzzle);
            }
        }

        [Test]
        public void Validate_FixedFrameOffTheFleetZone_Fails()
        {
            PuzzleData puzzle = Load();
            PuzzleSimSettings sim = Sim();
            Assert.IsTrue(
                PuzzleFleetSolution.TryFind(puzzle, sim, PuzzleMechanism.Any, out PuzzleZone zone),
                "no fleet zone");

            PuzzleData fixedPuzzle = Fixed(puzzle, zone);
            try
            {
                fixedPuzzle.frames[0] = new PuzzleFrame
                {
                    centre = zone.centre + Vector3.up * 100f,
                    size = zone.halfExtents.x * 2f
                };

                Assert.IsFalse(
                    Validate(fixedPuzzle, zone, sim, out List<string> failures),
                    string.Join("\n", failures));
                Assert.That(failures, Does.Contain("the fixed frame does not solve in place"));
            }
            finally
            {
                Object.DestroyImmediate(fixedPuzzle);
            }
        }

        [Test]
        public void ExistingContract_BoundsOff_StillValidates()
        {
            PuzzleData puzzle = LoadEdge();
            PuzzleSimSettings sim = Sim();
            Assert.IsTrue(
                PuzzleFleetSolution.TryFind(puzzle, sim, PuzzleMechanism.Any, out PuzzleZone zone),
                "no fleet zone");

            PuzzleJobContract contract = Contract(puzzle);
            Assert.AreEqual(0f, contract.MaxScaleDelta);
            Assert.AreEqual(0f, contract.MinScaleDelta);
            Assert.AreEqual(0f, contract.MinStartDistance);
            Assert.IsTrue(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Other, new[] { zone }, contract,
                    out List<string> failures, sim),
                string.Join("\n", failures));
        }

        private static bool Validate(
            PuzzleData puzzle, PuzzleZone zone, PuzzleSimSettings sim, out List<string> failures) =>
            PuzzleJobValidator.Validate(
                puzzle, PuzzleSolutionKind.Other, new[] { zone }, Contract(puzzle),
                out failures, sim);

        private static PuzzleData Fixed(PuzzleData source, PuzzleZone zone)
        {
            PuzzleData fixedPuzzle = Object.Instantiate(source);
            fixedPuzzle.translatable = false;
            fixedPuzzle.frames = new[]
            {
                new PuzzleFrame { centre = zone.centre, size = zone.halfExtents.x * 2f }
            };
            return fixedPuzzle;
        }

        private static PuzzleData Load()
        {
            PuzzleData puzzle = AssetDatabase.LoadAssetAtPath<PuzzleData>(FleetPath);
            Assert.IsNotNull(puzzle, FleetPath);
            return puzzle;
        }

        private static PuzzleData LoadEdge()
        {
            PuzzleData puzzle = AssetDatabase.LoadAssetAtPath<PuzzleData>(EdgeFleetPath);
            Assert.IsNotNull(puzzle, EdgeFleetPath);
            return puzzle;
        }

        private static PuzzleSimSettings Sim()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            return PuzzleSimSettings.From(settings);
        }

        private static PuzzleSunPlacement PlacementOf(PuzzleData puzzle)
        {
            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                if (!puzzle.sources[i].centreOnly)
                    continue;

                return puzzle.sources[i].home.sqrMagnitude > 0.000001f
                    ? PuzzleSunPlacement.OffCorridor
                    : PuzzleSunPlacement.Centre;
            }

            return PuzzleSunPlacement.Centre;
        }

        private static PuzzleJobContract Contract(PuzzleData puzzle)
        {
            int planets = puzzle.sources.Length;
            bool hasSun = false;
            for (int i = 0; i < puzzle.sources.Length; i++)
                hasSun |= puzzle.sources[i].centreOnly;
            if (hasSun)
                planets--;

            PuzzleJobContract job = PuzzleJobContracts.ById(puzzle.id);

            return new PuzzleJobContract
            {
                Id = puzzle.id,
                Ships = puzzle.ships.Length,
                Relation = job.Relation,
                HasSun = hasSun,
                SunPlacement = PlacementOf(puzzle),
                SunOnChord = PuzzleJobValidator.SunOnChord(puzzle),
                SunRule = PuzzleSunRule.Any,
                Mechanism = PuzzleMechanism.Any,
                Zones = puzzle.zones,
                MinRoutes = 0,
                Translatable = puzzle.translatable,
                StartRule = PuzzleStartRule.RunMidpoint,
                PlanetCount = planets,
                NoFail = false,
                NoGrade = false,
                NoIntro = false
            };
        }
    }
}
