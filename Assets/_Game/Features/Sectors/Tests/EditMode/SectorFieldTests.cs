using DigitalLove.Global;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.Sectors.Tests
{
    public class SectorFieldTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void Generate_StaysWithinBudgets()
        {
            for (int seed = 0; seed < 16; seed++)
            {
                SectorFieldData data = Default();
                data.worldSeed = seed;
                SectorPlacement[] sectors = SectorField.Generate(data);

                Assert.LessOrEqual(sectors.Length, data.softSectorBudget);

                int planets = 0;
                for (int i = 0; i < sectors.Length; i++)
                    planets += sectors[i].PlanetCount;

                Assert.LessOrEqual(planets, data.softPlanetBudget);
            }
        }

        [Test]
        public void Generate_PlacesSectorsOnCellCentersInsideTheField()
        {
            SectorFieldData data = Default();
            SectorPlacement[] sectors = SectorField.Generate(data);

            for (int i = 0; i < sectors.Length; i++)
            {
                Vector3 center = sectors[i].Center;
                Assert.AreEqual(0f, Mathf.Repeat(center.x, data.cellSize), Tolerance);
                Assert.AreEqual(0f, Mathf.Repeat(center.z, data.cellSize), Tolerance);
                Assert.LessOrEqual(Mathf.Abs(center.x), data.fieldRadius + Tolerance);
                Assert.LessOrEqual(Mathf.Abs(center.z), data.fieldRadius + Tolerance);
            }
        }

        [Test]
        public void Generate_KeepsSectorsApart()
        {
            SectorFieldData data = Default();
            SectorPlacement[] sectors = SectorField.Generate(data);

            for (int i = 0; i < sectors.Length; i++)
            {
                for (int j = i + 1; j < sectors.Length; j++)
                {
                    Assert.IsFalse(
                        SectorGrid.SectorsOverlap(
                            sectors[i].Center,
                            sectors[i].Layout.SectorRadius,
                            sectors[j].Center,
                            sectors[j].Layout.SectorRadius,
                            data.gap),
                        $"sectors {i} and {j} overlap");
                }
            }
        }

        [Test]
        public void Generate_UsesDensity()
        {
            SectorFieldData empty = Default();
            empty.density = 0f;
            Assert.AreEqual(0, SectorField.Generate(empty).Length);

            SectorFieldData full = Default();
            full.density = 1f;
            full.softPlanetBudget = 1000;
            Assert.AreEqual(full.softSectorBudget, SectorField.Generate(full).Length);
        }

        [Test]
        public void Generate_ProducesSectorsAtDefaultDensity()
        {
            for (int seed = 0; seed < 16; seed++)
            {
                SectorFieldData data = Default();
                data.worldSeed = seed;
                int count = SectorField.Generate(data).Length;

                Assert.Greater(count, 0, $"worldSeed {seed} produced an empty field");
                Assert.LessOrEqual(count, data.softSectorBudget);
            }
        }

        [Test]
        public void Generate_IsDeterministic()
        {
            SectorFieldData data = Default();
            SectorPlacement[] first = SectorField.Generate(data);
            SectorPlacement[] second = SectorField.Generate(data);

            Assert.AreEqual(first.Length, second.Length);
            for (int i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i].Center, second[i].Center);
                Assert.AreEqual(first[i].Cell, second[i].Cell);
                Assert.AreEqual(first[i].PlanetCount, second[i].PlanetCount);
                for (int p = 0; p < first[i].PlanetCount; p++)
                    Assert.AreEqual(first[i].Layout.Planets[p].Position, second[i].Layout.Planets[p].Position);
            }
        }

        [Test]
        public void Generate_KeepsPlanetRadiiInRange()
        {
            SectorFieldData data = Default();
            SectorPlacement[] sectors = SectorField.Generate(data);

            for (int i = 0; i < sectors.Length; i++)
            {
                for (int p = 0; p < sectors[i].PlanetCount; p++)
                {
                    float radius = sectors[i].Layout.Planets[p].Radius;
                    Assert.GreaterOrEqual(radius, data.shape.planetRadius.min - Tolerance);
                    Assert.LessOrEqual(radius, data.shape.planetRadius.max + Tolerance);
                }
            }
        }

        [Test]
        public void FitsInCells_ChecksSectorDiameterPlusGap()
        {
            SectorFieldData data = Default();
            Assert.IsTrue(SectorField.FitsInCells(data));

            data.cellSize = 1f;
            Assert.IsFalse(SectorField.FitsInCells(data));
        }

        private static SectorFieldData Default() => new();
    }
}
