using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;

namespace DigitalLove.Game.Persistence.Tests
{
    public class RunProgressCookiesTests
    {
        private readonly ProgressFixtures fixtures = new();

        [TearDown]
        public void TearDown() => fixtures.DestroyAll();

        [Test]
        public void Build_UsesCompletedCookiesAndScores()
        {
            LevelSelector selector = fixtures.Selector(
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"), fixtures.Puzzle("01_03"));
            PlayerData playerData = new();
            fixtures.AddComplete(playerData, "01_01", 3);

            RunProgress progress = RunProgressCookies.Build(selector, playerData);

            Assert.IsTrue(progress.IsSolved(0));
            Assert.AreEqual(3, progress.Stars(0));
            Assert.IsFalse(progress.IsSolved(1));
            Assert.AreEqual(1, progress.CurrentIndex);
            Assert.IsFalse(progress.IsFresh);
        }

        [Test]
        public void Build_FreshPlayerStartsAtTheFirstJob()
        {
            LevelSelector selector = fixtures.Selector(
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"));

            RunProgress progress = RunProgressCookies.Build(selector, new PlayerData());

            Assert.IsTrue(progress.IsFresh);
            Assert.AreEqual(0, progress.CurrentIndex);
            Assert.IsFalse(progress.IsSolved(0));
        }

        [Test]
        public void Build_AllCompleteMarksEveryJobSolved()
        {
            PuzzleData[] levels = { fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02") };
            LevelSelector selector = fixtures.Selector(levels);
            PlayerData playerData = new();
            fixtures.AddComplete(playerData, "01_01", 2);
            fixtures.AddComplete(playerData, "01_02", 1);

            RunProgress progress = RunProgressCookies.Build(selector, playerData);
            JobEntry[] entries = RunJobs.FromLevels(levels, progress);

            Assert.AreEqual(JobState.Solved, entries[0].State);
            Assert.AreEqual(JobState.Solved, entries[1].State);
        }

        [Test]
        public void OpenChapter_IsTrueOnlyTheFirstTime()
        {
            PlayerData playerData = new();

            Assert.IsTrue(RunProgressCookies.OpenChapter(playerData, "02"));
            Assert.IsFalse(RunProgressCookies.OpenChapter(playerData, "02"));
            Assert.IsTrue(RunProgressCookies.OpenChapter(playerData, "03"));
        }

        [Test]
        public void SeeCoverage_IsNewOnlyWhenTheCountGrows()
        {
            PlayerData playerData = new();

            Assert.IsFalse(RunProgressCookies.SeeCoverage(playerData, 0));
            Assert.IsTrue(RunProgressCookies.SeeCoverage(playerData, 1));
            Assert.IsFalse(RunProgressCookies.SeeCoverage(playerData, 1));
            Assert.IsTrue(RunProgressCookies.SeeCoverage(playerData, 2));
        }

        [Test]
        public void SeeCoverage_FirstVisitWithProgressTypesIn()
        {
            PlayerData playerData = new();

            Assert.IsTrue(RunProgressCookies.SeeCoverage(playerData, 5));
        }

        [Test]
        public void RecordSolve_AddsCookieWithGrade()
        {
            PlayerData playerData = new();

            bool changed = RunProgressCookies.RecordSolve(playerData, "01_01", 2);

            Assert.IsTrue(changed);
            Assert.IsNotNull(playerData.GetCookieById("level_complete_01_01"));
            Assert.AreEqual(2, fixtures.Score(playerData, "01_01"));
        }

        [Test]
        public void RecordSolve_KeepsTheBestGrade()
        {
            PlayerData playerData = new();
            fixtures.AddComplete(playerData, "01_03", 2);

            Assert.IsFalse(RunProgressCookies.RecordSolve(playerData, "01_03", 1));
            Assert.IsFalse(RunProgressCookies.RecordSolve(playerData, "01_03", 2));
            Assert.AreEqual(2, fixtures.Score(playerData, "01_03"));
            Assert.IsTrue(RunProgressCookies.RecordSolve(playerData, "01_03", 3));
            Assert.AreEqual(3, fixtures.Score(playerData, "01_03"));
            Assert.AreEqual(1, playerData.cookies.Count);
        }

        [Test]
        public void RecordSolve_UngradedSolveStillMarksTheJob()
        {
            LevelSelector selector = fixtures.Selector(fixtures.Puzzle("01_01"));
            PlayerData playerData = new();

            RunProgressCookies.RecordSolve(playerData, "01_01", 0);
            RunProgress progress = RunProgressCookies.Build(selector, playerData);

            Assert.IsTrue(progress.IsSolved(0));
            Assert.AreEqual(0, progress.Stars(0));
        }

        [Test]
        public void EndingFlag_RoundTripsTheRunCount()
        {
            PlayerData playerData = new();

            Assert.IsFalse(RunProgressCookies.HasSeenEnding(playerData, 7));
            RunProgressCookies.MarkEndingSeen(playerData, 7);

            Assert.IsTrue(RunProgressCookies.HasSeenEnding(playerData, 7));
            Assert.IsTrue(RunProgressCookies.HasSeenEnding(playerData, 6));
            Assert.IsFalse(RunProgressCookies.HasSeenEnding(playerData, 11));
        }

        [Test]
        public void EndingFlag_DoesNotCountAsACompletedJob()
        {
            PlayerData playerData = new();

            RunProgressCookies.MarkEndingSeen(playerData, 7);

            Assert.AreEqual(0, playerData.CompletedJobsCount);
            Assert.AreEqual(0, playerData.GetLevelCompleteCookies().Count);
        }

        [Test]
        public void EndingFlag_MalformedMetadataIsNotSeen()
        {
            PlayerData playerData = new();
            playerData.cookies.Add(
                new Cookie("run_ending_seen").SetMetadata("{not json"));

            Assert.IsFalse(RunProgressCookies.HasSeenEnding(playerData, 7));
        }
    }
}
