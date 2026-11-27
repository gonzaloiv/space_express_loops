using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The backfill's solver must return a reachable answer: a fixed job's answer is its start frame.
    public class PuzzleJobAnswerTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private const string ChaptersFolder = "Assets/_Game/Resources/Chapters";

        [Test]
        public void FixedFleetAnswers_SitOnTheirFrames()
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            CheckFixedAnswer("02_01", settings, sim);
            CheckFixedAnswer("02_02", settings, sim);
        }

        [Test]
        public void FixedSingleAnswers_SitOnTheirFrames()
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            CheckFixedAnswer("01_01", settings, sim);
            CheckFixedAnswer("01_02", settings, sim);
            CheckFixedAnswer("01_03", settings, sim);
        }

        private static SpaceShapingSettings LoadSettings()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            return settings;
        }

        private static void CheckFixedAnswer(
            string id, SpaceShapingSettings settings, PuzzleSimSettings sim)
        {
            PuzzleJobContract contract = PuzzleJobContracts.ById(id);
            PuzzleData puzzle = LoadShipped(id);
            PuzzleJobSolver.AnswerAttempt attempt =
                PuzzleJobSolver.TrySolveAnswer(puzzle, contract, settings, sim);
            Assert.IsTrue(attempt.Found, $"{id}: {string.Join("; ", attempt.Problems)}");

            PuzzleZone answer = attempt.Solution.Zones[0];
            Assert.AreEqual(puzzle.frames[0].centre, answer.centre, id);
            Assert.AreEqual(puzzle.frames[0].size, answer.Width, 0.0001f, id);
        }

        private static PuzzleData LoadShipped(string id)
        {
            string path = $"{ChaptersFolder}/{PuzzleJobContracts.ChapterOf(id)}/Job_{id}_Puzzle.asset";
            PuzzleData puzzle = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            Assert.IsNotNull(puzzle, path);
            return puzzle;
        }
    }
}
