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

        private const string ChaptersFolder = "Assets/_Game/Resources/Chapters";

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
                PuzzleSeedScanResult result = Bake(
                    contract, out PuzzleSeed seed, out SpaceShapingSettings settings);
                if (!result.Found)
                {
                    failures.Add(
                        $"{contract.Id}: {result.Status} seed {result.Seed} " +
                        $"attempts {result.Attempts} [{result.RunReport}]");
                }
                else if (!PuzzleJobValidator.Validate(
                        result.Puzzle, result.Kind, result.Solution, contract, out List<string> problems,
                        PuzzleSimSettings.From(settings)))
                {
                    failures.Add($"{contract.Id}: {string.Join("; ", problems)}");
                }

                Cleanup(result, seed);
            }

            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }

        // The shipped assets are the content: each one must validate against its own contract. Nothing
        // stores a job's answer or kind, so the test re-solves the asset before validating it.
        [Test]
        public void ShippedJobs_MeetTheContract()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            List<string> failures = new List<string>();
            for (int i = 0; i < PuzzleJobContracts.Enabled.Count; i++)
            {
                PuzzleJobContract contract = PuzzleJobContracts.Enabled[i];
                PuzzleData puzzle = LoadShipped(contract, failures);
                if (puzzle == null)
                    continue;

                if (!TrySolve(puzzle, contract, settings, sim, out PuzzleSolution solution))
                {
                    failures.Add($"{contract.Id}: no solution found");
                    continue;
                }

                if (!PuzzleJobValidator.Validate(
                        puzzle, solution.Kind, solution.Zones, contract, out List<string> problems, sim,
                        checkScramble: false))
                    failures.Add($"{contract.Id}: {string.Join("; ", problems)}");
            }

            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }

        private static PuzzleData LoadShipped(PuzzleJobContract contract, List<string> failures)
        {
            string chapter = PuzzleJobContracts.ChapterOf(contract.Id);
            string path = $"{ChaptersFolder}/{chapter}/Job_{contract.Id}_Puzzle.asset";
            PuzzleData puzzle = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            if (puzzle == null)
                failures.Add($"{contract.Id}: missing {path}");
            return puzzle;
        }

        private static bool TrySolve(
            PuzzleData puzzle, PuzzleJobContract contract, SpaceShapingSettings settings,
            PuzzleSimSettings sim, out PuzzleSolution solution)
        {
            if (contract.Ships > 1)
                return TrySolveFleet(puzzle, contract, sim, out solution);

            return PuzzleSolutionSearch.TrySelect(
                puzzle, sim, Brief(contract, settings), out solution);
        }

        private static bool TrySolveFleet(
            PuzzleData puzzle, PuzzleJobContract contract, PuzzleSimSettings sim,
            out PuzzleSolution solution)
        {
            solution = default;
            if (!PuzzleFleetSolution.TryFind(puzzle, sim, contract.Mechanism, out PuzzleZone zone))
                return false;

            PuzzleVerification check = PuzzleVerifier.Verify(puzzle, new[] { zone }, sim);
            solution = new PuzzleSolution
            {
                Zones = new[] { zone },
                EntropySpent = check.EntropySpent,
                Kind = check.Kind
            };
            return true;
        }

        private static PuzzleBrief Brief(PuzzleJobContract contract, SpaceShapingSettings settings)
        {
            PuzzleSeed seed = contract.CreateSeed();
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            Object.DestroyImmediate(seed);
            return brief;
        }

        private static PuzzleSeedScanResult Bake(
            PuzzleJobContract contract, out PuzzleSeed seed, out SpaceShapingSettings settings)
        {
            settings = AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            seed = contract.CreateSeed();
            return PuzzleSeedScanner.Find(seed, settings, contract.Attempts);
        }

        private static void Cleanup(PuzzleSeedScanResult result, PuzzleSeed seed)
        {
            if (result.Puzzle != null)
                Object.DestroyImmediate(result.Puzzle);
            Object.DestroyImmediate(seed);
        }
    }
}
