using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;

namespace DigitalLove.Game.Persistence.Tests
{
    public class JobOpenCookiesTests
    {
        private readonly ProgressFixtures fixtures = new();

        [TearDown]
        public void TearDown() => fixtures.DestroyAll();

        [Test]
        public void OpenNext_OpensTheFollowingJobOnce()
        {
            LevelSelector selector = fixtures.Selector(
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"), fixtures.Puzzle("01_03"));
            PlayerData playerData = new();
            PuzzleData failed = selector.GetLevelByGlobalIndex<PuzzleData>(1);

            JobOpenResult first = RunProgressCookies.OpenNext(playerData, selector, failed);

            Assert.IsTrue(first.Changed);
            Assert.AreEqual(2, first.SignpostIndex);
            Assert.IsTrue(first.ShowExitPrompt);
            Assert.IsNotNull(playerData.GetCookieById("job_opened_01_03"));

            JobOpenResult again = RunProgressCookies.OpenNext(playerData, selector, failed);

            Assert.IsFalse(again.Changed);
            Assert.AreEqual(2, again.SignpostIndex);
            Assert.IsFalse(again.ShowExitPrompt);
        }

        [Test]
        public void OpenNext_SolvedFollowingJobHasNoSignpost()
        {
            LevelSelector selector = fixtures.Selector(
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"), fixtures.Puzzle("01_03"));
            PlayerData playerData = new();
            fixtures.AddComplete(playerData, "01_03", 2);
            PuzzleData failed = selector.GetLevelByGlobalIndex<PuzzleData>(1);

            JobOpenResult open = RunProgressCookies.OpenNext(playerData, selector, failed);

            Assert.AreEqual(-1, open.SignpostIndex);
        }

        [Test]
        public void OpenNext_UnknownLevelOpensNothing()
        {
            LevelSelector selector = fixtures.Selector(
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"));
            PlayerData playerData = new();

            JobOpenResult open = RunProgressCookies.OpenNext(
                playerData, selector, fixtures.Puzzle("01_09"));

            Assert.IsFalse(open.Changed);
            Assert.AreEqual(-1, open.SignpostIndex);
            Assert.IsFalse(open.ShowExitPrompt);
        }

        [Test]
        public void OpenNext_LastJobOpensNothing()
        {
            LevelSelector selector = fixtures.Selector(
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"));
            PlayerData playerData = new();
            PuzzleData last = selector.GetLevelByGlobalIndex<PuzzleData>(1);

            JobOpenResult open = RunProgressCookies.OpenNext(playerData, selector, last);

            Assert.IsFalse(open.Changed);
            Assert.AreEqual(-1, open.SignpostIndex);
        }

        [Test]
        public void OpenNext_OpenedJobIsCurrentButNotSolved()
        {
            PuzzleData[] levels =
            {
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"), fixtures.Puzzle("01_03")
            };
            LevelSelector selector = fixtures.Selector(levels);
            PlayerData playerData = new();

            RunProgressCookies.OpenNext(playerData, selector, levels[1]);
            RunProgress progress = RunProgressCookies.Build(selector, playerData);
            JobEntry[] entries = RunJobs.FromLevels(levels, progress);

            Assert.AreEqual(JobState.Current, entries[2].State);
            Assert.IsFalse(progress.IsSolved(2));
            Assert.IsFalse(progress.IsComplete);
        }

        [Test]
        public void OpenNext_KeepsJobsPastTheOpenedOneLocked()
        {
            PuzzleData[] levels =
            {
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"), fixtures.Puzzle("01_03"),
                fixtures.Puzzle("01_04")
            };
            LevelSelector selector = fixtures.Selector(levels);
            PlayerData playerData = new();

            RunProgressCookies.OpenNext(playerData, selector, levels[1]);
            RunProgress progress = RunProgressCookies.Build(selector, playerData);

            Assert.AreEqual(JobState.Locked, RunJobs.FromLevels(levels, progress)[3].State);
        }

        [Test]
        public void Build_CleanRunKeepsOnlyTheCurrentJob()
        {
            PuzzleData[] levels =
            {
                fixtures.Puzzle("01_01"), fixtures.Puzzle("01_02"), fixtures.Puzzle("01_03")
            };
            LevelSelector selector = fixtures.Selector(levels);

            RunProgress progress = RunProgressCookies.Build(selector, new PlayerData());
            JobEntry[] entries = RunJobs.FromLevels(levels, progress);

            Assert.AreEqual(JobState.Current, entries[0].State);
            Assert.AreEqual(JobState.Locked, entries[1].State);
            Assert.AreEqual(JobState.Locked, entries[2].State);
        }
    }
}
