using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The Clear fleet rules: a shared blocker is one that sits on two or more ships' runs, and a
    // movable start must still need moving.
    public class PuzzleClearFleetTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private readonly List<PuzzleData> created = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
                Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        [Test]
        public void Validate_TwoShipClearFleet_SharedBlocker_Passes()
        {
            PuzzleData puzzle = ClearFleet(0.2f);
            puzzle.frames = new[] { new PuzzleFrame { centre = new Vector3(0f, 0.2f, 0f), size = 0.1f } };
            PuzzleJobContract contract = ClearFleetContract();

            Assert.IsTrue(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Clear, FleetSolution(), contract,
                    out List<string> failures),
                string.Join("\n", failures));
        }

        [Test]
        public void Validate_TwoShipClearFleet_TooFewSharedBlockers_Fails()
        {
            PuzzleData puzzle = ClearFleet(0.2f);
            PuzzleJobContract contract = ClearFleetContract();
            contract.MinBlockers = 2;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Clear, FleetSolution(), contract,
                    out List<string> failures));
            Assert.That(failures, Does.Contain("blockers 1, expected at least 2"));
        }

        [Test]
        public void Validate_TwoShipClearFleet_BlockerOnOneRun_DoesNotCount()
        {
            PuzzleData puzzle = ClearFleet(0.2f);
            PuzzleShip far = puzzle.ships[1];
            far.origin = new Vector3(-1f, 2f, 0f);
            far.destination = new Vector3(1f, 2f, 0f);
            puzzle.ships[1] = far;
            PuzzleJobContract contract = ClearFleetContract();

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Clear, FleetSolution(), contract,
                    out List<string> failures));
            Assert.That(failures, Does.Contain("blockers 0, expected at least 1"));
        }

        [Test]
        public void Validate_StartThatSolvesInPlace_Fails()
        {
            PuzzleData puzzle = SolveInPlacePuzzle();
            PuzzleJobContract contract = SolveInPlaceContract();

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Zone(Vector3.zero), contract,
                    out List<string> failures, Sim()));
            Assert.That(failures, Does.Contain("the start solves in place"));
        }

        private PuzzleData ClearFleet(float blockerY)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-1f, 0f, 0f);
            puzzle.destination = new Vector3(1f, 0f, 0f);
            puzzle.sources = new[]
            {
                new PuzzleSource { home = new Vector3(0f, blockerY, 0f), scale = 0.5f }
            };
            puzzle.ships = new[]
            {
                new PuzzleShip
                {
                    origin = puzzle.origin,
                    destination = puzzle.destination,
                    captureRadius = 0.1f
                },
                new PuzzleShip
                {
                    origin = new Vector3(-1f, 0.4f, 0f),
                    destination = new Vector3(1f, 0.4f, 0f),
                    captureRadius = 0.1f
                }
            };
            puzzle.zones = 1;
            puzzle.routes = 1;
            puzzle.translatable = false;
            created.Add(puzzle);
            return puzzle;
        }

        private static PuzzleJobContract ClearFleetContract() => new()
        {
            Id = "test", Seed = 1, Attempts = 1, PlanetCount = 1,
            FieldShape = PuzzleFieldShape.Ring,
            HasSun = false, SunOnChord = false, SunRule = PuzzleSunRule.Any, MinBlockers = 1,
            Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 1, Ships = 2,
            Relation = PuzzleShipRelation.Separate,
            Translatable = false, StartRule = PuzzleStartRule.RunMidpoint,
            NoFail = false, NoGrade = false,
        };

        private static PuzzleZone[] FleetSolution() =>
            Zone(new Vector3(0f, 0.2f, 0f), 0.8f);

        // A cube at the field centre scales a small planet and lands the run, so the start frame
        // solves in place and no candidate placement makes moving it the job.
        private PuzzleData SolveInPlacePuzzle()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-1f, 0.1f, 0f);
            puzzle.destination = new Vector3(1f, 0.1f, 0f);
            puzzle.zoneCentre = Vector3.zero;
            puzzle.captureRadius = 0.2f;
            puzzle.entropyBudget = 3f;
            puzzle.sources = new[]
            {
                new PuzzleSource { home = Vector3.zero, scale = 0.05f }
            };
            puzzle.zones = 1;
            puzzle.routes = 1;
            puzzle.translatable = true;
            puzzle.cubeSizeMin = 0.1f;
            puzzle.cubeSizeMax = 0.1f;
            puzzle.frames = new[] { new PuzzleFrame { centre = Vector3.zero, size = 0.1f } };
            created.Add(puzzle);
            return puzzle;
        }

        private static PuzzleJobContract SolveInPlaceContract() => new()
        {
            Id = "test", Seed = 1, Attempts = 1, PlanetCount = 1,
            FieldShape = PuzzleFieldShape.Ring,
            HasSun = false, SunOnChord = false, SunRule = PuzzleSunRule.Any, MinBlockers = 0,
            Mechanism = PuzzleMechanism.Any, Zones = 1, MinRoutes = 1, Ships = 1,
            Translatable = true, StartRule = PuzzleStartRule.FieldCentre,
            NoFail = false, NoGrade = false,
        };

        private static PuzzleSimSettings Sim()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            return PuzzleSimSettings.From(settings);
        }

        private static PuzzleZone[] Zone(Vector3 centre, float scale = 1f) => new[]
        {
            new PuzzleZone
            {
                centre = centre,
                halfExtents = new Vector3(0.05f, 0.05f, 0.05f),
                scale = scale,
                falloff = 0.25f,
            },
        };
    }
}
