using DigitalLove.Casual.Levels;
using NUnit.Framework;
using UnityEditor;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class SectorWireTests
    {
        private const string ChaptersFolder = "Assets/_Game/Resources/Chapters";

        // Chapter 3 has no asset yet: its six planned jobs (chapter-3-jobs U2) are the last term. When
        // Chapters/03.asset lands, read it here and delete this constant.
        private const int PlannedChapterThreeJobs = 6;

        [Test]
        public void Key_ChapterSeamsFollowThePlayOrder()
        {
            int one = ChapterJobs("01");
            int two = ChapterJobs("02");

            Assert.AreEqual($"sector_wire_{one:D2}", SectorWire.Key(one));
            Assert.AreEqual($"sector_wire_{one + 1:D2}", SectorWire.Key(one + 1));
            Assert.AreEqual($"sector_wire_{one + two:D2}", SectorWire.Key(one + two));
            Assert.AreEqual($"sector_wire_{one + two + 1:D2}", SectorWire.Key(one + two + 1));
        }

        [Test]
        public void LineCount_IsTheBootLinePlusEveryChapterJob()
        {
            int expected = 1 + ChapterJobs("01") + ChapterJobs("02") + PlannedChapterThreeJobs;

            Assert.AreEqual(
                expected, SectorWire.LineCount,
                $"set SectorWire.LineCount = {expected} (boot + chapter 1 + chapter 2 + chapter 3 jobs)");
        }

        [Test]
        public void Key_FollowsTheSolvedCount()
        {
            Assert.AreEqual("sector_wire_00", SectorWire.Key(0));
            Assert.AreEqual("sector_wire_03", SectorWire.Key(3));
            Assert.AreEqual("sector_wire_07", SectorWire.Key(7));
        }

        [Test]
        public void Key_HoldsOnTheLastLinePastTheEnd()
        {
            string last = $"sector_wire_{SectorWire.LineCount - 1:D2}";
            Assert.AreEqual(last, SectorWire.Key(SectorWire.LineCount - 1));
            Assert.AreEqual(last, SectorWire.Key(40));
            Assert.AreEqual("sector_wire_00", SectorWire.Key(-1));
        }

        [Test]
        public void SolvedCount_CountsSolvedJobsOnly()
        {
            RunProgress progress = RunProgress.FromSolved(4, 2, new[] { true, false, true, false });

            Assert.AreEqual(2, progress.SolvedCount);
            Assert.AreEqual(0, RunProgress.FromSolved(3, 0, null).SolvedCount);
        }

        private static int ChapterJobs(string id)
        {
            string path = $"{ChaptersFolder}/{id}.asset";
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
            Assert.IsNotNull(chapter, path);
            return chapter.levels.Length;
        }
    }
}
