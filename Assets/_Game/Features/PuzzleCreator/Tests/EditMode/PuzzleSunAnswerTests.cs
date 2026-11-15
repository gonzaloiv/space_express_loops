using NUnit.Framework;
using UnityEditor;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // NoSunAnswer on the shipped jobs: 02_01 is won by shrinking the sun, 01_04 was rebaked so it is not.
    public class PuzzleSunAnswerTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private PuzzleSimSettings sim;

        [OneTimeSetUp]
        public void SetUp()
        {
            SpaceShapingSettings settings = AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            sim = PuzzleSimSettings.From(settings);
        }

        [Test]
        public void SunOnlyJob_SunAnswers() =>
            Assert.IsTrue(PuzzleSunRuleCheck.SunAnswers(Job("02/Job_02_01"), sim));

        [Test]
        public void RebakedJob_HasNoSunAnswer() =>
            Assert.IsFalse(PuzzleSunRuleCheck.SunAnswers(Job("01/Job_01_04"), sim));

        [Test]
        public void Validator_RejectsASunAnswerUnderNoSunAnswer()
        {
            PuzzleJobContract contract = PuzzleJobContracts.ById("01_04");
            PuzzleData sunJob = Job("02/Job_02_01");

            PuzzleJobValidator.Validate(
                sunJob, PuzzleSolutionKind.Curve, null, contract, out var failures, sim);

            Assert.Contains("a cube on the sun solves it", failures);
        }

        [Test]
        public void NoSunAnswer_KeepsTheSunOutOfTheChosenZone()
        {
            PuzzleData job = Job("01/Job_01_04");
            PuzzleZone overSun = PuzzleZoneCandidates.Zone(
                job.sources[0].home, job.frames[0].size, 0.8f, sim.ZoneFalloff);

            Assert.IsFalse(PuzzleSunRuleCheck.Matches(job, new[] { overSun }, PuzzleSunRule.NoSunAnswer));
        }

        private static PuzzleData Job(string name)
        {
            string path = $"Assets/_Game/Resources/Chapters/{name}_Puzzle.asset";
            PuzzleData job = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            Assert.IsNotNull(job, path);
            return job;
        }
    }
}
