using DigitalLove.Game.SpaceShaping;
using DigitalLove.Game.Spatial;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The snapshot an off-thread probe runs on must carry every value the synchronous bake reads;
    // these checks lock the projection and prove both constructor paths agree.
    public class FleetFieldTests
    {
        private const float A = 0.15f;
        private const float Radius = 0.08f;
        private const float CubeSize = 0.2f;

        private SpaceShapingSettings settings;
        private PuzzleSimSettings sim;
        private PuzzleBrief brief;
        private PuzzleData field;

        [OneTimeSetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            sim = PuzzleSimSettings.From(settings);
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = 13;
            seed.translatable = true;
            seed.ships = 2;
            brief = PuzzleBrief.From(seed, settings);
            Object.DestroyImmediate(seed);
            field = Field();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(field);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void Snapshot_BuildsTheSameSetup()
        {
            SpaceShapingSetup direct = SpaceShapingSetup.Build(field, sim);
            SpaceShapingSetup snapshot = FleetField.From(field, sim).ToSetup();

            Assert.AreEqual(direct.Origin, snapshot.Origin);
            Assert.AreEqual(direct.Destination, snapshot.Destination);
            Assert.AreEqual(direct.MaxSteps, snapshot.MaxSteps);
            Assert.AreEqual(direct.LaunchSpeed, snapshot.LaunchSpeed, 1e-6f);
            Assert.AreEqual(direct.EntropyBudget, snapshot.EntropyBudget, 1e-6f);
            Assert.AreEqual(direct.NoFail, snapshot.NoFail);
            Assert.AreEqual(direct.NoGrade, snapshot.NoGrade);
            Assert.AreEqual(direct.Sim.CaptureRadius, snapshot.Sim.CaptureRadius, 1e-6f);
            Assert.AreEqual(direct.Sim.ZoneFalloff, snapshot.Sim.ZoneFalloff, 1e-6f);
            AssertSameSources(direct.Sources, snapshot.Sources);
            AssertSameShips(direct.Ships, snapshot.Ships);
        }

        [Test]
        public void Snapshot_FindsTheSameZone()
        {
            FleetZoneResult asset = new PuzzleMultiShipProbe(field, sim, brief).Find();
            FleetZoneResult snapshot =
                new PuzzleMultiShipProbe(FleetField.From(field, sim), brief).Find();

            Assert.AreEqual(asset.Found, snapshot.Found);
            if (!asset.Found)
                return;

            Assert.AreEqual(asset.Zone.centre, snapshot.Zone.centre);
            Assert.AreEqual(asset.Zone.halfExtents, snapshot.Zone.halfExtents);
            Assert.AreEqual(asset.Zone.scale, snapshot.Zone.scale, 1e-6f);
            Assert.AreEqual(asset.Rescued.Count, snapshot.Rescued.Count);
        }

        [Test]
        public void Snapshot_DoesNotAliasTheAsset()
        {
            FleetField snapshot = FleetField.From(field, sim);
            field.sources[0].scale = 0.25f;
            field.ships[0].origin = new Vector3(9f, 9f, 9f);

            Assert.AreEqual(Radius, snapshot.Sources[0].scale, 1e-6f);
            Assert.AreEqual(-A, snapshot.Ships[0].origin.x, 1e-6f);
        }

        private static void AssertSameSources(SourceSpec[] direct, SourceSpec[] snapshot)
        {
            Assert.AreEqual(direct.Length, snapshot.Length);
            for (int i = 0; i < direct.Length; i++)
            {
                Assert.AreEqual(direct[i].Home, snapshot[i].Home);
                Assert.AreEqual(direct[i].InitialScale, snapshot[i].InitialScale, 1e-6f);
                Assert.AreEqual(direct[i].Name, snapshot[i].Name);
                Assert.AreEqual(direct[i].CentreOnly, snapshot[i].CentreOnly);
                Assert.AreEqual(direct[i].FixedMass, snapshot[i].FixedMass);
            }
        }

        private static void AssertSameShips(ShipSpec[] direct, ShipSpec[] snapshot)
        {
            Assert.AreEqual(direct.Length, snapshot.Length);
            for (int i = 0; i < direct.Length; i++)
            {
                Assert.AreEqual(direct[i].Origin, snapshot[i].Origin);
                Assert.AreEqual(direct[i].Destination, snapshot[i].Destination);
                Assert.AreEqual(direct[i].CaptureRadius, snapshot[i].CaptureRadius, 1e-6f);
            }
        }

        private static PuzzleData Field()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-A, 0f, 0f);
            puzzle.destination = new Vector3(A, 0f, 0f);
            puzzle.sources = new[]
            {
                Planet(puzzle.origin, "a"),
                Planet(puzzle.destination, "b"),
                Planet(new Vector3(0f, A, 0f), "c")
            };
            puzzle.ships = new[]
            {
                new PuzzleShip
                {
                    origin = puzzle.origin,
                    destination = puzzle.destination,
                    captureRadius = Radius * 0.5f
                },
                new PuzzleShip
                {
                    origin = puzzle.destination,
                    destination = new Vector3(0f, A, 0f),
                    captureRadius = Radius * 0.5f
                }
            };
            puzzle.cubeSizeMin = CubeSize;
            puzzle.cubeSizeMax = CubeSize;
            puzzle.captureRadius = Radius * 0.5f;
            puzzle.translatable = true;
            puzzle.zones = 1;
            puzzle.entropyBudget = 2f;
            return puzzle;
        }

        private static PuzzleSource Planet(Vector3 home, string name) => new()
        {
            home = home,
            scale = Radius,
            name = name
        };
    }
}
