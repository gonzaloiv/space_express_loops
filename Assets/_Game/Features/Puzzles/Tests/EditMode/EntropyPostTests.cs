using NUnit.Framework;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class EntropyPostTests
    {
        private const float ThreeStars = 1f / 3f;
        private const float TwoStars = 2f / 3f;

        [Test]
        public void FreshBudget_IsAFullPost_AndHoldsEveryStar()
        {
            EntropyPost post = new(0f, 0f, 0f, false);

            Assert.AreEqual(1f, post.RemainingTop, 1e-5f);
            Assert.AreEqual(1f, post.PreviewFrom, 1e-5f);
            Assert.AreEqual(StarHold.Held, post.Star(ThreeStars));
            Assert.AreEqual(StarHold.Held, post.Star(TwoStars));
            Assert.AreEqual(StarHold.Held, post.Star(1f));
        }

        [Test]
        public void SpendingCutsTheTop_AndThePreviewEatsBelowIt()
        {
            EntropyPost post = new(0.25f, 0.2f, 0.1f, false);

            Assert.AreEqual(0.75f, post.RemainingTop, 1e-5f);
            Assert.AreEqual(0.55f, post.PreviewFrom, 1e-5f);
            Assert.AreEqual(0.85f, post.LastEditTop, 1e-5f);
        }

        [Test]
        public void APinchThatCrossesALimit_PutsThatStarAtRisk()
        {
            EntropyPost post = new(0.3f, 0.1f, 0f, false);

            Assert.AreEqual(StarHold.AtRisk, post.Star(ThreeStars));
            Assert.AreEqual(StarHold.Held, post.Star(TwoStars));
        }

        [Test]
        public void SpendingPastALimit_LosesThatStar_ButExactlyOnItHolds()
        {
            Assert.AreEqual(StarHold.Held, new EntropyPost(ThreeStars, 0f, 0f, false).Star(ThreeStars));
            Assert.AreEqual(StarHold.Lost, new EntropyPost(0.4f, 0f, 0f, false).Star(ThreeStars));
        }

        [Test]
        public void Overflow_PutsTheLastStarAtRisk()
        {
            EntropyPost post = new(0.8f, 0.2f, 0f, true);

            Assert.AreEqual(StarHold.AtRisk, post.Star(1f));
            Assert.AreEqual(0f, post.PreviewFrom, 1e-5f);
        }
    }
}
