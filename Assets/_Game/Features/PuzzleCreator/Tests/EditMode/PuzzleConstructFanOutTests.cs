using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The fan-out steps that need no simulation: the shared-origin picker, the cone, the geometry
    // helpers and the fixture wiring. The aim and the bake live in PuzzleConstructFanOutBakeTests.
    public class PuzzleConstructFanOutTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private SpaceShapingSettings settings;
        private PuzzleJobContract contract;
        private PuzzleSeed seed;

        [SetUp]
        public void SetUp()
        {
            settings = AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            contract = PuzzleJobContracts.ById("02_04C");
            seed = contract.CreateSeed();
        }

        [TearDown]
        public void TearDown()
        {
            if (seed != null)
                Object.DestroyImmediate(seed);
        }

        [Test]
        public void Fixtures_ResolveAsFanOutCurve()
        {
            PuzzleJobContract three = PuzzleJobContracts.ById("02_09C");
            Assert.AreEqual(PuzzleShipRelation.FanOut, contract.Relation);
            Assert.AreEqual(PuzzleMechanism.Curve, contract.Mechanism);
            Assert.AreEqual(PuzzleShipRelation.FanOut, three.Relation);
            Assert.AreEqual(3, three.Ships);
            Assert.AreEqual(0.35f, three.MinScaleDelta);
        }

        [Test]
        public void Choice_SelectsTheFanOutLesson()
        {
            Assert.IsInstanceOf<PuzzleFanOutCurveLesson>(
                PuzzleLessonChoice.For(PuzzleBrief.From(seed, settings)));
        }

        [Test]
        public void Chord_PicksOneSharedOriginAndDistinctDestinations()
        {
            PuzzleConstructRequest request = Request(contract, seed);
            PuzzleFieldPlanet[] planets = Field(request);
            Assert.IsTrue(PuzzleConstructFanOutChord.TryPick(
                planets, request, out PuzzleConstructedShip[] ships, out int bend));

            Assert.AreEqual(contract.Ships, ships.Length);
            for (int i = 0; i < ships.Length; i++)
            {
                Assert.AreEqual(ships[0].OriginIndex, ships[i].OriginIndex, $"origin {i}");
                Assert.AreNotEqual(ships[i].OriginIndex, ships[i].DestinationIndex, $"ship {i}");
                Assert.AreNotEqual(bend, ships[i].OriginIndex, $"bend {i}");
                Assert.AreNotEqual(bend, ships[i].DestinationIndex, $"bend {i}");
                for (int j = i + 1; j < ships.Length; j++)
                    Assert.AreNotEqual(
                        ships[i].DestinationIndex, ships[j].DestinationIndex, $"pair {i}/{j}");
            }
        }

        [Test]
        public void Chord_KeepsTheFanInsideTheCone()
        {
            PuzzleConstructRequest request = Request(contract, seed);
            PuzzleFieldPlanet[] planets = Field(request);
            Assert.IsTrue(PuzzleConstructFanOutChord.TryPick(
                planets, request, out PuzzleConstructedShip[] ships, out _));

            for (int i = 0; i < ships.Length; i++)
            {
                for (int j = i + 1; j < ships.Length; j++)
                {
                    Vector3 a = ships[i].Destination - ships[i].Origin;
                    Vector3 b = ships[j].Destination - ships[j].Origin;
                    Assert.LessOrEqual(Vector3.Angle(a, b),
                        PuzzleConstructFanOutChord.ConeDegrees + 0.001f, $"{i}/{j}");
                }
            }
        }

        [Test]
        public void Chord_PicksABendClearOfTheOrigin()
        {
            PuzzleConstructRequest request = Request(contract, seed);
            PuzzleFieldPlanet[] planets = Field(request);
            Assert.IsTrue(PuzzleConstructFanOutChord.TryPick(
                planets, request, out PuzzleConstructedShip[] ships, out int bend));

            float clearance = request.Brief.EndpointClearance + planets[bend].radius;
            Assert.GreaterOrEqual(
                Vector3.Distance(planets[bend].position, ships[0].Origin),
                clearance - 0.0001f);
        }

        [Test]
        public void ClosestPoint_ReturnsTheNearestPointOnTheRun()
        {
            TrajectoryResult result = Run(new[]
            {
                Vector3.zero, Vector3.right * 2f
            });

            Vector3 closest = PuzzleRunGeometry.ClosestPoint(result, new Vector3(1f, 1f, 0f));
            Assert.AreEqual(new Vector3(1f, 0f, 0f), closest);
        }

        [Test]
        public void SideOf_ReadsEachSideOfTheRun()
        {
            TrajectoryResult result = Run(new[]
            {
                Vector3.zero, Vector3.right * 2f
            });

            Assert.AreEqual(-1, PuzzleRunGeometry.SideOf(result, new Vector3(1f, 0f, 1f)));
            Assert.AreEqual(1, PuzzleRunGeometry.SideOf(result, new Vector3(1f, 0f, -1f)));
        }

        private PuzzleConstructRequest Request(PuzzleJobContract job, PuzzleSeed source)
        {
            PuzzleBrief brief = PuzzleBrief.From(source, settings);
            return new PuzzleConstructRequest
            {
                Contract = job,
                Brief = brief,
                Seed = PuzzleSeedFields.From(source, settings),
                Field = PuzzleFieldSettings.From(settings),
                Sim = PuzzleSimSettings.From(settings),
                Lesson = PuzzleLessonChoice.For(brief)
            };
        }

        private static PuzzleFieldPlanet[] Field(PuzzleConstructRequest request) =>
            PuzzleConstructShape.Generate(
                request, PuzzleConstructSeeds.Value(request.Seed.Seed, 0));

        private static TrajectoryResult Run(Vector3[] points) =>
            new(TrajectoryOutcome.Arrive, new List<Vector3>(points));
    }
}
