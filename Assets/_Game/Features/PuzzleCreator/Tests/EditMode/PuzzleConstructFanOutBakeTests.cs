using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The fan-out's aim and the bake-heavy checks. Explicit: run in the bake checkout with
    // -testFilter PuzzleConstructFanOutBakeTests.
    public class PuzzleConstructFanOutBakeTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        [Test, Explicit]
        public void Build_AimsDestinationsOntoTheShape()
        {
            PuzzleJobContract job = PuzzleJobContracts.ById("02_04C");
            PuzzleSeed source = job.CreateSeed();
            try
            {
                PuzzleConstructRequest request = Request(job, source, new NoMoves());
                PuzzleConstructedField field = PuzzleConstruct.Build(request);
                Assert.IsNotNull(field);
                CheckAimed(request, field, source.seed);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        private static void CheckAimed(
            PuzzleConstructRequest request, PuzzleConstructedField field, int seed)
        {
            Assert.AreEqual(request.Brief.Ships, field.Ships.Length);
            for (int i = 0; i < field.Ships.Length; i++)
            {
                PuzzleConstructedShip ship = field.Ships[i];
                Assert.AreEqual(
                    field.Planets[ship.DestinationIndex].position, ship.Destination, $"ship {i}");
                Assert.IsTrue(OnShape(request, seed, ship.Destination), $"ship {i}");
            }
        }

        [Test, Explicit]
        public void Spike_TwoShips() => Spike("02_04C");

        [Test, Explicit]
        public void Spike_ThreeShips() => Spike("02_09C");

        [Test, Explicit]
        public void Rate_TwoShips() => Rate("02_04C");

        [Test, Explicit]
        public void Rate_ThreeShips() => Rate("02_09C");

        private static void Spike(string id)
        {
            PuzzleJobContract job = PuzzleJobContracts.ById(id);
            PuzzleSeed source = job.CreateSeed();
            try
            {
                PuzzleBakeResult result = PuzzleConstructBake.Bake(Request(job, source, null));
                TestContext.Progress.WriteLine(
                    $"{id}: {result.Status} {result.Kind} window {result.Window:F3}");
                Assert.IsTrue(result.Baked, $"{id}: {result.Status}");
                Assert.AreEqual(PuzzleSolutionKind.Curve, result.Kind);
                Object.DestroyImmediate(result.Puzzle);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        private static void Rate(string id)
        {
            PuzzleJobContract job = PuzzleJobContracts.ById(id);
            int passed = Passes(job);
            TestContext.Progress.WriteLine($"{id}: {passed}/20 constructed");
            Assert.GreaterOrEqual(passed, 12, $"{id}: {passed}/20");
        }

        private static int Passes(PuzzleJobContract job)
        {
            int passed = 0;
            for (int i = 0; i < 20; i++)
            {
                PuzzleSeed source = job.CreateSeed();
                try
                {
                    source.seed = job.Seed + i;
                    PuzzleBakeResult result = PuzzleConstructBake.Bake(Request(job, source, null));
                    if (!result.Baked)
                    {
                        TestContext.Progress.WriteLine($"{job.Id} fail {result.Status}");
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

            return passed;
        }

        private static bool OnShape(PuzzleConstructRequest request, int seed, Vector3 position)
        {
            int baseSeed = PuzzleConstructSeeds.Value(seed, 0);
            for (int step = -1; step < PuzzleConstructFanOutAim.Steps; step++)
            {
                int value = step < 0 ? baseSeed : PuzzleConstructFanOutAim.ShapeSeed(baseSeed, step);
                PuzzleFieldPlanet[] shape = PuzzleConstructShape.Generate(request, value);
                for (int i = 0; i < shape.Length; i++)
                {
                    if ((shape[i].position - position).sqrMagnitude < 0.000001f)
                        return true;
                }
            }

            return false;
        }

        private static PuzzleConstructRequest Request(
            PuzzleJobContract job, PuzzleSeed source, IPuzzleLesson lesson)
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            PuzzleBrief brief = PuzzleBrief.From(source, settings);
            return new PuzzleConstructRequest
            {
                Contract = job,
                Brief = brief,
                Seed = PuzzleSeedFields.From(source, settings),
                Field = PuzzleFieldSettings.From(settings),
                Sim = PuzzleSimSettings.From(settings),
                Lesson = lesson ?? PuzzleLessonChoice.For(brief)
            };
        }

        private sealed class NoMoves : IPuzzleLesson
        {
            public PuzzleLessonPlan Place(PuzzleLessonContext context) => PuzzleLessonPlan.None;
        }
    }
}
