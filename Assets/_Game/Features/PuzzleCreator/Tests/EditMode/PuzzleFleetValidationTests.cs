using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleFleetValidationTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        // 02_01 is a two-ship separate fleet that still solves under today's settings. 02_04 no longer
        // solves (no candidate zone rescues all three ships), so it cannot be the accepting fixture.
        private const string FleetPath = "Assets/_Game/Resources/Chapters/02/Job_02_01_Puzzle.asset";

        [Test]
        public void Validate_TwoShipFleet_Passes()
        {
            PuzzleData puzzle = Load();
            PuzzleSimSettings sim = Sim();
            PuzzleJobContract contract = Contract(puzzle);
            Assert.IsTrue(
                PuzzleFleetSolution.TryFind(puzzle, sim, contract.Mechanism, out PuzzleZone zone),
                "no fleet zone");

            Assert.IsTrue(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Other, new[] { zone }, contract,
                    out List<string> failures, sim),
                string.Join("\n", failures));
        }

        [Test]
        public void Validate_TwoShipClearFleet_CurveAnswer_Fails()
        {
            PuzzleData puzzle = Load();
            PuzzleSimSettings sim = Sim();
            PuzzleJobContract contract = Contract(puzzle);
            contract.Mechanism = PuzzleMechanism.Clear;
            Assert.IsTrue(
                PuzzleFleetSolution.TryFind(puzzle, sim, PuzzleMechanism.Any, out PuzzleZone zone),
                "no fleet zone");

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, new[] { zone }, contract,
                    out List<string> failures, sim),
                string.Join("\n", failures));
            Assert.That(failures, Does.Contain("role Curve, expected Clear"));
        }

        [Test]
        public void Validate_ShipThatMisses_Fails()
        {
            PuzzleData puzzle = Load();
            PuzzleSimSettings sim = Sim();
            PuzzleJobContract contract = Contract(puzzle);
            Assert.IsTrue(
                PuzzleFleetSolution.TryFind(puzzle, sim, contract.Mechanism, out PuzzleZone zone),
                "no fleet zone");

            PuzzleData broken = Object.Instantiate(puzzle);
            try
            {
                PuzzleShip ship = broken.ships[broken.ships.Length - 1];
                ship.destination += new Vector3(0.2f, 0.2f, 0f);
                broken.ships[broken.ships.Length - 1] = ship;

                Assert.IsFalse(
                    PuzzleJobValidator.Validate(
                        broken, PuzzleSolutionKind.Other, new[] { zone }, Contract(broken),
                        out List<string> failures, sim),
                    string.Join("\n", failures));
                Assert.That(failures, Does.Contain("the fleet does not solve"));
            }
            finally
            {
                Object.DestroyImmediate(broken);
            }
        }

        private static PuzzleData Load()
        {
            PuzzleData puzzle = AssetDatabase.LoadAssetAtPath<PuzzleData>(FleetPath);
            Assert.IsNotNull(puzzle, FleetPath);
            return puzzle;
        }

        private static PuzzleSimSettings Sim()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            return PuzzleSimSettings.From(settings);
        }

        private static PuzzleJobContract Contract(PuzzleData puzzle)
        {
            int planets = puzzle.sources.Length;
            bool hasSun = false;
            for (int i = 0; i < puzzle.sources.Length; i++)
                hasSun |= puzzle.sources[i].centreOnly;
            if (hasSun)
                planets--;

            return new PuzzleJobContract
            {
                Id = puzzle.id,
                Ships = puzzle.ships.Length,
                Relation = PuzzleShipRelation.Separate,
                HasSun = hasSun,
                SunOnChord = PuzzleJobValidator.SunOnChord(puzzle),
                SunRule = PuzzleSunRule.Any,
                Mechanism = PuzzleMechanism.Any,
                Zones = puzzle.zones,
                MinRoutes = 0,
                Translatable = true,
                StartRule = PuzzleStartRule.RunMidpoint,
                PlanetCount = planets,
                NoFail = false,
                NoGrade = false,
                NoIntro = false
            };
        }
    }
}
