using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The fleet build's plain steps, the shipped Clear fleet through the whole gate, and the bake-heavy
    // spikes/rates (Explicit; run in the bake checkout).
    public class PuzzleConstructFleetTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private const string FleetFieldPath =
            "Assets/_Game/Resources/Chapters/02/Job_02_07_Puzzle.asset";

        private SpaceShapingSettings settings;
        private PuzzleJobContract contract;
        private PuzzleSeed seed;

        [SetUp]
        public void SetUp()
        {
            settings = AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            contract = PuzzleJobContracts.ById("02_07C");
            seed = contract.CreateSeed();
        }

        [TearDown]
        public void TearDown()
        {
            if (seed != null)
                Object.DestroyImmediate(seed);
        }

        [Test]
        public void Build_PicksOneChordPerShipAroundTheHub()
        {
            PuzzleConstructedField field = PuzzleConstruct.Build(Request(contract, seed, new NoMoves()));
            Assert.IsNotNull(field);
            AssertChords(field, contract);
        }

        [Test]
        public void Build_ThreeShips_KeepsEveryEndpointDistinct()
        {
            PuzzleJobContract three = PuzzleJobContracts.ById("02_07C3");
            PuzzleSeed source = three.CreateSeed();
            try
            {
                PuzzleConstructedField field =
                    PuzzleConstruct.Build(Request(three, source, new NoMoves()));
                Assert.IsNotNull(field);
                AssertChords(field, three);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void Build_KeepsDecoysOutOfEveryChordsCorridor()
        {
            PuzzleConstructRequest request = Request(contract, seed, new NoMoves());
            PuzzleConstructedField field = PuzzleConstruct.Build(request);
            Assert.IsNotNull(field);
            for (int p = 0; p < field.Planets.Length; p++)
            {
                if (field.Protected[p])
                    continue;

                float margin = request.Brief.EndpointClearance
                    + field.Planets[p].radius * PuzzleRunChoice.PlanetBlockBandFactor;
                for (int s = 0; s < field.Ships.Length; s++)
                {
                    float distance = PuzzleRunGeometry.DistanceToSegment(
                        field.Planets[p].position, field.Ships[s].Origin, field.Ships[s].Destination);
                    Assert.GreaterOrEqual(distance, margin - 0.0001f, $"decoy {p}, ship {s}");
                }
            }
        }

        [Test]
        public void Build_ContextCarriesEveryShipAndTheHub()
        {
            Capture capture = new();
            PuzzleConstructedField field = PuzzleConstruct.Build(Request(contract, seed, capture));
            Assert.IsNotNull(field);
            Assert.AreEqual(contract.Ships, capture.Ships);
            Assert.AreEqual(field.HubIndex, capture.Hub);
            Assert.Greater(capture.CaptureRadius, 0f);
        }

        [Test]
        public void Build_ProtectsEveryEndpointAndTheHub()
        {
            PuzzleConstructedField field = PuzzleConstruct.Build(Request(contract, seed, new NoMoves()));
            Assert.IsNotNull(field);
            for (int i = 0; i < field.Ships.Length; i++)
            {
                Assert.IsTrue(field.Protected[field.Ships[i].OriginIndex], $"origin {i}");
                Assert.IsTrue(field.Protected[field.Ships[i].DestinationIndex], $"destination {i}");
            }

            Assert.IsTrue(field.Protected[field.HubIndex]);
        }

        [Test]
        public void MoveDecoy_TargetsTheGivenShipsChord()
        {
            PuzzleConstructRequest request = Request(contract, seed, new NoMoves());
            PuzzleConstructedField field = PuzzleConstruct.Build(request);
            Assert.IsNotNull(field);

            PuzzleDecoyField decoys = new()
            {
                Planets = (PuzzleFieldPlanet[])field.Planets.Clone(),
                Protected = field.Protected,
                Request = request,
                Origin = field.Ships[1].Origin,
                Destination = field.Ships[1].Destination,
                Ships = field.Ships
            };
            int nearest = PuzzleConstructDecoys.NearestUnprotected(decoys, 1);
            Assert.GreaterOrEqual(nearest, 0);

            PuzzleConstructedField moved = PuzzleConstruct.MoveDecoy(field, request, 1, 1);
            Assert.IsNotNull(moved);
            Assert.AreNotEqual(
                field.Planets[nearest].position, moved.Planets[nearest].position);
        }

        [Test]
        public void ShippedClearFleet_PassesTheGate()
        {
            PuzzleJobContract shipped = PuzzleJobContracts.ById("02_07");
            PuzzleSeed source = shipped.CreateSeed();
            try
            {
                PuzzleData field = AssetDatabase.LoadAssetAtPath<PuzzleData>(FleetFieldPath);
                Assert.IsNotNull(field, FleetFieldPath);
                PuzzleAnswerAsset answer = LoadAnswer("02_07");

                PuzzleBakeResult result = PuzzleConstructBake.VerifyAnswer(
                    field, answer.zones, Request(shipped, source, null));
                Assert.IsTrue(result.Baked, $"{shipped.Id}: {result.Status}");
                Assert.AreEqual(PuzzleSolutionKind.Clear, result.Kind);
                Object.DestroyImmediate(result.Puzzle);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test, Explicit]
        public void Clear_TwoShips_Spike() => Spike("02_07C");

        [Test, Explicit]
        public void Clear_ThreeShips_Spike() => Spike("02_07C3");

        [Test, Explicit]
        public void Curve_TwoShips_Spike() => Spike("02_03C");

        [Test, Explicit]
        public void Curve_ThreeShips_Spike() => Spike("02_06C");

        [Test, Explicit]
        public void Clear_TwoShips_On20Seeds() => Rate("02_07C");

        [Test, Explicit]
        public void Clear_ThreeShips_On20Seeds() => Rate("02_07C3");

        [Test, Explicit]
        public void Curve_TwoShips_On20Seeds() => Rate("02_03C");

        [Test, Explicit]
        public void Curve_ThreeShips_On20Seeds() => Rate("02_06C");

        private static void Spike(string id)
        {
            PuzzleJobContract job = PuzzleJobContracts.ById(id);
            PuzzleSeed source = job.CreateSeed();
            try
            {
                PuzzleBakeResult result = PuzzleConstructBake.Bake(
                    Request(job, source, null));
                TestContext.Progress.WriteLine(
                    $"{id}: {result.Status} {result.Kind} window {result.Window:F3}");
                Assert.IsTrue(result.Baked, $"{id}: {result.Status}");
                Object.DestroyImmediate(result.Puzzle);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        private static void Rate(string id)
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract job = PuzzleJobContracts.ById(id);
            int passed = 0;
            for (int i = 0; i < 20; i++)
            {
                PuzzleSeed source = job.CreateSeed();
                try
                {
                    source.seed = job.Seed + i;
                    PuzzleBakeResult result = PuzzleConstructBake.Bake(
                        Request(job, source, null, settings));
                    if (!result.Baked)
                    {
                        TestContext.Progress.WriteLine($"{id} fail {result.Status}");
                        continue;
                    }

                    passed++;
                    Object.DestroyImmediate(result.Puzzle);
                }
                finally
                {
                    Object.DestroyImmediate(source);
                }
            }

            TestContext.Progress.WriteLine($"{id}: {passed}/20 constructed");
            Assert.GreaterOrEqual(passed, 12, $"{id}: {passed}/20");
        }

        private static PuzzleConstructRequest Request(
            PuzzleJobContract job, PuzzleSeed source, IPuzzleLesson lesson,
            SpaceShapingSettings settings = null)
        {
            settings ??= LoadSettings();
            PuzzleBrief brief = PuzzleBrief.From(source, settings);
            return new PuzzleConstructRequest
            {
                Contract = job,
                Brief = brief,
                Seed = PuzzleSeedFields.From(source, settings),
                Field = PuzzleFieldSettings.From(settings),
                Sim = PuzzleSimSettings.From(settings),
                Lesson = lesson ?? PuzzleLessonChoice.For(brief),
                Attempt = 0
            };
        }

        private void AssertChords(PuzzleConstructedField field, PuzzleJobContract job)
        {
            Assert.AreEqual(job.Ships, field.Ships.Length);
            Assert.GreaterOrEqual(field.HubIndex, 0);
            Assert.Less(field.HubIndex, field.Planets.Length);

            float band = field.Planets[field.HubIndex].radius
                * PuzzleRunChoice.PlanetBlockBandFactor;
            bool[] used = new bool[field.Planets.Length];
            for (int i = 0; i < field.Ships.Length; i++)
            {
                PuzzleConstructedShip ship = field.Ships[i];
                Assert.AreNotEqual(ship.OriginIndex, ship.DestinationIndex);
                Assert.IsFalse(used[ship.OriginIndex], $"endpoint {i} shared");
                Assert.IsFalse(used[ship.DestinationIndex], $"endpoint {i} shared");
                used[ship.OriginIndex] = true;
                used[ship.DestinationIndex] = true;

                float length = Vector3.Distance(ship.Origin, ship.Destination);
                Assert.GreaterOrEqual(length, settings.runDistance * 0.5f - 0.0001f);
                Assert.LessOrEqual(length, settings.runDistance * 1.5f + 0.0001f);
                Assert.LessOrEqual(
                    PuzzleRunGeometry.DistanceToSegment(
                        field.HubHome, ship.Origin, ship.Destination),
                    band + 0.0001f);
            }

            Assert.IsFalse(used[field.HubIndex]);
        }

        private static PuzzleAnswerAsset LoadAnswer(string id)
        {
            PuzzleAnswerAsset answer =
                AssetDatabase.LoadAssetAtPath<PuzzleAnswerAsset>(PuzzleAnswerAsset.PathFor(id));
            Assert.IsNotNull(answer, PuzzleAnswerAsset.PathFor(id));
            return answer;
        }

        private static SpaceShapingSettings LoadSettings()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            return settings;
        }

        private sealed class NoMoves : IPuzzleLesson
        {
            public PuzzleLessonPlan Place(PuzzleLessonContext context) => PuzzleLessonPlan.None;
        }

        private sealed class Capture : IPuzzleLesson
        {
            public int Ships;
            public int Hub;
            public float CaptureRadius;

            public PuzzleLessonPlan Place(PuzzleLessonContext context)
            {
                Ships = context.Ships.Length;
                Hub = context.HubIndex;
                CaptureRadius = context.CaptureRadius;
                return PuzzleLessonPlan.None;
            }
        }
    }
}
