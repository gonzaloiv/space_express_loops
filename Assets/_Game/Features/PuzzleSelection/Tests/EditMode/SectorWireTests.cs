using NUnit.Framework;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class SectorWireTests
    {
        private const int ChapterOneJobs = 7;
        private const int ChapterTwoJobs = 8;
        private const int ChapterThreeJobs = 6;

        [Test]
        public void Key_ChapterSeamsFollowThePlayOrder()
        {
            Assert.AreEqual("sector_wire_07", SectorWire.Key(ChapterOneJobs));
            Assert.AreEqual("sector_wire_08", SectorWire.Key(ChapterOneJobs + 1));
            Assert.AreEqual("sector_wire_15", SectorWire.Key(ChapterOneJobs + ChapterTwoJobs));
            Assert.AreEqual("sector_wire_16",
                SectorWire.Key(ChapterOneJobs + ChapterTwoJobs + 1));
            Assert.AreEqual(1 + ChapterOneJobs + ChapterTwoJobs + ChapterThreeJobs,
                SectorWire.LineCount);
        }

        [Test]
        public void Key_FollowsTheSolvedCount()
        {
            Assert.AreEqual("sector_wire_00", SectorWire.Key(0));
            Assert.AreEqual("sector_wire_07", SectorWire.Key(7));
            Assert.AreEqual("sector_wire_18", SectorWire.Key(18));
        }

        [Test]
        public void Key_HoldsOnTheLastLinePastTheEnd()
        {
            Assert.AreEqual("sector_wire_21", SectorWire.Key(SectorWire.LineCount - 1));
            Assert.AreEqual("sector_wire_21", SectorWire.Key(40));
            Assert.AreEqual("sector_wire_00", SectorWire.Key(-1));
        }

        [Test]
        public void SolvedCount_CountsSolvedJobsOnly()
        {
            RunProgress progress = RunProgress.FromSolved(4, 2, new[] { true, false, true, false });

            Assert.AreEqual(2, progress.SolvedCount);
            Assert.AreEqual(0, RunProgress.FromSolved(3, 0, null).SolvedCount);
        }
    }
}
