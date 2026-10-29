using NUnit.Framework;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class PuzzleArrivalTests
    {
        [Test]
        public void Offset_Inside_IsZero()
        {
            Assert.AreEqual(0f, PuzzleArrival.Offset(5f, 4f, 6f), 0.0001f);
        }

        [Test]
        public void Offset_Early_IsNegative()
        {
            Assert.AreEqual(-1.5f, PuzzleArrival.Offset(2.5f, 4f, 6f), 0.0001f);
        }

        [Test]
        public void Offset_Late_IsPositive()
        {
            Assert.AreEqual(1.2f, PuzzleArrival.Offset(7.2f, 4f, 6f), 0.0001f);
        }

        [Test]
        public void Describe_Early_SaysEarlyWithAPlus()
        {
            Assert.AreEqual("+1.5 s early", PuzzleArrival.Describe(-1.5f));
        }

        [Test]
        public void Describe_Late_SaysLate()
        {
            Assert.AreEqual("1.2 s late", PuzzleArrival.Describe(1.2f));
        }

        [Test]
        public void Describe_Inside_SaysInWindow()
        {
            Assert.AreEqual("in window", PuzzleArrival.Describe(0f));
        }
    }
}
