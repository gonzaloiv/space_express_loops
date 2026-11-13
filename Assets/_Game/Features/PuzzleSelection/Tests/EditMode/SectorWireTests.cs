using NUnit.Framework;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class SectorWireTests
    {
        [Test]
        public void Key_FollowsTheSolvedCount()
        {
            Assert.AreEqual("sector_wire_00", SectorWire.Key(0));
            Assert.AreEqual("sector_wire_07", SectorWire.Key(7));
        }

        [Test]
        public void Key_HoldsOnTheLastLinePastTheEnd()
        {
            Assert.AreEqual("sector_wire_11", SectorWire.Key(SectorWire.LineCount - 1));
            Assert.AreEqual("sector_wire_11", SectorWire.Key(40));
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
