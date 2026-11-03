using NUnit.Framework;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleSeedScannerTests
    {
        [Test]
        public void IsBetter_PrefersTheSeedWithMoreRoutes()
        {
            PuzzleSeedScanResult two = new() { Found = true, Routes = 2 };
            PuzzleSeedScanResult three = new() { Found = true, Routes = 3 };

            Assert.IsTrue(PuzzleSeedScanner.IsBetter(three, two));
            Assert.IsFalse(PuzzleSeedScanner.IsBetter(two, three));
            Assert.IsFalse(PuzzleSeedScanner.IsBetter(two, two));
        }

        [Test]
        public void IsBetter_KeepsTheFirstFoundSeedOverAFailure()
        {
            PuzzleSeedScanResult found = new() { Found = true, Routes = 1 };
            PuzzleSeedScanResult failed = new() { Found = false };

            Assert.IsFalse(PuzzleSeedScanner.IsBetter(failed, found));
            Assert.IsTrue(PuzzleSeedScanner.IsBetter(found, failed));
        }
    }
}
