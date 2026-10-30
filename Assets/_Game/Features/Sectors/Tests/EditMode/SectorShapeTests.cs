using DigitalLove.Global;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.Sectors.Tests
{
    public class SectorShapeTests
    {
        private const int Seeds = 24;
        private const float Tolerance = 0.001f;

        [Test]
        public void Generate_PlacesRequestedCount([Values] SectorShapeKind kind)
        {
            SectorShapeData data = Default(kind);
            data.planetCount = new MinMaxInt { min = 5, max = 5 };

            for (int seed = 0; seed < Seeds; seed++)
                Assert.AreEqual(5, SectorShape.Generate(data, new SeededRandom(seed)).PlanetCount);
        }

        [Test]
        public void Generate_KeepsPlanetsInsideSectorAndOutsideAnchor([Values] SectorShapeKind kind)
        {
            SectorShapeData data = Default(kind);

            for (int seed = 0; seed < Seeds; seed++)
            {
                SectorLayout layout = SectorShape.Generate(data, new SeededRandom(seed));
                float outer = layout.SectorRadius * data.spread;
                for (int i = 0; i < layout.PlanetCount; i++)
                {
                    SectorPlanet planet = layout.Planets[i];
                    float distance = planet.Position.magnitude;
                    Assert.GreaterOrEqual(
                        distance,
                        layout.AnchorRadius + planet.Radius - Tolerance,
                        $"{kind} seed {seed} planet {i} overlaps the anchor");
                    Assert.LessOrEqual(
                        distance,
                        outer + Tolerance,
                        $"{kind} seed {seed} planet {i} left the sector");
                }
            }
        }

        [Test]
        public void Generate_KeepsPlanetsApart([Values] SectorShapeKind kind)
        {
            SectorShapeData data = Default(kind);

            for (int seed = 0; seed < Seeds; seed++)
            {
                SectorLayout layout = SectorShape.Generate(data, new SeededRandom(seed));
                for (int i = 0; i < layout.PlanetCount; i++)
                {
                    for (int j = i + 1; j < layout.PlanetCount; j++)
                    {
                        float required = data.minSeparation
                            * (layout.Planets[i].Radius + layout.Planets[j].Radius);
                        float distance = Vector3.Distance(
                            layout.Planets[i].Position,
                            layout.Planets[j].Position);
                        Assert.GreaterOrEqual(
                            distance,
                            required - Tolerance,
                            $"{kind} seed {seed} planets {i} and {j} overlap");
                    }
                }
            }
        }

        [Test]
        public void Generate_UsesPlanetRadiusRange([Values] SectorShapeKind kind)
        {
            SectorShapeData data = Default(kind);

            for (int seed = 0; seed < Seeds; seed++)
            {
                SectorLayout layout = SectorShape.Generate(data, new SeededRandom(seed));
                for (int i = 0; i < layout.PlanetCount; i++)
                {
                    Assert.GreaterOrEqual(layout.Planets[i].Radius, data.planetRadius.min - Tolerance);
                    Assert.LessOrEqual(layout.Planets[i].Radius, data.planetRadius.max + Tolerance);
                }
            }
        }

        [Test]
        public void Generate_IsDeterministic([Values] SectorShapeKind kind)
        {
            SectorShapeData data = Default(kind);
            SectorLayout first = SectorShape.Generate(data, new SeededRandom(1234));
            SectorLayout second = SectorShape.Generate(data, new SeededRandom(1234));

            Assert.AreEqual(first.SectorRadius, second.SectorRadius, Tolerance);
            Assert.AreEqual(first.AnchorRadius, second.AnchorRadius, Tolerance);
            Assert.AreEqual(first.AnchorRotation, second.AnchorRotation);
            Assert.AreEqual(first.PlanetCount, second.PlanetCount);
            for (int i = 0; i < first.PlanetCount; i++)
            {
                Assert.AreEqual(first.Planets[i].Radius, second.Planets[i].Radius, Tolerance);
                Assert.AreEqual(first.Planets[i].Position, second.Planets[i].Position);
            }
        }

        [Test]
        public void Generate_Cluster_HasComparableVerticalAndHorizontalExtent()
        {
            SectorShapeData data = Default(SectorShapeKind.Cluster);
            data.bulge = 0.8f;
            data.planetCount = new MinMaxInt { min = 24, max = 24 };

            for (int seed = 0; seed < Seeds; seed++)
            {
                SectorLayout layout = SectorShape.Generate(data, new SeededRandom(seed));
                float horizontal = 0f;
                float vertical = 0f;
                for (int i = 0; i < layout.PlanetCount; i++)
                {
                    SectorPlanet planet = layout.Planets[i];
                    horizontal = Mathf.Max(
                        horizontal,
                        Mathf.Sqrt(planet.Position.x * planet.Position.x + planet.Position.z * planet.Position.z));
                    vertical = Mathf.Max(vertical, Mathf.Abs(planet.Position.y));
                }

                Assert.Greater(
                    vertical,
                    0.5f * horizontal,
                    $"cluster seed {seed} is flatter than a disc");
            }
        }

        [Test]
        public void Generate_Tilt_IsDeterministic_And_Applied([Values] SectorShapeKind kind)
        {
            SectorShapeData data = Default(kind);
            data.tilt = 12f;
            SectorLayout first = SectorShape.Generate(data, new SeededRandom(1));
            SectorLayout second = SectorShape.Generate(data, new SeededRandom(1));

            Assert.AreEqual(first.AnchorRotation, second.AnchorRotation);
            Assert.AreNotEqual(first.AnchorRotation, Quaternion.identity);
            for (int i = 0; i < first.PlanetCount; i++)
            {
                Assert.AreEqual(first.Planets[i].Position, second.Planets[i].Position);
                Assert.AreEqual(first.Planets[i].Radius, second.Planets[i].Radius, Tolerance);
            }
        }

        [Test]
        public void Generate_RingAndSpiral_HaveCentreBulge([Values] SectorShapeKind kind)
        {
            if (kind == SectorShapeKind.Cluster)
                return;

            SectorShapeData data = Default(kind);
            data.bulge = 0.8f;
            float thinJitter = data.inclination * data.radius.max;

            for (int seed = 0; seed < Seeds; seed++)
            {
                SectorLayout layout = SectorShape.Generate(data, new SeededRandom(seed));
                float maxAbsY = 0f;
                for (int i = 0; i < layout.PlanetCount; i++)
                    maxAbsY = Mathf.Max(maxAbsY, Mathf.Abs(layout.Planets[i].Position.y));

                Assert.Greater(
                    maxAbsY,
                    thinJitter,
                    $"{kind} seed {seed} has no centre bulge");
            }
        }

        [Test]
        public void Generate_RingAndSpiral_VerticalSpreadStaysBounded([Values] SectorShapeKind kind)
        {
            if (kind == SectorShapeKind.Cluster)
                return;

            SectorShapeData data = Default(kind);
            data.bulge = 0.8f;
            float bound = (0.8f + 0.25f) * data.radius.max + Tolerance;

            for (int seed = 0; seed < Seeds; seed++)
            {
                SectorLayout layout = SectorShape.Generate(data, new SeededRandom(seed));
                for (int i = 0; i < layout.PlanetCount; i++)
                    Assert.LessOrEqual(
                        Mathf.Abs(layout.Planets[i].Position.y),
                        bound,
                        $"{kind} seed {seed} planet {i} left the bulge band");
            }
        }

        private static SectorShapeData Default(SectorShapeKind kind) => new() { kind = kind };
    }
}
