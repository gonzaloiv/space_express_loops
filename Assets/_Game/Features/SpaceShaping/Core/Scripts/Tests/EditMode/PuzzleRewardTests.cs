using NUnit.Framework;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class PuzzleRewardTests
    {
        [Test]
        public void Tier_IsDistinctPerGrade()
        {
            Assert.AreEqual("DELIVERED", PuzzleReward.Tier(1));
            Assert.AreEqual("CLEAN", PuzzleReward.Tier(2));
            Assert.AreEqual("PERFECT", PuzzleReward.Tier(3));
        }

        [Test]
        public void Describe_ShowsStarsAndTier()
        {
            Assert.AreEqual("★★ CLEAN", PuzzleReward.Describe(2));
            Assert.AreEqual("★★★ PERFECT", PuzzleReward.Describe(3));
        }

        [Test]
        public void Describe_WithoutAGrade_IsBlank()
        {
            Assert.AreEqual("-", PuzzleReward.Describe(0));
        }
    }
}
