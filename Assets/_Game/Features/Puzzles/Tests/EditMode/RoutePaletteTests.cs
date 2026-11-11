using NUnit.Framework;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class RoutePaletteTests
    {
        [Test]
        public void StartIndex_LeavesJobOneAtTheFirstEntry()
        {
            Assert.AreEqual(0, RoutePalette.StartIndex("01_01"));
        }

        [Test]
        public void StartIndex_EmptyIdStartsAtTheFirstEntry()
        {
            Assert.AreEqual(0, RoutePalette.StartIndex(string.Empty));
        }

        [Test]
        public void StartIndex_UnparsedIdStartsAtTheFirstEntry()
        {
            Assert.AreEqual(0, RoutePalette.StartIndex("two-ships"));
        }

        [Test]
        public void Entry_TwoShipsTakeNeighbouringEntries()
        {
            int start = RoutePalette.StartIndex("01_03");

            Assert.AreEqual(2, start);
            Assert.AreEqual(2, RoutePalette.Entry(start, 0, 4));
            Assert.AreEqual(3, RoutePalette.Entry(start, 1, 4));
        }

        [Test]
        public void Entry_WrapsPastThePaletteLength()
        {
            Assert.AreEqual(0, RoutePalette.Entry(3, 1, 4));
            Assert.AreEqual(1, RoutePalette.Entry(3, 2, 4));
        }
    }
}
