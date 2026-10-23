using DigitalLove.Global;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.Sectors.Tests
{
    public class SectorSelectorTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void SelectDestinations_IsDeterministic()
        {
            SectorFieldData field = Default();
            SectorSelection[] first = SectorSelector.SelectDestinations(field, 7, 0, 5);
            SectorSelection[] second = SectorSelector.SelectDestinations(field, 7, 0, 5);

            Assert.AreEqual(first.Length, second.Length);
            Assert.Greater(first.Length, 0);
            for (int i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i].Position, second[i].Position);
                Assert.AreEqual(first[i].Radius, second[i].Radius, Tolerance);
                Assert.AreEqual(first[i].VariantSeed, second[i].VariantSeed);
                Assert.AreEqual(first[i].Cell, second[i].Cell);
            }
        }

        [Test]
        public void SelectDestinations_ClampsToAvailableSectors()
        {
            SectorFieldData field = Default();
            int available = SectorField.Generate(field).Length;

            Assert.AreEqual(available, SectorSelector.SelectDestinations(field, 3, 0, 1000).Length);
            Assert.AreEqual(0, SectorSelector.SelectDestinations(field, 3, 0, 0).Length);
        }

        [Test]
        public void SelectDestinations_UsesDistinctSectors()
        {
            SectorFieldData field = Default();
            SectorSelection[] selections = SectorSelector.SelectDestinations(field, 11, 0, 8);

            for (int i = 0; i < selections.Length; i++)
            {
                for (int j = i + 1; j < selections.Length; j++)
                    Assert.AreNotEqual(selections[i].Cell, selections[j].Cell, $"sectors {i} and {j} repeat");
            }
        }

        [Test]
        public void SelectDestinations_ReturnsPlanetsOfTheChosenSector()
        {
            SectorFieldData field = Default();
            SectorSelection[] selections = SectorSelector.SelectDestinations(field, 5, 0, 6);

            for (int i = 0; i < selections.Length; i++)
            {
                SectorSelection selection = selections[i];
                SectorLayout layout = selection.Placement.Layout;
                Assert.GreaterOrEqual(selection.PlanetIndex, 0);
                Assert.Less(selection.PlanetIndex, layout.PlanetCount);

                SectorPlanet planet = layout.Planets[selection.PlanetIndex];
                Assert.AreEqual(selection.Placement.Center + planet.Position, selection.Position);
                Assert.AreEqual(planet.Radius, selection.Radius, Tolerance);
            }
        }

        [Test]
        public void SelectDestinations_OffsetTakesSuccessiveSectors()
        {
            SectorFieldData field = Default();
            int available = SectorField.Generate(field).Length;
            Assert.Greater(available, 3);

            SectorSelection[] all = SectorSelector.SelectDestinations(field, 9, 0, available);
            SectorSelection[] later = SectorSelector.SelectDestinations(field, 9, 2, 2);

            Assert.AreEqual(2, later.Length);
            Assert.AreEqual(all[2].Cell, later[0].Cell);
            Assert.AreEqual(all[3].Cell, later[1].Cell);

            Assert.AreEqual(0, SectorSelector.SelectDestinations(field, 9, available, 4).Length);
            Assert.AreEqual(1, SectorSelector.SelectDestinations(field, 9, available - 1, 4).Length);
        }

        [Test]
        public void SelectDestinations_RespectsDestinationRadius()
        {
            SectorFieldData field = Small();
            SectorSelection[] all = SectorSelector.SelectDestinations(field, 4, 0, 1000);

            Assert.Greater(all.Length, 1);
            for (int i = 0; i < all.Length; i++)
                Assert.LessOrEqual(
                    all[i].Placement.Center.magnitude,
                    field.destinationRadius + Tolerance,
                    $"selection {i} is outside the destination radius");

            field.destinationRadius = 0.01f;
            SectorSelection[] core = SectorSelector.SelectDestinations(field, 4, 0, 1000);
            Assert.LessOrEqual(core.Length, 1, "only the central sector should qualify");
        }

        [Test]
        public void SelectDestinations_VariesByLevelSeed()
        {
            SectorFieldData field = Default();
            SectorSelection[] reference = SectorSelector.SelectDestinations(field, 0, 0, 4);

            bool differs = false;
            for (int levelSeed = 1; levelSeed < 8 && !differs; levelSeed++)
            {
                SectorSelection[] other = SectorSelector.SelectDestinations(field, levelSeed, 0, 4);
                for (int i = 0; i < reference.Length && !differs; i++)
                    differs = !reference[i].Cell.Equals(other[i].Cell);
            }

            Assert.IsTrue(differs, "level seed did not change the selection");
        }

        [Test]
        public void TrySelectHub_PicksTheMostCentralSector()
        {
            SectorFieldData field = Default();
            Assert.IsTrue(SectorSelector.TrySelectHub(field, 2, out SectorSelection hub));
            Assert.IsTrue(hub.IsAnchor);
            Assert.AreEqual(-1, hub.PlanetIndex);

            SectorPlacement[] placements = SectorField.Generate(field);
            float nearest = float.MaxValue;
            for (int i = 0; i < placements.Length; i++)
                nearest = Mathf.Min(nearest, placements[i].Center.sqrMagnitude);

            Assert.AreEqual(nearest, hub.Placement.Center.sqrMagnitude, Tolerance);
            Assert.AreEqual(hub.Placement.Layout.AnchorRadius, hub.Radius, Tolerance);
        }

        [Test]
        public void TrySelectHub_FailsOnAnEmptyField()
        {
            SectorFieldData field = Default();
            field.density = 0f;

            Assert.IsFalse(SectorSelector.TrySelectHub(field, 1, out _));
        }

        private static SectorFieldData Default() => new();

        private static SectorFieldData Small() => new()
        {
            cellSize = 0.4f,
            fieldRadius = 0.6f,
            gap = 0.05f,
            destinationRadius = 0.6f,
            shape = new SectorShapeData
            {
                radius = new MinMaxFloat(0.12f, 0.16f),
                planetRadius = new MinMaxFloat(0.02f, 0.03f)
            }
        };
    }
}
