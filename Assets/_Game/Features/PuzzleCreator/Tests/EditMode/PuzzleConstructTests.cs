using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The skeleton's plain steps: the chord on counted planets, the lesson's move, the corridor, the
    // sun. No simulation here; the verdict has its own test.
    public class PuzzleConstructTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private SpaceShapingSettings settings;
        private PuzzleSeed seed;
        private PuzzleJobContract contract;

        [SetUp]
        public void SetUp()
        {
            settings = AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            contract = PuzzleJobContracts.ById("01_03");
            seed = contract.CreateSeed();
        }

        [TearDown]
        public void TearDown()
        {
            if (seed != null)
                Object.DestroyImmediate(seed);
        }

        [Test]
        public void Build_PlacesTheEndpointsOnCountedPlanets()
        {
            PuzzleConstructedField field = PuzzleConstruct.Build(Request(new NoMoves()));
            Assert.IsNotNull(field);
            Assert.AreEqual(contract.PlanetCount, field.Planets.Length);
            Assert.AreEqual(field.Planets[field.OriginIndex].position, field.Origin);
            Assert.AreEqual(field.Planets[field.DestinationIndex].position, field.Destination);

            float run = Vector3.Distance(field.Origin, field.Destination);
            Assert.GreaterOrEqual(run, settings.runDistance * 0.5f);
            Assert.LessOrEqual(run, settings.runDistance * 1.5f);
        }

        [Test]
        public void Build_KeepsDecoysOutOfTheStartCorridor()
        {
            PuzzleConstructRequest request = Request(new NoMoves());
            PuzzleConstructedField field = PuzzleConstruct.Build(request);
            Assert.IsNotNull(field);
            for (int i = 0; i < field.Planets.Length; i++)
            {
                if (field.Protected[i])
                    continue;

                float margin = request.Brief.EndpointClearance
                    + field.Planets[i].radius * PuzzleRunChoice.PlanetBlockBandFactor;
                float distance = PuzzleRunGeometry.DistanceToSegment(
                    field.Planets[i].position, field.Origin, field.Destination);
                Assert.GreaterOrEqual(distance, margin - 0.0001f, $"decoy {i}");
            }
        }

        [Test]
        public void Build_PutsTheCentreSunAtTheOrigin()
        {
            PuzzleConstructedField field = PuzzleConstruct.Build(Request(new NoMoves()));
            Assert.IsNotNull(field);
            Assert.IsTrue(field.HasSun);
            Assert.AreEqual(Vector3.zero, field.SunHome);
        }

        [Test]
        public void Build_PutsAnOffCorridorSunBeyondTheRim()
        {
            seed.sunPlacement = PuzzleSunPlacement.OffCorridor;
            PuzzleConstructedField field = PuzzleConstruct.Build(Request(new NoMoves()));
            Assert.IsNotNull(field);
            Assert.IsTrue(field.HasSun);

            float rim = 0f;
            float low = float.MaxValue;
            float high = float.MinValue;
            for (int i = 0; i < field.Planets.Length; i++)
            {
                Vector3 planet = field.Planets[i].position;
                rim = Mathf.Max(rim, new Vector2(planet.x, planet.z).magnitude);
                low = Mathf.Min(low, planet.y);
                high = Mathf.Max(high, planet.y);
            }

            float horizontal = new Vector2(field.SunHome.x, field.SunHome.z).magnitude;
            Assert.Greater(horizontal, rim);
            Assert.GreaterOrEqual(field.SunHome.y, low - 0.01f);
            Assert.LessOrEqual(field.SunHome.y, high + 0.01f);
        }

        [Test]
        public void Attempts_ChangeTheField()
        {
            PuzzleConstructedField first = PuzzleConstruct.Build(Request(new NoMoves(), 0));
            PuzzleConstructedField second = PuzzleConstruct.Build(Request(new NoMoves(), 1));
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreNotEqual(first.SeedValue, second.SeedValue);
            CollectionAssert.AreNotEqual(first.Planets, second.Planets);
        }

        [Test]
        public void LessonMove_RepositionsAndProtectsOneDecoy()
        {
            MoveOne lesson = new();
            PuzzleConstructedField field = PuzzleConstruct.Build(Request(lesson));
            Assert.IsNotNull(field);
            Assert.IsTrue(field.Protected[lesson.Index]);
            Assert.AreEqual(lesson.Home, field.Planets[lesson.Index].position);
        }

        [Test]
        public void MoveDecoy_RepositionsTheNearestUnprotectedPlanet()
        {
            PuzzleConstructRequest request = Request(new NoMoves());
            PuzzleConstructedField field = PuzzleConstruct.Build(request);
            PuzzleConstructedField moved = PuzzleConstruct.MoveDecoy(field, request, 1);
            Assert.IsNotNull(moved);

            int changed = -1;
            for (int i = 0; i < field.Planets.Length; i++)
            {
                if (field.Planets[i].position == moved.Planets[i].position)
                    continue;

                changed = i;
                break;
            }

            Assert.GreaterOrEqual(changed, 0);
            Assert.IsFalse(moved.Protected[changed]);
        }

        [Test]
        public void RetrySeed_DoesNotCollideWithTheNextSeed()
        {
            PuzzleSeed next = contract.CreateSeed();
            next.seed = seed.seed + 1;
            try
            {
                PuzzleConstructedField retry = PuzzleConstruct.Build(Request(new NoMoves(), 1));
                PuzzleConstructedField nextField =
                    PuzzleConstruct.Build(RequestFor(next, new NoMoves()));
                Assert.IsNotNull(retry);
                Assert.IsNotNull(nextField);
                CollectionAssert.AreNotEqual(retry.Planets, nextField.Planets);
            }
            finally
            {
                Object.DestroyImmediate(next);
            }
        }

        private PuzzleConstructRequest Request(IPuzzleLesson lesson, int attempt = 0) =>
            RequestFor(seed, lesson, attempt);

        private PuzzleConstructRequest RequestFor(
            PuzzleSeed source, IPuzzleLesson lesson, int attempt = 0)
        {
            return new PuzzleConstructRequest
            {
                Contract = contract,
                Brief = PuzzleBrief.From(source, settings),
                Seed = PuzzleSeedFields.From(source, settings),
                Field = PuzzleFieldSettings.From(settings),
                Sim = PuzzleSimSettings.From(settings),
                Lesson = lesson,
                Attempt = attempt
            };
        }

        private sealed class NoMoves : IPuzzleLesson
        {
            public PuzzleLessonPlan Place(PuzzleLessonContext context) => PuzzleLessonPlan.None;
        }

        private sealed class MoveOne : IPuzzleLesson
        {
            public int Index;
            public Vector3 Home;

            public PuzzleLessonPlan Place(PuzzleLessonContext context)
            {
                Index = (context.OriginIndex + 2) % context.Planets.Length;
                if (Index == context.DestinationIndex)
                    Index = (Index + 1) % context.Planets.Length;

                Home = context.Origin + Vector3.up * 0.02f;
                return new PuzzleLessonPlan(
                    new[] { new PuzzleLessonMove(Index, Home, context.Planets[Index].radius) },
                    new[]
                    {
                        PuzzleZoneCandidates.Zone(
                            Home, context.CubeSizeMin, 1f, context.Settings.ZoneFalloff)
                    },
                    PuzzleSolutionKind.Curve);
            }
        }
    }
}
