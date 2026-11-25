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

        // 02_05 is the capstone fleet and the first shipped edge-sun fleet, so it is the accepting
        // fixture. 02_01 stays the two-ship negative fixture; 02_04 no longer solves (no candidate
        // zone rescues all three ships).
        private const string FleetPath = "Assets/_Game/Resources/Chapters/02/Job_02_01_Puzzle.asset";
        private const string EdgeFleetPath =
            "Assets/_Game/Resources/Chapters/02/Job_02_05_Puzzle.asset";

        [Test]
        public void Validate_Fleet_Passes()
        {
            PuzzleData puzzle = LoadEdge();
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
                ship.destination += new Vector3(1f, 1f, 0f);
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
