using NUnit.Framework;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class PuzzleGradeTests
    {
        private const float Reference = 1f;
        private const float Budget = Reference * PuzzleGrade.BudgetFactor;

        [Test]
        public void ReferenceSpend_GradesThreeStars()
        {
            Assert.AreEqual(3, PuzzleGrade.Stars(Reference, Budget));
        }

        [Test]
        public void OneAndAHalfTimesTheReference_StillGradesThreeStars()
        {
            Assert.AreEqual(3, PuzzleGrade.Stars(Reference * 1.5f, Budget));
        }

        [Test]
        public void JustPastTheThreeStarBand_GradesTwoStars()
        {
            Assert.AreEqual(2, PuzzleGrade.Stars(Budget * PuzzleGrade.ThreeStarUse + 0.001f, Budget));
        }

        [Test]
        public void TheTwoStarBand_EndsAtTwoStarUse()
        {
            Assert.AreEqual(2, PuzzleGrade.Stars(Budget * PuzzleGrade.TwoStarUse, Budget));
            Assert.AreEqual(1, PuzzleGrade.Stars(Budget * PuzzleGrade.TwoStarUse + 0.001f, Budget));
            Assert.AreEqual(1, PuzzleGrade.Stars(Budget, Budget));
        }
    }
}
