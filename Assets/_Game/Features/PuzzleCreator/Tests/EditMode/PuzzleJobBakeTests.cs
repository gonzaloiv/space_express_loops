using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleJobBakeTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        [Test]
        [Explicit]
        public void Tutorial_FourPlanetRing_BakesCurve()
        {
            PuzzleJobContract contract = PuzzleJobContracts.ById("01_01");
            PuzzleSeedScanResult result = Bake(
                contract, out PuzzleSeed seed, out SpaceShapingSettings settings);

            Assert.IsTrue(
                result.Found,
                $"{contract.Id}: {result.Status} [{result.RunReport}]");
            Assert.AreEqual(PuzzleSolutionKind.Curve, result.Kind);
            PuzzleVerification check = PuzzleVerifier.Verify(
                result.Puzzle, result.Solution, PuzzleSimSettings.From(settings));
            Assert.AreEqual(TrajectoryOutcome.Crash, check.StartOutcome);

            Cleanup(result, seed);
        }

        [Test]
        [Explicit]
        public void Centre_SunCentredGrown()
        {
            PuzzleJobContract contract = PuzzleJobContracts.ById("01_02");
            PuzzleSeedScanResult result = Bake(contract, out PuzzleSeed seed, out _);

            Assert.IsTrue(
                result.Found,
                $"{contract.Id}: {result.Status} [{result.RunReport}]");
            Assert.IsTrue(
                PuzzleSunRuleCheck.Matches(result.Puzzle, result.Solution, PuzzleSunRule.CentredGrown),
                $"{contract.Id}: the solved zone is not a centred, grown star");

            Cleanup(result, seed);
        }

        [Test]
        [Explicit]
        public void FixedJobs_KeepTheSunOutOfTheZones()
        {
            for (int i = 0; i < PuzzleJobContracts.Enabled.Count; i++)
            {
                PuzzleJobContract contract = PuzzleJobContracts.Enabled[i];
                if (contract.Translatable || contract.SunRule != PuzzleSunRule.OutOfZone)
                    continue;
                PuzzleSeedScanResult result = Bake(contract, out PuzzleSeed seed, out _);
                Assert.IsTrue(
                result.Found,
                $"{contract.Id}: {result.Status} [{result.RunReport}]");
                Assert.IsTrue(
                    PuzzleSunRuleCheck.Matches(result.Puzzle, result.Solution, PuzzleSunRule.OutOfZone),
                    $"{contract.Id}: a solved zone contains the sun");
                Cleanup(result, seed);
            }
        }

        [Test]
        [Explicit]
        public void JobSet_MeetsTheContract()
        {
            List<string> failures = new List<string>();
            for (int i = 0; i < PuzzleJobContracts.Enabled.Count; i++)
            {
                PuzzleJobContract contract = PuzzleJobContracts.Enabled[i];
                PuzzleSeedScanResult result = Bake(contract, out PuzzleSeed seed, out _);
                if (!result.Found)
                {
                    failures.Add(
                        $"{contract.Id}: {result.Status} seed {result.Seed} " +
                        $"attempts {result.Attempts} [{result.RunReport}]");
                }
                else if (!PuzzleJobValidator.Validate(
                        result.Puzzle, result.Kind, result.Solution, contract, out List<string> problems))
                {
                    failures.Add($"{contract.Id}: {string.Join("; ", problems)}");
                }

                Cleanup(result, seed);
            }

            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }

        private static PuzzleSeedScanResult Bake(
            PuzzleJobContract contract, out PuzzleSeed seed, out SpaceShapingSettings settings)
        {
            settings = AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            seed = Seed(contract);
            return PuzzleSeedScanner.Find(seed, settings, contract.Attempts);
        }

        private static PuzzleSeed Seed(PuzzleJobContract contract)
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = contract.Seed;
            seed.planetCount = contract.PlanetCount;
            seed.fieldShape = contract.FieldShape;
            seed.hasSun = contract.HasSun;
            seed.sunRule = contract.SunRule;
            seed.sunOnChord = contract.SunOnChord;
            seed.mechanism = contract.Mechanism;
            seed.zones = contract.Zones;
            seed.minRoutes = contract.MinRoutes;
            seed.translatable = contract.Translatable;
            seed.startRule = contract.StartRule;
            seed.noFail = contract.NoFail;
            seed.noGrade = contract.NoGrade;
            return seed;
        }

        private static void Cleanup(PuzzleSeedScanResult result, PuzzleSeed seed)
        {
            if (result.Puzzle != null)
                Object.DestroyImmediate(result.Puzzle);
            Object.DestroyImmediate(seed);
        }
    }
}
