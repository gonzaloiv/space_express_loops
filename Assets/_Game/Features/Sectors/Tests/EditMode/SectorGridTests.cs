using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.Sectors.Tests
{
    public class SectorGridTests
    {
        [Test]
        public void CellAt_GroupsPointsIntoTheSameCell()
        {
            Assert.AreEqual(new SectorCell(0, 0, 0), SectorGrid.CellAt(new Vector3(0.1f, 0.1f, 0.1f), 1f));
            Assert.AreEqual(new SectorCell(1, -1, 0), SectorGrid.CellAt(new Vector3(1.2f, -0.1f, 0.5f), 1f));
        }

        [Test]
        public void CellSeed_IsStableAndVariesByCell()
        {
            SectorCell cell = new(2, 3, 4);

            Assert.AreEqual(SectorGrid.CellSeed(7, cell), SectorGrid.CellSeed(7, cell));
            Assert.AreNotEqual(SectorGrid.CellSeed(7, cell), SectorGrid.CellSeed(7, new SectorCell(2, 3, 5)));
            Assert.AreNotEqual(SectorGrid.CellSeed(7, cell), SectorGrid.CellSeed(8, cell));
        }

        [Test]
        public void CellSeed_IsWellDistributed()
        {
            for (int worldSeed = 0; worldSeed < 8; worldSeed++)
            {
                bool[] buckets = new bool[10];
                for (int x = -6; x <= 6; x++)
                {
                    for (int z = -6; z <= 6; z++)
                    {
                        uint seed = unchecked((uint)SectorGrid.CellSeed(worldSeed, new SectorCell(x, 0, z)));
                        buckets[seed % 10u] = true;
                    }
                }

                for (int bucket = 0; bucket < buckets.Length; bucket++)
                    Assert.IsTrue(buckets[bucket], $"worldSeed {worldSeed} never hit bucket {bucket}");
            }
        }

        [Test]
        public void FitsInCell_ChecksDiameterPlusGap()
        {
            Assert.IsTrue(SectorGrid.FitsInCell(2f, 0.5f, 5f));
            Assert.IsFalse(SectorGrid.FitsInCell(3f, 0.5f, 5f));
        }

        [Test]
        public void SectorsOverlap_ComparesRadiusPlusGap()
        {
            Assert.IsFalse(SectorGrid.SectorsOverlap(Vector3.zero, 0.4f, Vector3.right, 0.4f, 0.1f));
            Assert.IsTrue(SectorGrid.SectorsOverlap(Vector3.zero, 0.4f, Vector3.right * 0.5f, 0.4f, 0.1f));
        }
    }
}
